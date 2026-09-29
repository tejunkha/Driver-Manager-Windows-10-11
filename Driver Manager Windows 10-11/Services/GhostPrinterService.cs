using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Ghost Printer scanner.
    ///
    /// Important design:
    /// - Get-Printer is the primary source for installed printer queues.
    /// - Win32_Printer is used only to enrich queue information such as PNP ID.
    /// - Win32_PnPEntity also includes Printer and PrintQueue classes so stale
    ///   PnP print entries can be found even when they are not in Win32_Printer.
    /// - Offline alone is NOT considered Ghost. Duplicate/copy queues,
    ///   missing ports, and disconnected PnP entries are.
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
            public string DeviceUrl { get; set; } = "";
            public string ComputerName { get; set; } = "";
            public string Type { get; set; } = "";
            public string Status { get; set; } = "";
            public string PnpDeviceId { get; set; } = "";
            public bool WorkOffline { get; set; }
            public bool Local { get; set; }
            public bool Network { get; set; }
            public bool IsDefault { get; set; }
            public bool Shared { get; set; }
            public bool Published { get; set; }
            public uint PrinterStatus { get; set; }
        }

        private sealed class PnpDeviceInfo
        {
            public string Name { get; set; } = "";
            public string DeviceId { get; set; } = "";
            public string Manufacturer { get; set; } = "";
            public string ClassName { get; set; } = "";
            public string ClassGuid { get; set; } = "";
            public bool Present { get; set; }
            public uint ProblemCode { get; set; }
            public string Status { get; set; } = "";
        }

        public static List<GhostPrinterModel> GetGhostPrinters()
        {
            var result = new List<GhostPrinterModel>();

            // ---------------------------------------------------------
            // 1. Installed print queues - primary source
            // ---------------------------------------------------------
            List<PrinterQueueInfo> queues = GetInstalledPrinterQueues();

            // ---------------------------------------------------------
            // 2. WMI enrichment: PNPDeviceID / Local / Network / Status
            // ---------------------------------------------------------
            Dictionary<string, PrinterQueueInfo> wmiQueues =
                GetWmiPrinterQueues();

            foreach (PrinterQueueInfo queue in queues)
            {
                if (wmiQueues.TryGetValue(
                        queue.Name,
                        out PrinterQueueInfo? wmi))
                {
                    if (string.IsNullOrWhiteSpace(queue.PnpDeviceId))
                        queue.PnpDeviceId = wmi.PnpDeviceId;

                    if (string.IsNullOrWhiteSpace(queue.Status))
                        queue.Status = wmi.Status;

                    queue.Local = wmi.Local;
                    queue.Network = wmi.Network;
                }
            }

            // If Get-Printer is unavailable, WMI still gives us queues.
            if (queues.Count == 0)
            {
                queues.AddRange(wmiQueues.Values);
            }

            // ---------------------------------------------------------
            // 3. Current / stale PnP printer devices
            // ---------------------------------------------------------
            List<PnpDeviceInfo> pnpDevices = GetAllPrinterPnpDevices();

            var pnpById = new Dictionary<string, PnpDeviceInfo>(
                StringComparer.OrdinalIgnoreCase);

            foreach (PnpDeviceInfo device in pnpDevices)
            {
                if (!string.IsNullOrWhiteSpace(device.DeviceId) &&
                    !pnpById.ContainsKey(device.DeviceId))
                {
                    pnpById.Add(device.DeviceId, device);
                }
            }

            // ---------------------------------------------------------
            // 4. Detect queue problems
            // ---------------------------------------------------------
            Dictionary<string, string> driverProviders =
                GetPrinterDriverProviders();

            HashSet<string> installedPorts =
                GetInstalledPrinterPorts();

            foreach (PrinterQueueInfo queue in queues)
            {
                if (IsVirtualPrinter(
                        queue.Name,
                        queue.DriverName,
                        queue.PortName))
                {
                    continue;
                }

                // A direct match is the strongest PnP correlation.
                PnpDeviceInfo? matchedPnp = null;
                if (!string.IsNullOrWhiteSpace(queue.PnpDeviceId))
                {
                    pnpById.TryGetValue(
                        queue.PnpDeviceId,
                        out matchedPnp);
                }

                bool hasPhysicalPort =
                    IsPhysicalPrinterPort(queue.PortName);

                bool isConnection =
                    IsNetworkConnection(queue);

                // A physical queue whose referenced PnP device is known but
                // no longer present is a disconnected queue.
                bool disconnectedByPnp =
                    !isConnection &&
                    hasPhysicalPort &&
                    matchedPnp != null &&
                    !matchedPnp.Present;

                // Some old queues lose their PNPDeviceID completely. If the
                // queue still references a physical USB/WSD/DOT4/TCPIP port
                // that is no longer installed, treat it as a stale queue.
                bool missingPhysicalPort =
                    !isConnection &&
                    hasPhysicalPort &&
                    !string.IsNullOrWhiteSpace(queue.PortName) &&
                    installedPorts.Count > 0 &&
                    !installedPorts.Contains(queue.PortName);

                // No PNP ID + physical queue is still a useful stale candidate
                // when the current PnP database contains no matching name.
                bool staleWithoutPnp =
                    !isConnection &&
                    hasPhysicalPort &&
                    string.IsNullOrWhiteSpace(queue.PnpDeviceId) &&
                    !HasPresentPnpName(queue.Name, pnpDevices);

                if (disconnectedByPnp)
                {
                    AddQueueResult(
                        result,
                        queue,
                        driverProviders,
                        "Disconnected",
                        "Printer queue remains installed, but its physical PnP printer device is not present.");
                }
                else if (missingPhysicalPort)
                {
                    AddQueueResult(
                        result,
                        queue,
                        driverProviders,
                        "Stale Queue",
                        $"Printer queue references port '{queue.PortName}', but that printer port is no longer installed.");
                }
                else if (staleWithoutPnp)
                {
                    AddQueueResult(
                        result,
                        queue,
                        driverProviders,
                        "Stale Queue",
                        "Printer queue remains installed but has no current physical PnP printer association.");
                }
            }

            // ---------------------------------------------------------
            // 5. Duplicate / Copy queues
            // ---------------------------------------------------------
            DetectDuplicateQueues(
                queues,
                result,
                driverProviders);

            // ---------------------------------------------------------
            // 6. PnP-only stale print devices
            // ---------------------------------------------------------
            foreach (PnpDeviceInfo device in pnpDevices)
            {
                if (device.Present)
                    continue;

                if (string.IsNullOrWhiteSpace(device.DeviceId))
                    continue;

                if (AlreadyRepresentedByDeviceId(
                        result,
                        device.DeviceId))
                {
                    continue;
                }

                result.Add(new GhostPrinterModel
                {
                    Name = string.IsNullOrWhiteSpace(device.Name)
                        ? "Unknown Printer Device"
                        : device.Name,
                    DriverName = "",
                    PortName = "",
                    PrinterType = string.IsNullOrWhiteSpace(device.ClassName)
                        ? "Printer"
                        : device.ClassName,
                    DeviceName = device.Name,
                    DeviceId = device.DeviceId,
                    Manufacturer = device.Manufacturer,
                    State = "Disconnected Device",
                    Reason = "Windows still has a non-present Printer / PrintQueue PnP device registered.",
                    ActionType = "PNP",
                    ClassGuid = string.IsNullOrWhiteSpace(device.ClassName)
                        ? device.ClassGuid
                        : device.ClassName,
                    Status = string.IsNullOrWhiteSpace(device.Status)
                        ? "Disconnected"
                        : device.Status,
                    ProblemCode = device.ProblemCode,
                    Description = "Disconnected Printer / PrintQueue PnP device",
                    IsDefault = false,
                    IsNetwork = false,
                    WorkOffline = false
                });
            }

            SortResult(result);
            return result;
        }

        // =============================================================
        // PrintManagement: Get-Printer
        // =============================================================
        private static List<PrinterQueueInfo> GetInstalledPrinterQueues()
        {
            var result = new List<PrinterQueueInfo>();

            const string script = @"
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Import-Module PrintManagement -ErrorAction Stop

Get-Printer -Full |
    Select-Object Name,
                  DriverName,
                  PortName,
                  ComputerName,
                  Type,
                  PrinterStatus,
                  WorkOffline,
                  Shared,
                  Published,
                  Default,
                  DeviceURL |
    ConvertTo-Json -Depth 4 -Compress
";

            try
            {
                string json = RunPowerShell(script, 20_000);

                if (string.IsNullOrWhiteSpace(json))
                    return result;

                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        AddPowerShellQueue(item, result);
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    AddPowerShellQueue(root, result);
                }
            }
            catch
            {
                // WMI fallback is handled by the caller.
            }

            return result;
        }

        private static void AddPowerShellQueue(
            JsonElement item,
            List<PrinterQueueInfo> result)
        {
            string name = JsonString(item, "Name");
            if (string.IsNullOrWhiteSpace(name))
                return;

            result.Add(new PrinterQueueInfo
            {
                Name = name,
                DriverName = JsonString(item, "DriverName"),
                PortName = JsonString(item, "PortName"),
                ComputerName = JsonString(item, "ComputerName"),
                Type = JsonString(item, "Type"),
                DeviceUrl = JsonString(item, "DeviceURL"),
                Status = JsonString(item, "PrinterStatus"),
                PrinterStatus = JsonUInt32(item, "PrinterStatus"),
                WorkOffline = JsonBool(item, "WorkOffline"),
                Shared = JsonBool(item, "Shared"),
                Published = JsonBool(item, "Published"),
                IsDefault = JsonBool(item, "Default"),
                Network = IsNetworkPort(JsonString(item, "PortName")) ||
                          JsonString(item, "Type").Contains(
                              "Connection",
                              StringComparison.OrdinalIgnoreCase)
            });
        }

        // =============================================================
        // WMI fallback / enrichment
        // =============================================================
        private static Dictionary<string, PrinterQueueInfo>
            GetWmiPrinterQueues()
        {
            var result = new Dictionary<string, PrinterQueueInfo>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"SELECT Name, DriverName, PortName, PNPDeviceID,
                             PrinterStatus, WorkOffline, Local, Network,
                             Default, Shared, ServerName, Status
                      FROM Win32_Printer");

                using ManagementObjectCollection collection =
                    searcher.Get();

                foreach (ManagementObject printer in collection)
                {
                    string name = GetString(printer, "Name");
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var queue = new PrinterQueueInfo
                    {
                        Name = name,
                        DriverName = GetString(printer, "DriverName"),
                        PortName = GetString(printer, "PortName"),
                        PnpDeviceId = GetString(printer, "PNPDeviceID"),
                        PrinterStatus = GetUInt32(printer, "PrinterStatus"),
                        WorkOffline = GetBool(printer, "WorkOffline"),
                        Local = GetBool(printer, "Local"),
                        Network = GetBool(printer, "Network"),
                        IsDefault = GetBool(printer, "Default"),
                        Shared = GetBool(printer, "Shared"),
                        ComputerName = GetString(printer, "ServerName"),
                        Status = GetString(printer, "Status")
                    };

                    result[name] = queue;
                }
            }
            catch
            {
            }

            return result;
        }

        // =============================================================
        // PnP printer + PrintQueue devices
        // =============================================================
        private static List<PnpDeviceInfo> GetAllPrinterPnpDevices()
        {
            var result = new List<PnpDeviceInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $@"SELECT Name, DeviceID, Manufacturer, PNPClass,
                              ClassGuid, Present, ConfigManagerErrorCode, Status
                       FROM Win32_PnPEntity
                       WHERE PNPClass = 'Printer'
                          OR PNPClass = 'PrintQueue'
                          OR ClassGuid = '{PrinterClassGuid}'");

                using ManagementObjectCollection collection =
                    searcher.Get();

                foreach (ManagementObject device in collection)
                {
                    string id = GetString(device, "DeviceID");
                    string name = GetString(device, "Name");

                    if (string.IsNullOrWhiteSpace(id) &&
                        string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    result.Add(new PnpDeviceInfo
                    {
                        Name = name,
                        DeviceId = id,
                        Manufacturer = GetString(device, "Manufacturer"),
                        ClassName = GetString(device, "PNPClass"),
                        ClassGuid = GetString(device, "ClassGuid"),
                        Present = GetBool(device, "Present"),
                        ProblemCode = GetUInt32(device, "ConfigManagerErrorCode"),
                        Status = GetString(device, "Status")
                    });
                }
            }
            catch
            {
                // Optional correlation source.
            }

            return result;
        }

        // =============================================================
        // Installed printer ports
        // =============================================================
        private static HashSet<string> GetInstalledPrinterPorts()
        {
            var result = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            const string script = @"
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Import-Module PrintManagement -ErrorAction Stop
Get-PrinterPort |
    Select-Object -ExpandProperty Name |
    ConvertTo-Json -Compress
";

            try
            {
                string json = RunPowerShell(script, 15_000);

                if (string.IsNullOrWhiteSpace(json))
                    return result;

                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        string value = item.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(value))
                            result.Add(value);
                    }
                }
                else if (root.ValueKind == JsonValueKind.String)
                {
                    string value = root.GetString() ?? "";
                    if (!string.IsNullOrWhiteSpace(value))
                        result.Add(value.Trim());
                }
            }
            catch
            {
            }

            return result;
        }

        // =============================================================
        // Duplicate detection
        // =============================================================
        private static void DetectDuplicateQueues(
            List<PrinterQueueInfo> queues,
            List<GhostPrinterModel> result,
            Dictionary<string, string> providers)
        {
            var usable = new List<PrinterQueueInfo>();

            foreach (PrinterQueueInfo queue in queues)
            {
                if (!IsVirtualPrinter(
                        queue.Name,
                        queue.DriverName,
                        queue.PortName))
                {
                    usable.Add(queue);
                }
            }

            // A) Explicit Copy 1 / Copy 2 / Copy 3 is always surfaced.
            foreach (PrinterQueueInfo queue in usable)
            {
                if (!HasCopyLikeSuffix(queue.Name))
                    continue;

                AddOrUpdateResult(
                    result,
                    queue,
                    providers,
                    "Duplicate / Copy",
                    "Printer name has a Copy suffix and is a duplicate-queue candidate.");
            }

            // B) Same base printer name.
            var nameGroups = new Dictionary<
                string,
                List<PrinterQueueInfo>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (PrinterQueueInfo queue in usable)
            {
                string baseName =
                    NormalizePrinterBaseName(queue.Name);

                if (string.IsNullOrWhiteSpace(baseName))
                    continue;

                if (!nameGroups.TryGetValue(
                        baseName,
                        out List<PrinterQueueInfo>? group))
                {
                    group = new List<PrinterQueueInfo>();
                    nameGroups.Add(baseName, group);
                }

                group.Add(queue);
            }

            foreach (List<PrinterQueueInfo> group in nameGroups.Values)
            {
                if (group.Count < 2)
                    continue;

                PrinterQueueInfo canonical =
                    ChooseCanonical(group);

                foreach (PrinterQueueInfo queue in group)
                {
                    if (ReferenceEquals(queue, canonical))
                        continue;

                    AddOrUpdateResult(
                        result,
                        queue,
                        providers,
                        "Duplicate / Copy",
                        $"Another printer queue with the same base name exists: '{canonical.Name}'.");
                }
            }

            // C) Same driver + same port.
            // This catches stale duplicates whose names were manually renamed.
            var technicalGroups = new Dictionary<
                string,
                List<PrinterQueueInfo>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (PrinterQueueInfo queue in usable)
            {
                string driver = NormalizeText(queue.DriverName);
                string port = NormalizeText(queue.PortName);

                if (string.IsNullOrWhiteSpace(driver) ||
                    string.IsNullOrWhiteSpace(port))
                {
                    continue;
                }

                string key = driver + "|" + port;

                if (!technicalGroups.TryGetValue(
                        key,
                        out List<PrinterQueueInfo>? group))
                {
                    group = new List<PrinterQueueInfo>();
                    technicalGroups.Add(key, group);
                }

                group.Add(queue);
            }

            foreach (List<PrinterQueueInfo> group in technicalGroups.Values)
            {
                if (group.Count < 2)
                    continue;

                PrinterQueueInfo canonical =
                    ChooseCanonical(group);

                foreach (PrinterQueueInfo queue in group)
                {
                    if (ReferenceEquals(queue, canonical))
                        continue;

                    AddOrUpdateResult(
                        result,
                        queue,
                        providers,
                        "Duplicate Candidate",
                        $"Another queue uses the same driver and port as '{canonical.Name}'.");
                }
            }
        }

        private static PrinterQueueInfo ChooseCanonical(
            List<PrinterQueueInfo> group)
        {
            foreach (PrinterQueueInfo queue in group)
            {
                if (queue.IsDefault)
                    return queue;
            }

            foreach (PrinterQueueInfo queue in group)
            {
                if (!HasCopyLikeSuffix(queue.Name) &&
                    !queue.WorkOffline)
                {
                    return queue;
                }
            }

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

        private static void AddOrUpdateResult(
            List<GhostPrinterModel> result,
            PrinterQueueInfo queue,
            Dictionary<string, string> providers,
            string state,
            string reason)
        {
            GhostPrinterModel? existing = null;

            foreach (GhostPrinterModel item in result)
            {
                if (item.ActionType == "Queue" &&
                    item.Name.Equals(
                        queue.Name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    existing = item;
                    break;
                }
            }

            string provider =
                ResolveProvider(queue.DriverName, providers);

            if (existing == null)
            {
                result.Add(CreateQueueModel(
                    queue,
                    provider,
                    state,
                    reason));
                return;
            }

            if (!existing.Reason.Contains(
                    reason,
                    StringComparison.OrdinalIgnoreCase))
            {
                existing.Reason +=
                    string.IsNullOrWhiteSpace(existing.Reason)
                        ? reason
                        : " " + reason;
            }

            existing.Description = existing.Reason;

            // A real disconnect/stale state is stronger than a duplicate hint.
            if (state == "Disconnected" ||
                state == "Stale Queue")
            {
                existing.State = state;
            }
            else if (existing.State == "")
            {
                existing.State = state;
            }
        }

        private static void AddQueueResult(
            List<GhostPrinterModel> result,
            PrinterQueueInfo queue,
            Dictionary<string, string> providers,
            string state,
            string reason)
        {
            AddOrUpdateResult(
                result,
                queue,
                providers,
                state,
                reason);
        }

        private static GhostPrinterModel CreateQueueModel(
            PrinterQueueInfo queue,
            string provider,
            string state,
            string reason)
        {
            return new GhostPrinterModel
            {
                Name = queue.Name,
                DriverName = queue.DriverName,
                PortName = queue.PortName,
                PrinterType = DetectPrinterType(
                    queue.PortName,
                    queue.Network),
                DeviceName = string.IsNullOrWhiteSpace(queue.PnpDeviceId)
                    ? queue.Name
                    : queue.PnpDeviceId,
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
            };
        }

        // =============================================================
        // Helpers: classification
        // =============================================================
        private static bool IsNetworkConnection(
            PrinterQueueInfo queue)
        {
            if (queue.Network)
                return true;

            if (queue.Type.Contains(
                    "Connection",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (queue.PortName.StartsWith(
                    "\\\\",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(queue.ComputerName) &&
                !string.Equals(
                    queue.ComputerName,
                    Environment.MachineName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsNetworkPort(string portName)
        {
            return portName.StartsWith(
                       "\\\\",
                       StringComparison.OrdinalIgnoreCase) ||
                   portName.StartsWith(
                       "IPP",
                       StringComparison.OrdinalIgnoreCase) ||
                   portName.StartsWith(
                       "HTTP",
                       StringComparison.OrdinalIgnoreCase);
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

            if (Regex.IsMatch(
                    value,
                    @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$"))
            {
                return true;
            }

            return false;
        }

        private static bool HasPresentPnpName(
            string printerName,
            List<PnpDeviceInfo> devices)
        {
            foreach (PnpDeviceInfo device in devices)
            {
                if (!device.Present)
                    continue;

                if (string.IsNullOrWhiteSpace(device.Name))
                    continue;

                if (device.Name.Equals(
                        printerName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (NormalizePrinterBaseName(device.Name).Equals(
                        NormalizePrinterBaseName(printerName),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AlreadyRepresentedByDeviceId(
            List<GhostPrinterModel> result,
            string deviceId)
        {
            foreach (GhostPrinterModel item in result)
            {
                if (!string.IsNullOrWhiteSpace(item.DeviceId) &&
                    item.DeviceId.Equals(
                        deviceId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCopyLikeSuffix(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return Regex.IsMatch(
                name.Trim(),
                @"(?:\s+[-–—]?\s*Copy(?:\s*\d+)?)$|(?:\s*\(Copy\s*\d*\))$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string NormalizePrinterBaseName(
            string name)
        {
            string value = NormalizeText(name);

            for (int i = 0; i < 4; i++)
            {
                value = Regex.Replace(
                    value,
                    @"(?:\s+[-–—]?\s*Copy(?:\s*\d+)?)$",
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

        private static string DetectPrinterType(
            string portName,
            bool network)
        {
            if (network ||
                portName.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                return "Network";

            if (portName.StartsWith("WSD", StringComparison.OrdinalIgnoreCase))
                return "WSD";

            if (portName.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
                return "USB";

            if (portName.StartsWith("DOT4", StringComparison.OrdinalIgnoreCase))
                return "DOT4";

            if (portName.StartsWith("IP_", StringComparison.OrdinalIgnoreCase) ||
                portName.StartsWith("TCPIP_", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(portName, @"^\d{1,3}(\.\d{1,3}){3}"))
            {
                return "TCP/IP";
            }

            if (portName.StartsWith("IPP", StringComparison.OrdinalIgnoreCase) ||
                portName.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                return "IPP";

            return "PrintQueue";
        }

        private static bool IsVirtualPrinter(
            string name,
            string driver,
            string port)
        {
            string[] values =
            {
                name,
                driver,
                port
            };

            string[] virtualNames =
            {
                "Microsoft Print to PDF",
                "Microsoft XPS Document Writer",
                "Fax",
                "OneNote",
                "Send To OneNote",
                "Microsoft Shared Fax Driver",
                "Microsoft Software Printer"
            };

            foreach (string value in values)
            {
                foreach (string virtualName in virtualNames)
                {
                    if (value.Contains(
                            virtualName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return port.Equals(
                       "PORTPROMPT:",
                       StringComparison.OrdinalIgnoreCase) ||
                   port.Equals(
                       "FILE:",
                       StringComparison.OrdinalIgnoreCase) ||
                   port.Equals(
                       "NUL:",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string>
            GetPrinterDriverProviders()
        {
            var result = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, DriverName, ProviderName FROM Win32_PrinterDriver");

                using ManagementObjectCollection collection =
                    searcher.Get();

                foreach (ManagementObject driver in collection)
                {
                    string name = GetString(driver, "Name");
                    string driverName = GetString(driver, "DriverName");
                    string provider = GetString(driver, "ProviderName");

                    if (!string.IsNullOrWhiteSpace(name))
                        result[name] = provider;

                    if (!string.IsNullOrWhiteSpace(driverName))
                        result[driverName] = provider;
                }
            }
            catch
            {
            }

            return result;
        }

        private static string ResolveProvider(
            string driverName,
            Dictionary<string, string> providers)
        {
            if (!string.IsNullOrWhiteSpace(driverName) &&
                providers.TryGetValue(
                    driverName,
                    out string? value) &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

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
                "Pantum",
                "Fuji Xerox",
                "Samsung"
            };

            foreach (string vendor in vendors)
            {
                if (driverName.Contains(
                        vendor,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return vendor;
                }
            }

            if (driverName.Contains(
                    "Microsoft",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Microsoft";
            }

            return "Unknown";
        }

        private static string NormalizeText(string value)
        {
            return Regex.Replace(
                value ?? "",
                @"\s+",
                " ",
                RegexOptions.CultureInvariant).Trim();
        }

        private static void SortResult(
            List<GhostPrinterModel> list)
        {
            list.Sort((a, b) =>
            {
                int state =
                    StateOrder(a.State).CompareTo(
                        StateOrder(b.State));

                if (state != 0)
                    return state;

                return string.Compare(
                    a.Name,
                    b.Name,
                    StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int StateOrder(string state)
        {
            return state switch
            {
                "Disconnected" => 0,
                "Stale Queue" => 1,
                "Duplicate / Copy" => 2,
                "Duplicate Candidate" => 3,
                "Disconnected Device" => 4,
                _ => 5
            };
        }

        // =============================================================
        // PowerShell helper
        // =============================================================
        private static string RunPowerShell(
            string script,
            int timeout)
        {
            string encoded = Convert.ToBase64String(
                Encoding.Unicode.GetBytes(script));

            var info = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments =
                    "-NoLogo -NoProfile -NonInteractive " +
                    "-ExecutionPolicy Bypass " +
                    "-EncodedCommand " + encoded,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process
            {
                StartInfo = info
            };

            process.Start();

            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync();

            Task<string> errorTask =
                process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(timeout))
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

            string output =
                outputTask.GetAwaiter().GetResult();

            if (process.ExitCode != 0)
            {
                return "";
            }

            return output.Trim();
        }

        // =============================================================
        // JSON helpers
        // =============================================================
        private static string JsonString(
            JsonElement item,
            string property)
        {
            if (!item.TryGetProperty(
                    property,
                    out JsonElement value))
            {
                return "";
            }

            if (value.ValueKind == JsonValueKind.Null)
                return "";

            return value.ToString().Trim();
        }

        private static bool JsonBool(
            JsonElement item,
            string property)
        {
            if (!item.TryGetProperty(
                    property,
                    out JsonElement value))
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.True)
                return true;

            if (value.ValueKind == JsonValueKind.False)
                return false;

            return bool.TryParse(
                value.ToString(),
                out bool result) && result;
        }

        private static uint JsonUInt32(
            JsonElement item,
            string property)
        {
            if (!item.TryGetProperty(
                    property,
                    out JsonElement value))
            {
                return 0;
            }

            if (value.ValueKind == JsonValueKind.Number &&
                value.TryGetUInt32(out uint number))
            {
                return number;
            }

            return uint.TryParse(
                value.ToString(),
                out uint result)
                ? result
                : 0;
        }

        // =============================================================
        // WMI helpers
        // =============================================================
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
                return value is bool b && b;
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
                return value == null
                    ? 0
                    : Convert.ToUInt32(value);
            }
            catch
            {
                return 0;
            }
        }
    }
}
