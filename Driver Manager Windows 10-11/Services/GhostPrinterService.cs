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
    /// <summary>
    /// Detects printer queues/devices that need manual cleanup.
    ///
    /// A "Ghost Printer" is NOT limited to a disconnected PnP device.
    /// This service also detects stale printer queues and duplicate/copy queues
    /// that remain installed in Windows after hardware is removed/reinstalled.
    /// </summary>
    public static class GhostPrinterService
    {
        private const string PrinterClassGuid =
            "{4D36E979-E325-11CE-BFC1-08002BE10318}";

        private sealed class PrinterQueueInfo
        {
            public string Name { get; set; } = "";
            public string DriverName { get; set; } = "";
            public string PortName { get; set; } = "";
            public string PnpDeviceId { get; set; } = "";
            public string DeviceId { get; set; } = "";
            public string Status { get; set; } = "";
            public bool WorkOffline { get; set; }
            public bool Local { get; set; }
            public bool Network { get; set; }
            public bool IsDefault { get; set; }
            public bool Shared { get; set; }
            public string ServerName { get; set; } = "";
            public uint PrinterStatus { get; set; }
            public uint Availability { get; set; }
        }

        private sealed class PnpDeviceInfo
        {
            public string Name { get; set; } = "";
            public string DeviceId { get; set; } = "";
            public string Manufacturer { get; set; } = "";
            public string ClassName { get; set; } = "Printer";
            public string ClassGuid { get; set; } = PrinterClassGuid;
            public uint ProblemCode { get; set; }
        }

        /// <summary>
        /// Returns printer queues/devices that are likely stale, duplicated,
        /// or disconnected. Normal healthy queues are not returned.
        /// </summary>
        public static List<GhostPrinterModel> GetGhostPrinters()
        {
            var result = new List<GhostPrinterModel>();

            List<PnpDeviceInfo> disconnectedDevices = GetDisconnectedPnpPrinters();
            Dictionary<string, string> presentPnpNames = GetPresentPnpDevices();
            List<PrinterQueueInfo> queues = GetInstalledPrinterQueues();
            Dictionary<string, string> driverProviders = GetPrinterDriverProviders();

            // ---------------------------------------------------------
            // 1) Stale / disconnected printer queues
            // ---------------------------------------------------------
            foreach (PrinterQueueInfo queue in queues)
            {
                if (IsVirtualPrinter(queue.Name, queue.DriverName, queue.PortName))
                    continue;

                bool pnpKnown = !string.IsNullOrWhiteSpace(queue.PnpDeviceId);
                bool pnpPresent =
                    pnpKnown &&
                    presentPnpNames.ContainsKey(queue.PnpDeviceId);

                bool localPhysical =
                    queue.Local &&
                    !queue.Network &&
                    IsPhysicalPrinterPort(queue.PortName);

                // Only flag a queue as disconnected when we have a meaningful
                // physical/local correlation. A normal TCP/IP network printer
                // that is simply offline must not become a Ghost Printer.
                bool disconnectedQueue =
                    localPhysical &&
                    pnpKnown &&
                    !pnpPresent;

                // Some old USB queues lose their PNPDeviceID entirely.
                // For these, require a clearly physical port before flagging.
                bool staleQueueWithoutPnpId =
                    localPhysical &&
                    !pnpKnown &&
                    !IsLikelyNetworkPort(queue.PortName);

                if (!disconnectedQueue && !staleQueueWithoutPnpId)
                    continue;

                string provider = ResolveProvider(queue.DriverName, driverProviders);
                string deviceName = ResolveDeviceName(
                    queue.PnpDeviceId,
                    queue.Name,
                    presentPnpNames);

                string state = disconnectedQueue
                    ? "Disconnected"
                    : "Stale Queue";

                string reason = disconnectedQueue
                    ? "Printer queue remains installed, but its physical PnP printer device is no longer present"
                    : "Printer queue remains installed without a current physical PnP device association";

                result.Add(new GhostPrinterModel
                {
                    Name = queue.Name,
                    DriverName = queue.DriverName,
                    PortName = queue.PortName,
                    PrinterType = DetectPrinterType(queue.PortName, queue.Network),
                    DeviceName = deviceName,
                    DeviceId = queue.PnpDeviceId,
                    Manufacturer = provider,
                    State = state,
                    Reason = reason,
                    ActionType = "Queue",
                    ClassGuid = "PrintQueue",
                    Status = string.IsNullOrWhiteSpace(queue.Status)
                        ? state
                        : queue.Status,
                    ProblemCode = 0,
                    Description = reason,
                    IsDefault = queue.IsDefault,
                    IsNetwork = queue.Network,
                    WorkOffline = queue.WorkOffline
                });
            }

            // ---------------------------------------------------------
            // 2) Duplicate / Copy queues
            // ---------------------------------------------------------
            AddDuplicateQueueCandidates(queues, result, driverProviders);

            // ---------------------------------------------------------
            // 3) Disconnected PnP devices that are still registered
            // ---------------------------------------------------------
            foreach (PnpDeviceInfo device in disconnectedDevices)
            {
                // If there is already a queue row carrying the same PNP ID,
                // keep the queue row. The queue action is more useful because
                // it removes the item that remains visible under Printers.
                bool alreadyRepresented = false;

                foreach (GhostPrinterModel item in result)
                {
                    if (!string.IsNullOrWhiteSpace(device.DeviceId) &&
                        !string.IsNullOrWhiteSpace(item.DeviceId) &&
                        device.DeviceId.Equals(
                            item.DeviceId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyRepresented = true;
                        break;
                    }
                }

                if (alreadyRepresented)
                    continue;

                result.Add(new GhostPrinterModel
                {
                    Name = device.Name,
                    DriverName = "",
                    PortName = "",
                    PrinterType = "PnP Printer",
                    DeviceName = device.Name,
                    DeviceId = device.DeviceId,
                    Manufacturer = device.Manufacturer,
                    State = "Disconnected Device",
                    Reason = "Windows still has a disconnected printer PnP device registered",
                    ActionType = "PNP",
                    ClassGuid = string.IsNullOrWhiteSpace(device.ClassName)
                        ? device.ClassGuid
                        : device.ClassName,
                    Status = "Disconnected",
                    ProblemCode = device.ProblemCode,
                    Description = "Disconnected printer PnP device",
                    IsDefault = false,
                    IsNetwork = false,
                    WorkOffline = false
                });
            }

            SortResult(result);
            return result;
        }

        // =============================================================
        // Installed printer queues
        // =============================================================
        private static List<PrinterQueueInfo> GetInstalledPrinterQueues()
        {
            var result = new List<PrinterQueueInfo>();

            using var searcher = new ManagementObjectSearcher(
                @"SELECT Name, DriverName, PortName, PNPDeviceID, DeviceID,
                         PrinterStatus, WorkOffline, Availability,
                         Local, Network, Default, Shared, ServerName, Status
                  FROM Win32_Printer");

            using ManagementObjectCollection printers = searcher.Get();

            foreach (ManagementObject printer in printers)
            {
                string name = GetString(printer, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                result.Add(new PrinterQueueInfo
                {
                    Name = name,
                    DriverName = GetString(printer, "DriverName"),
                    PortName = GetString(printer, "PortName"),
                    PnpDeviceId = GetString(printer, "PNPDeviceID"),
                    DeviceId = GetString(printer, "DeviceID"),
                    PrinterStatus = GetUInt32(printer, "PrinterStatus"),
                    WorkOffline = GetBool(printer, "WorkOffline"),
                    Availability = GetUInt32(printer, "Availability"),
                    Local = GetBool(printer, "Local"),
                    Network = GetBool(printer, "Network"),
                    IsDefault = GetBool(printer, "Default"),
                    Shared = GetBool(printer, "Shared"),
                    ServerName = GetString(printer, "ServerName"),
                    Status = GetString(printer, "Status")
                });
            }

            return result;
        }

        private static Dictionary<string, string> GetPrinterDriverProviders()
        {
            var result = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, DriverName, ProviderName FROM Win32_PrinterDriver");

                using ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject driver in collection)
                {
                    string name = GetString(driver, "Name");
                    string driverName = GetString(driver, "DriverName");
                    string provider = GetString(driver, "ProviderName");

                    if (!string.IsNullOrWhiteSpace(name) &&
                        !result.ContainsKey(name))
                    {
                        result[name] = provider;
                    }

                    if (!string.IsNullOrWhiteSpace(driverName) &&
                        !result.ContainsKey(driverName))
                    {
                        result[driverName] = provider;
                    }
                }
            }
            catch
            {
                // Provider is optional.
            }

            return result;
        }

        // =============================================================
        // Duplicate detection
        // =============================================================
        private static void AddDuplicateQueueCandidates(
            List<PrinterQueueInfo> queues,
            List<GhostPrinterModel> result,
            Dictionary<string, string> driverProviders)
        {
            var candidates = new List<PrinterQueueInfo>();

            foreach (PrinterQueueInfo queue in queues)
            {
                if (IsVirtualPrinter(queue.Name, queue.DriverName, queue.PortName))
                    continue;

                candidates.Add(queue);
            }

            // Group primarily by normalized printer name + driver.
            var nameGroups = new Dictionary<string, List<PrinterQueueInfo>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (PrinterQueueInfo queue in candidates)
            {
                string key =
                    NormalizePrinterBaseName(queue.Name) + "|" +
                    NormalizeDriver(queue.DriverName);

                if (!nameGroups.TryGetValue(key, out List<PrinterQueueInfo>? group))
                {
                    group = new List<PrinterQueueInfo>();
                    nameGroups[key] = group;
                }

                group.Add(queue);
            }

            foreach (List<PrinterQueueInfo> group in nameGroups.Values)
            {
                if (group.Count < 2)
                    continue;

                PrinterQueueInfo canonical = ChooseCanonicalQueue(group);

                foreach (PrinterQueueInfo queue in group)
                {
                    if (ReferenceEquals(queue, canonical))
                        continue;

                    if (!HasCopyLikeSuffix(queue.Name) &&
                        !SamePortAs(queue, canonical))
                    {
                        continue;
                    }

                    AddOrUpgradeDuplicate(
                        result,
                        queue,
                        driverProviders,
                        "Duplicate / Copy",
                        "Duplicate printer queue detected. Another installed queue uses the same printer identity.");
                }
            }

            // Secondary grouping: same driver + same port.
            var portGroups = new Dictionary<string, List<PrinterQueueInfo>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (PrinterQueueInfo queue in candidates)
            {
                string driver = NormalizeDriver(queue.DriverName);
                string port = NormalizePort(queue.PortName);

                if (string.IsNullOrWhiteSpace(driver) ||
                    string.IsNullOrWhiteSpace(port))
                {
                    continue;
                }

                string key = driver + "|" + port;

                if (!portGroups.TryGetValue(key, out List<PrinterQueueInfo>? group))
                {
                    group = new List<PrinterQueueInfo>();
                    portGroups[key] = group;
                }

                group.Add(queue);
            }

            foreach (List<PrinterQueueInfo> group in portGroups.Values)
            {
                if (group.Count < 2)
                    continue;

                PrinterQueueInfo canonical = ChooseCanonicalQueue(group);

                foreach (PrinterQueueInfo queue in group)
                {
                    if (ReferenceEquals(queue, canonical))
                        continue;

                    // Only surface this as a duplicate candidate when the
                    // names are clearly related or the queue has a copy suffix.
                    bool relatedName =
                        NormalizePrinterBaseName(queue.Name).Equals(
                            NormalizePrinterBaseName(canonical.Name),
                            StringComparison.OrdinalIgnoreCase);

                    if (!relatedName && !HasCopyLikeSuffix(queue.Name))
                        continue;

                    AddOrUpgradeDuplicate(
                        result,
                        queue,
                        driverProviders,
                        "Duplicate / Copy",
                        "Another printer queue uses the same driver and printer port.");
                }
            }
        }

        private static void AddOrUpgradeDuplicate(
            List<GhostPrinterModel> result,
            PrinterQueueInfo queue,
            Dictionary<string, string> driverProviders,
            string state,
            string reason)
        {
            GhostPrinterModel? existing = null;

            foreach (GhostPrinterModel item in result)
            {
                if (item.ActionType == "Queue" &&
                    item.Name.Equals(queue.Name, StringComparison.OrdinalIgnoreCase))
                {
                    existing = item;
                    break;
                }
            }

            string provider = ResolveProvider(queue.DriverName, driverProviders);
            string deviceName = string.IsNullOrWhiteSpace(queue.PnpDeviceId)
                ? queue.Name
                : queue.PnpDeviceId;

            if (existing == null)
            {
                result.Add(new GhostPrinterModel
                {
                    Name = queue.Name,
                    DriverName = queue.DriverName,
                    PortName = queue.PortName,
                    PrinterType = DetectPrinterType(queue.PortName, queue.Network),
                    DeviceName = deviceName,
                    DeviceId = queue.PnpDeviceId,
                    Manufacturer = provider,
                    State = state,
                    Reason = reason,
                    ActionType = "Queue",
                    ClassGuid = "PrintQueue",
                    Status = string.IsNullOrWhiteSpace(queue.Status)
                        ? state
                        : queue.Status,
                    ProblemCode = 0,
                    Description = reason,
                    IsDefault = queue.IsDefault,
                    IsNetwork = queue.Network,
                    WorkOffline = queue.WorkOffline
                });

                return;
            }

            existing.State = state;
            existing.Reason =
                existing.Reason.Contains(reason, StringComparison.OrdinalIgnoreCase)
                    ? existing.Reason
                    : existing.Reason + " | " + reason;

            existing.Description = existing.Reason;
        }

        private static PrinterQueueInfo ChooseCanonicalQueue(
            List<PrinterQueueInfo> group)
        {
            foreach (PrinterQueueInfo queue in group)
            {
                if (queue.IsDefault)
                    return queue;
            }

            foreach (PrinterQueueInfo queue in group)
            {
                if (!queue.WorkOffline)
                    return queue;
            }

            // Prefer the shortest base name. Copy 1/2/3 normally become
            // the longer names, so the original queue tends to stay canonical.
            PrinterQueueInfo best = group[0];
            int bestLength = NormalizePrinterBaseName(best.Name).Length;

            foreach (PrinterQueueInfo queue in group)
            {
                int length = NormalizePrinterBaseName(queue.Name).Length;
                if (length < bestLength)
                {
                    best = queue;
                    bestLength = length;
                }
            }

            return best;
        }

        private static bool SamePortAs(
            PrinterQueueInfo a,
            PrinterQueueInfo b)
        {
            return NormalizePort(a.PortName).Equals(
                NormalizePort(b.PortName),
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasCopyLikeSuffix(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return Regex.IsMatch(
                name.Trim(),
                @"(?:\s+Copy(?:\s*\d+)?)$|(?:\s*\(Copy\s*\d*\))$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string NormalizePrinterBaseName(string name)
        {
            string value = name?.Trim() ?? "";

            if (value.Length == 0)
                return "";

            for (int i = 0; i < 3; i++)
            {
                value = Regex.Replace(
                    value,
                    @"\s+Copy(?:\s*\d+)?$",
                    "",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                value = Regex.Replace(
                    value,
                    @"\s*\(Copy\s*\d*\)$",
                    "",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            }

            return NormalizeText(value);
        }

        private static string NormalizeDriver(string driverName)
        {
            return NormalizeText(driverName);
        }

        private static string NormalizePort(string portName)
        {
            return NormalizeText(portName);
        }

        private static string NormalizeText(string value)
        {
            return Regex.Replace(
                value ?? "",
                @"\s+",
                " ",
                RegexOptions.CultureInvariant).Trim();
        }

        // =============================================================
        // PnP presence
        // =============================================================
        private static Dictionary<string, string> GetPresentPnpDevices()
        {
            var result = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"SELECT Name, DeviceID, PNPClass, ClassGuid, ConfigManagerErrorCode
                      FROM Win32_PnPEntity
                      WHERE PNPClass = 'Printer'
                         OR ClassGuid = '{4D36E979-E325-11CE-BFC1-08002BE10318}'");

                using ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject device in collection)
                {
                    string id = GetString(device, "DeviceID");
                    string name = GetString(device, "Name");
                    uint problem = GetUInt32(device, "ConfigManagerErrorCode");

                    if (string.IsNullOrWhiteSpace(id) || problem != 0)
                        continue;

                    result[id] = name;
                }
            }
            catch
            {
                // PnP correlation is advisory.
            }

            return result;
        }

        private static string ResolveDeviceName(
            string deviceId,
            string fallbackName,
            Dictionary<string, string> presentPnpNames)
        {
            if (!string.IsNullOrWhiteSpace(deviceId) &&
                presentPnpNames.TryGetValue(deviceId, out string? presentName) &&
                !string.IsNullOrWhiteSpace(presentName))
            {
                return presentName;
            }

            return string.IsNullOrWhiteSpace(deviceId)
                ? fallbackName
                : deviceId;
        }

        // =============================================================
        // Disconnected PnP devices via pnputil
        // =============================================================
        private static List<PnpDeviceInfo> GetDisconnectedPnpPrinters()
        {
            string pnputilPath = Path.Combine(
                Environment.SystemDirectory,
                "pnputil.exe");

            if (!File.Exists(pnputilPath))
                return new List<PnpDeviceInfo>();

            string output = RunPnpUtil(pnputilPath);

            if (string.IsNullOrWhiteSpace(output))
                return new List<PnpDeviceInfo>();

            return ParsePnpOutput(output);
        }

        private static string RunPnpUtil(string pnputilPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = pnputilPath,
                Arguments = $"/enum-devices /disconnected /class \"{PrinterClassGuid}\"",
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

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();

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

            if (process.ExitCode != 0)
                return "";

            string combined = string.IsNullOrWhiteSpace(output)
                ? error
                : output;

            if (ContainsNoMatchingDevices(combined))
                return "";

            return output.Trim();
        }

        private static List<PnpDeviceInfo> ParsePnpOutput(string output)
        {
            var result = new List<PnpDeviceInfo>();

            string normalized = output
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            if (ContainsNoMatchingDevices(normalized))
                return result;

            string[] blocks = Regex.Split(
                normalized.Trim(),
                @"\n\s*\n");

            foreach (string block in blocks)
            {
                PnpDeviceInfo? item = ParsePnpBlock(block);
                if (item == null)
                    continue;

                bool duplicate = false;
                foreach (PnpDeviceInfo existing in result)
                {
                    if (!string.IsNullOrWhiteSpace(item.DeviceId) &&
                        existing.DeviceId.Equals(
                            item.DeviceId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                    result.Add(item);
            }

            return result;
        }

        private static PnpDeviceInfo? ParsePnpBlock(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            string[] lines = block.Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries);

            string instanceId = "";
            string description = "";
            string manufacturer = "";
            string className = "Printer";
            string classGuid = PrinterClassGuid;
            uint problemCode = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                if (IsNoiseLine(line))
                    continue;

                int colon = line.IndexOf(':');
                if (colon < 0)
                    continue;

                string label = line[..colon].Trim();
                string value = line[(colon + 1)..].Trim();

                if (label.Equals("Instance ID", StringComparison.OrdinalIgnoreCase))
                    instanceId = value;
                else if (label.Equals("Device Description", StringComparison.OrdinalIgnoreCase))
                    description = value;
                else if (label.Equals("Manufacturer Name", StringComparison.OrdinalIgnoreCase))
                    manufacturer = value;
                else if (label.Equals("Class Name", StringComparison.OrdinalIgnoreCase))
                    className = value;
                else if (label.Equals("Class GUID", StringComparison.OrdinalIgnoreCase))
                    classGuid = value;
                else if (label.Equals("Problem Code", StringComparison.OrdinalIgnoreCase))
                    problemCode = ParseProblemCode(value);
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

            if (string.IsNullOrWhiteSpace(description))
            {
                foreach (string rawLine in lines)
                {
                    string value = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(value) ||
                        IsNoiseLine(value) ||
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

            return new PnpDeviceInfo
            {
                Name = string.IsNullOrWhiteSpace(description)
                    ? "Unknown Printer Device"
                    : description,
                DeviceId = instanceId,
                Manufacturer = manufacturer,
                ClassName = className,
                ClassGuid = classGuid,
                ProblemCode = problemCode
            };
        }

        // =============================================================
        // Classification helpers
        // =============================================================
        private static string ResolveProvider(
            string driverName,
            Dictionary<string, string> providerMap)
        {
            if (!string.IsNullOrWhiteSpace(driverName) &&
                providerMap.TryGetValue(driverName, out string? provider) &&
                !string.IsNullOrWhiteSpace(provider))
            {
                return provider;
            }

            if (string.IsNullOrWhiteSpace(driverName))
                return "Unknown";

            string[] vendors =
            {
                "HP", "Canon", "Epson", "Brother", "Xerox", "Ricoh",
                "Kyocera", "Konica Minolta", "Lexmark", "Fuji Xerox", "Pantum"
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

        private static string DetectPrinterType(string portName, bool network)
        {
            if (network || IsLikelyNetworkPort(portName))
            {
                if (portName.Contains("WSD", StringComparison.OrdinalIgnoreCase))
                    return "WSD";

                if (portName.Contains("IPP", StringComparison.OrdinalIgnoreCase))
                    return "IPP";

                return "Network";
            }

            if (portName.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
                return "USB";

            if (portName.StartsWith("DOT4", StringComparison.OrdinalIgnoreCase))
                return "DOT4";

            if (portName.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
                return "LPT";

            if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                return "COM";

            return "Local";
        }

        private static bool IsPhysicalPrinterPort(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            string value = portName.Trim();

            return value.StartsWith("USB", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("DOT4", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("WSD", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("COM", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLikelyNetworkPort(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            string value = portName.Trim();

            if (value.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.StartsWith("IP_", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("TCPIP_", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("WSD", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return Regex.IsMatch(
                value,
                @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$",
                RegexOptions.CultureInvariant);
        }

        private static bool IsVirtualPrinter(
            string printerName,
            string driverName,
            string portName)
        {
            string combined =
                $"{printerName} {driverName} {portName}";

            string[] virtualNames =
            {
                "Microsoft Print to PDF",
                "Microsoft XPS Document Writer",
                "Fax",
                "OneNote",
                "Send To OneNote",
                "Microsoft Shared Fax Driver",
                "PORTPROMPT:",
                "FILE:",
                "NUL:",
                "XPSPort:"
            };

            foreach (string value in virtualNames)
            {
                if (combined.Contains(value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        // =============================================================
        // Generic helpers
        // =============================================================
        private static bool IsNoiseLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            string text = value.Trim();

            return text.Equals(
                       "Microsoft PnP Utility",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   ContainsNoMatchingDevices(text);
        }

        private static bool ContainsNoMatchingDevices(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            return text.Contains("No matching devices found", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("No devices found", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("No devices were found", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeInstanceId(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && value.Contains('\\')
                   && value.Length >= 5
                   && !LooksLikeGuid(value);
        }

        private static bool LooksLikeGuid(string value)
        {
            return Regex.IsMatch(
                value ?? "",
                @"^\{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\}$",
                RegexOptions.CultureInvariant);
        }

        private static bool LooksLikeProblemCode(string value)
        {
            return Regex.IsMatch(
                value ?? "",
                @"^\d+\s*\(0x[0-9A-Fa-f]+\)",
                RegexOptions.CultureInvariant);
        }

        private static uint ParseProblemCode(string value)
        {
            Match match = Regex.Match(value ?? "", @"^\s*(\d+)");

            return match.Success &&
                   uint.TryParse(match.Groups[1].Value, out uint result)
                ? result
                : 0;
        }

        private static string GetString(ManagementObject obj, string property)
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

        private static bool GetBool(ManagementObject obj, string property)
        {
            try
            {
                object? value = obj[property];
                return value is bool b && b;
            }
            catch
            {
                return false;
            }
        }

        private static uint GetUInt32(ManagementObject obj, string property)
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

        private static void SortResult(List<GhostPrinterModel> result)
        {
            result.Sort((a, b) =>
            {
                int state = GetStateOrder(a.State).CompareTo(GetStateOrder(b.State));
                if (state != 0)
                    return state;

                return string.Compare(
                    a.Name,
                    b.Name,
                    StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int GetStateOrder(string state)
        {
            if (state.Equals("Duplicate / Copy", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (state.Equals("Stale Queue", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (state.Equals("Disconnected", StringComparison.OrdinalIgnoreCase))
                return 2;

            if (state.Equals("Disconnected Device", StringComparison.OrdinalIgnoreCase))
                return 3;

            return 4;
        }
    }
}
