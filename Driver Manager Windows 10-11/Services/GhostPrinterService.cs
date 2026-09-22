using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class GhostPrinterService
    {
        private const string PrinterClassGuid =
            "{4D36E979-E325-11CE-BFC1-08002BE10318}";

        private static readonly string[] VirtualPortNames =
        {
            "PORTPROMPT:",
            "FILE:",
            "NUL:",
            "XPSPort:",
            "OneNote:",
            "Microsoft XPS Document Writer",
            "Microsoft Print to PDF"
        };

        /// <summary>
        /// Detect printer entries that are not currently present/usable.
        /// Two sources are checked:
        /// 1) PnP printer devices reported by pnputil as disconnected.
        /// 2) Installed printer queues that are explicitly offline/error,
        ///    or local physical printer queues whose PnP device is no longer present.
        /// </summary>
        public static List<GhostPrinterModel> GetGhostPrinters()
        {
            var result = new List<GhostPrinterModel>();

            // 1. Non-present PnP printer devices.
            try
            {
                foreach (GhostPrinterModel item in GetDisconnectedPnpPrinters())
                    AddUnique(result, item);
            }
            catch
            {
                // Continue with printer queue detection.
            }

            // 2. Installed printer queues that look inactive/non-present.
            try
            {
                foreach (GhostPrinterModel item in GetInactivePrinterQueues())
                    AddUnique(result, item);
            }
            catch
            {
                // Keep the page usable even when WMI access is restricted.
            }

            result.Sort((a, b) => string.Compare(
                a.Name,
                b.Name,
                StringComparison.OrdinalIgnoreCase));

            return result;
        }

        // =========================================================
        // Installed printer queues
        // =========================================================
        private static List<GhostPrinterModel> GetInactivePrinterQueues()
        {
            var result = new List<GhostPrinterModel>();

            var providerMap = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var driverSearcher = new ManagementObjectSearcher(
                    @"SELECT Name, DriverName, ProviderName FROM Win32_PrinterDriver");

                using ManagementObjectCollection drivers = driverSearcher.Get();

                foreach (ManagementObject driver in drivers)
                {
                    string driverName = GetString(driver, "Name");
                    if (string.IsNullOrWhiteSpace(driverName))
                        driverName = GetString(driver, "DriverName");

                    string provider = GetString(driver, "ProviderName");

                    if (!string.IsNullOrWhiteSpace(driverName) &&
                        !providerMap.ContainsKey(driverName))
                    {
                        providerMap[driverName] = provider;
                    }
                }
            }
            catch
            {
                // Provider is optional.
            }

            // Build a list of currently present PnP printer devices.
            var presentPnpIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var presentPrinterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var pnpSearcher = new ManagementObjectSearcher(
                    @"SELECT Name, DeviceID, ClassGuid, PNPClass, ConfigManagerErrorCode
                      FROM Win32_PnPEntity
                      WHERE PNPClass = 'Printer'
                         OR ClassGuid = '{4D36E979-E325-11CE-BFC1-08002BE10318}'");

                using ManagementObjectCollection pnpDevices = pnpSearcher.Get();

                foreach (ManagementObject device in pnpDevices)
                {
                    string deviceId = GetString(device, "DeviceID");
                    string name = GetString(device, "Name");
                    uint configError = GetUInt32(device, "ConfigManagerErrorCode");

                    if (!string.IsNullOrWhiteSpace(deviceId))
                        presentPnpIds.Add(deviceId);

                    if (!string.IsNullOrWhiteSpace(name) && configError == 0)
                        presentPrinterNames.Add(name);
                }
            }
            catch
            {
                // PnP correlation is optional.
            }

            using var searcher = new ManagementObjectSearcher(
                @"SELECT Name, DeviceID, PNPDeviceID, DriverName, PortName,
                         PrinterStatus, WorkOffline, Availability,
                         ConfigManagerErrorCode, Local, Network,
                         ServerName, Shared, Status, StatusInfo
                  FROM Win32_Printer");

            using ManagementObjectCollection printers = searcher.Get();

            foreach (ManagementObject printer in printers)
            {
                string name = GetString(printer, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                bool workOffline = GetBool(printer, "WorkOffline");
                bool local = GetBool(printer, "Local");
                bool network = GetBool(printer, "Network");

                uint printerStatus = GetUInt32(printer, "PrinterStatus");
                uint availability = GetUInt32(printer, "Availability");
                uint configError = GetUInt32(printer, "ConfigManagerErrorCode");

                string statusText = GetString(printer, "Status");
                string portName = GetString(printer, "PortName");
                string pnpDeviceId = GetString(printer, "PNPDeviceID");
                string deviceId = GetString(printer, "DeviceID");
                string driverName = GetString(printer, "DriverName");

                bool explicitlyInactive = IsExplicitlyInactive(
                    workOffline,
                    printerStatus,
                    availability,
                    configError,
                    statusText);

                bool physicalLocalDeviceMissing = false;

                // A local physical printer with a USB/DOT4/WSD/TCP/IP style
                // port but no longer-present PnP device is a common "ghost"
                // queue scenario. Do not flag virtual printer ports.
                if (local && !network &&
                    string.IsNullOrWhiteSpace(pnpDeviceId) &&
                    IsPhysicalPrinterPort(portName) &&
                    !IsVirtualPrinter(name, driverName, portName))
                {
                    physicalLocalDeviceMissing =
                        !presentPrinterNames.Contains(name);
                }
                else if (local && !network &&
                         !string.IsNullOrWhiteSpace(pnpDeviceId) &&
                         IsPhysicalPrinterPort(portName) &&
                         presentPnpIds.Count > 0)
                {
                    physicalLocalDeviceMissing =
                        !presentPnpIds.Contains(pnpDeviceId);
                }

                if (!explicitlyInactive && !physicalLocalDeviceMissing)
                    continue;

                string provider = "";
                if (!string.IsNullOrWhiteSpace(driverName) &&
                    providerMap.TryGetValue(driverName, out string? mappedProvider))
                {
                    provider = mappedProvider ?? "";
                }

                if (string.IsNullOrWhiteSpace(provider))
                    provider = GuessProvider(driverName);

                string resolvedStatus = physicalLocalDeviceMissing
                    ? "Not Present"
                    : ResolvePrinterStatus(
                        workOffline,
                        printerStatus,
                        availability,
                        configError,
                        statusText);

                string reason = physicalLocalDeviceMissing
                    ? "Installed printer queue found, but the physical PnP printer device is not present"
                    : BuildDetectionReason(
                        workOffline,
                        printerStatus,
                        availability,
                        configError,
                        statusText);

                string className =
                    name.Contains("Fax", StringComparison.OrdinalIgnoreCase)
                        ? "Fax / PrintQueue"
                        : "PrintQueue";

                result.Add(new GhostPrinterModel
                {
                    Name = name,
                    DeviceId = string.IsNullOrWhiteSpace(pnpDeviceId)
                        ? deviceId
                        : pnpDeviceId,
                    Manufacturer = provider,
                    ClassGuid = className,
                    Status = resolvedStatus,
                    ProblemCode = configError,
                    Description = reason
                });
            }

            return result;
        }

        private static bool IsExplicitlyInactive(
            bool workOffline,
            uint printerStatus,
            uint availability,
            uint configError,
            string statusText)
        {
            if (workOffline)
                return true;

            // Win32_Printer: PrinterStatus 7 = Offline.
            if (printerStatus == 7)
                return true;

            // Win32_Printer: Availability 9 = Offline.
            if (availability == 9)
                return true;

            if (configError != 0)
                return true;

            if (!string.IsNullOrWhiteSpace(statusText))
            {
                if (statusText.Contains("offline", StringComparison.OrdinalIgnoreCase) ||
                    statusText.Contains("stopped", StringComparison.OrdinalIgnoreCase) ||
                    statusText.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPhysicalPrinterPort(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            string value = portName.Trim();

            if (value.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("DOT4", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("WSD", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("IP_", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("TCPIP_", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                return true;

            // Standard TCP/IP ports can be named by raw IPv4/IPv6 host names.
            if (Regex.IsMatch(value, @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$"))
                return true;

            return false;
        }

        private static bool IsVirtualPrinter(
            string printerName,
            string driverName,
            string portName)
        {
            foreach (string virtualName in VirtualPortNames)
            {
                if (printerName.Contains(virtualName, StringComparison.OrdinalIgnoreCase) ||
                    driverName.Contains(virtualName, StringComparison.OrdinalIgnoreCase) ||
                    portName.Contains(virtualName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            string[] virtualPrinterNames =
            {
                "Microsoft Print to PDF",
                "Microsoft XPS Document Writer",
                "Fax",
                "OneNote",
                "Send To OneNote",
                "Microsoft Shared Fax Driver"
            };

            foreach (string virtualName in virtualPrinterNames)
            {
                if (printerName.Contains(virtualName, StringComparison.OrdinalIgnoreCase) ||
                    driverName.Contains(virtualName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolvePrinterStatus(
            bool workOffline,
            uint printerStatus,
            uint availability,
            uint configError,
            string statusText)
        {
            if (configError != 0)
                return "Problem";

            if (workOffline || printerStatus == 7 || availability == 9)
                return "Offline";

            if (!string.IsNullOrWhiteSpace(statusText))
                return statusText;

            return "Inactive";
        }

        private static string BuildDetectionReason(
            bool workOffline,
            uint printerStatus,
            uint availability,
            uint configError,
            string statusText)
        {
            if (configError != 0)
                return $"Device configuration problem (Code {configError})";

            if (workOffline)
                return "Printer queue is set to Work Offline";

            if (printerStatus == 7 || availability == 9)
                return "Windows reports the printer as Offline";

            if (!string.IsNullOrWhiteSpace(statusText) &&
                (statusText.Contains("stopped", StringComparison.OrdinalIgnoreCase) ||
                 statusText.Contains("error", StringComparison.OrdinalIgnoreCase)))
            {
                return $"Windows reports printer status: {statusText}";
            }

            return "Printer queue is not currently active";
        }

        private static string GuessProvider(string driverName)
        {
            if (string.IsNullOrWhiteSpace(driverName))
                return "Unknown";

            string[] vendors =
            {
                "HP",
                "Canon",
                "Epson",
                "Brother",
                "Xerox",
                "Ricoh",
                "Kyocera",
                "Konica Minolta",
                "Lexmark",
                "Fuji Xerox",
                "Pantum"
            };

            foreach (string vendor in vendors)
            {
                if (driverName.Contains(vendor, StringComparison.OrdinalIgnoreCase))
                    return vendor;
            }

            if (driverName.Contains("Microsoft", StringComparison.OrdinalIgnoreCase))
                return "Microsoft";

            return "Unknown";
        }

        // =========================================================
        // Disconnected PnP devices via pnputil
        // =========================================================
        private static List<GhostPrinterModel> GetDisconnectedPnpPrinters()
        {
            string pnputilPath = Path.Combine(
                Environment.SystemDirectory,
                "pnputil.exe");

            if (!File.Exists(pnputilPath))
                return new List<GhostPrinterModel>();

            string output = RunPnpUtil(pnputilPath);

            if (string.IsNullOrWhiteSpace(output))
                return new List<GhostPrinterModel>();

            return ParsePnpOutput(output);
        }

        private static string RunPnpUtil(string pnputilPath)
        {
            string arguments =
                $"/enum-devices /disconnected /class \"{PrinterClassGuid}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = pnputilPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.Default,
                StandardErrorEncoding = Encoding.Default
            };

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync();

            Task<string> errorTask =
                process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                }

                return "";
            }

            string output = outputTask.GetAwaiter().GetResult();
            string error = errorTask.GetAwaiter().GetResult();

            string combined = string.IsNullOrWhiteSpace(output)
                ? error
                : output;

            // IMPORTANT: pnputil can return exit code 0 while printing a
            // "no devices" message. Never parse that message as a device.
            if (ContainsNoMatchingDevices(combined))
                return "";

            if (process.ExitCode != 0)
                return "";

            return output.Trim();
        }

        private static List<GhostPrinterModel> ParsePnpOutput(string output)
        {
            var result = new List<GhostPrinterModel>();

            if (string.IsNullOrWhiteSpace(output))
                return result;

            string normalized = output
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            // Guard against command headers/no-device messages appearing
            // together with the output.
            if (ContainsNoMatchingDevices(normalized))
                return result;

            string[] blocks = Regex.Split(
                normalized.Trim(),
                @"\n\s*\n");

            foreach (string block in blocks)
            {
                if (IsPnpHeaderOrNoise(block))
                    continue;

                GhostPrinterModel? item = ParsePnpBlock(block);

                if (item == null)
                    continue;

                AddUnique(result, item);
            }

            return result;
        }

        private static bool IsPnpHeaderOrNoise(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return true;

            string value = block.Trim();

            if (value.Equals("Microsoft PnP Utility", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("Microsoft PnP Utility", StringComparison.OrdinalIgnoreCase) &&
                !value.Contains("Instance ID:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ContainsNoMatchingDevices(value);
        }

        private static GhostPrinterModel? ParsePnpBlock(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            string[] lines = block.Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries);

            string? instanceId = null;
            string? description = null;
            string? manufacturer = null;
            string? className = null;
            string? classGuid = null;
            uint problemCode = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                int colon = line.IndexOf(':');
                if (colon < 0)
                    continue;

                string label = line[..colon].Trim();
                string value = line[(colon + 1)..].Trim();

                if (value.Length == 0)
                    continue;

                if (label.Equals("Instance ID", StringComparison.OrdinalIgnoreCase))
                {
                    instanceId = value;
                }
                else if (label.Equals("Device Description", StringComparison.OrdinalIgnoreCase))
                {
                    description = value;
                }
                else if (label.Equals("Manufacturer Name", StringComparison.OrdinalIgnoreCase))
                {
                    manufacturer = value;
                }
                else if (label.Equals("Class Name", StringComparison.OrdinalIgnoreCase))
                {
                    className = value;
                }
                else if (label.Equals("Class GUID", StringComparison.OrdinalIgnoreCase))
                {
                    classGuid = value;
                }
                else if (label.Equals("Problem Code", StringComparison.OrdinalIgnoreCase))
                {
                    problemCode = ParseProblemCode(value);
                }
            }

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                foreach (string rawLine in lines)
                {
                    string value = rawLine.Trim();
                    if (LooksLikeInstanceId(value))
                    {
                        instanceId = value;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(classGuid))
            {
                foreach (string rawLine in lines)
                {
                    string value = rawLine.Trim();
                    if (LooksLikeGuid(value))
                    {
                        classGuid = value;
                        break;
                    }
                }
            }

            // Never use the command header/no-device message as the printer name.
            if (string.IsNullOrWhiteSpace(description))
            {
                foreach (string rawLine in lines)
                {
                    string value = rawLine.Trim();

                    if (string.IsNullOrWhiteSpace(value) ||
                        value.Equals("Microsoft PnP Utility", StringComparison.OrdinalIgnoreCase) ||
                        ContainsNoMatchingDevices(value) ||
                        LooksLikeInstanceId(value) ||
                        LooksLikeGuid(value) ||
                        LooksLikeProblemCode(value))
                    {
                        continue;
                    }

                    description = value;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(instanceId) &&
                string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            return new GhostPrinterModel
            {
                Name = string.IsNullOrWhiteSpace(description)
                    ? "Unknown Printer Device"
                    : description,
                DeviceId = instanceId ?? "",
                Manufacturer = manufacturer ?? "",
                ClassGuid = string.IsNullOrWhiteSpace(className)
                    ? (classGuid ?? "Printer")
                    : className,
                Status = "Disconnected",
                ProblemCode = problemCode,
                Description = "PnP printer device is disconnected"
            };
        }

        // =========================================================
        // Helpers
        // =========================================================
        private static void AddUnique(
            List<GhostPrinterModel> list,
            GhostPrinterModel item)
        {
            foreach (GhostPrinterModel existing in list)
            {
                if (!string.IsNullOrWhiteSpace(item.DeviceId) &&
                    existing.DeviceId.Equals(
                        item.DeviceId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (existing.Name.Equals(
                        item.Name,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(existing.DeviceId) ==
                    string.IsNullOrWhiteSpace(item.DeviceId))
                {
                    return;
                }
            }

            list.Add(item);
        }

        private static bool ContainsNoMatchingDevices(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            string normalized = text.Trim();

            return normalized.Contains(
                       "No matching devices found",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   normalized.Contains(
                       "No devices found",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   normalized.Contains(
                       "No devices were found",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeInstanceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Contains('\\') &&
                   !LooksLikeGuid(value) &&
                   value.Length >= 5;
        }

        private static bool LooksLikeGuid(string value)
        {
            return Regex.IsMatch(
                value,
                @"^\{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\}$");
        }

        private static bool LooksLikeProblemCode(string value)
        {
            return Regex.IsMatch(
                value,
                @"^\d+\s*\(0x[0-9A-Fa-f]+\)");
        }

        private static uint ParseProblemCode(string value)
        {
            Match match = Regex.Match(
                value ?? "",
                @"^\s*(\d+)");

            return match.Success &&
                   uint.TryParse(
                       match.Groups[1].Value,
                       out uint result)
                ? result
                : 0;
        }

        private static string GetString(
            ManagementObject obj,
            string property)
        {
            try
            {
                return Convert.ToString(obj[property])?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static bool GetBool(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value = obj[property];
                return value is bool boolValue && boolValue;
            }
            catch
            {
                return false;
            }
        }

        private static uint GetUInt32(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value = obj[property];
                return value == null ? 0 : Convert.ToUInt32(value);
            }
            catch
            {
                return 0;
            }
        }
    }
}
