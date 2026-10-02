using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Printer / printer-device inventory.
    ///
    /// Discovery order:
    /// 1. Windows "Devices and Printers" Shell namespace.
    /// 2. Win32_Printer queue inventory.
    /// 3. Win32_PnPEntity device inventory, including non-present devices.
    ///
    /// This service is read-only. It does not delete or modify devices.
    /// </summary>
    public static class PrintServerService
    {
        private const string PrinterClassGuid =
            "{4D36E979-E325-11CE-BFC1-08002BE10318}";

        private const string DevicesAndPrintersNamespace =
            "::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";

        public static List<PrintServerPrinterModel> GetServerPrinters(
            string serverName,
            bool includeNormal = true)
        {
            string server = NormalizeServerName(serverName);

            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentException(
                    "Print Server name is required.",
                    nameof(serverName));

            bool local =
                IsLocalServer(server);

            var result =
                new List<PrintServerPrinterModel>();

            // ---------------------------------------------------------
            // 1. Devices and Printers Shell inventory
            // ---------------------------------------------------------
            if (local)
            {
                try
                {
                    AddShellDevicesAndPrinters(
                        server,
                        result);
                }
                catch
                {
                    // Shell enumeration is an additional source.
                    // WMI/queue sources below remain available.
                }

                try
                {
                    AddShellPrinterFolder(
                        server,
                        result);
                }
                catch
                {
                    // Optional source.
                }
            }

            // ---------------------------------------------------------
            // 2. Win32_Printer queue inventory
            // ---------------------------------------------------------
            try
            {
                AddWin32Printers(
                    server,
                    result);
            }
            catch
            {
                // Keep PnP/Shell results if queue query fails.
            }

            // ---------------------------------------------------------
            // 3. Win32_PnPEntity inventory.
            // No PresentOnly filter is used.
            // ---------------------------------------------------------
            try
            {
                AddPnpPrinters(
                    server,
                    result,
                    local);
            }
            catch
            {
                // Keep results from other sources.
            }

            // Merge duplicate rows.
            result =
                MergeRows(result);

            if (!includeNormal)
            {
                result.RemoveAll(item =>
                    item.State.Equals(
                        "Normal",
                        StringComparison.OrdinalIgnoreCase));
            }

            result.Sort((a, b) =>
                string.Compare(
                    a.PrinterName,
                    b.PrinterName,
                    StringComparison.OrdinalIgnoreCase));

            return result;
        }

        // =============================================================
        // SHELL: DEVICES AND PRINTERS
        // =============================================================
        private static void AddShellDevicesAndPrinters(
            string server,
            List<PrintServerPrinterModel> result)
        {
            Type? shellType =
                Type.GetTypeFromProgID("Shell.Application");

            if (shellType == null)
                return;

            object? shellObject =
                Activator.CreateInstance(shellType);

            if (shellObject == null)
                return;

            dynamic shell = shellObject;

            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                folder =
                    shell.NameSpace(
                        DevicesAndPrintersNamespace);

                if (folder == null)
                    return;

                items = folder.Items();

                int count =
                    Convert.ToInt32(items.Count);

                for (int i = 0; i < count; i++)
                {
                    dynamic? item = null;

                    try
                    {
                        item = items.Item(i);

                        if (item == null)
                            continue;

                        string name =
                            SafeDynamicString(
                                item.Name);

                        string type =
                            SafeDynamicString(
                                item.Type);

                        string path =
                            SafeDynamicString(
                                item.Path);

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        if (!LooksLikePrinter(
                                name,
                                type,
                                path))
                        {
                            continue;
                        }

                        var model =
                            new PrintServerPrinterModel
                            {
                                ServerName = server,
                                PrinterName = name,
                                Source = "Devices and Printers",
                                Status = "Unknown",
                                PrinterState = "Unknown",
                                State = "PnP Device",
                                Reason =
                                    string.IsNullOrWhiteSpace(type)
                                        ? "Device found in Windows Devices and Printers."
                                        : $"Device found in Windows Devices and Printers ({type}).",
                                JobCount = "0",
                                Jobs = "",
                                DeviceId = path,
                                Manufacturer =
                                    DetectManufacturer(name),
                                ClassGuid =
                                    PrinterClassGuid,
                                Present = true
                            };

                        result.Add(model);
                    }
                    catch
                    {
                        // Ignore a single Shell item and continue scanning.
                    }
                    finally
                    {
                        ReleaseComObject(item);
                    }
                }
            }
            finally
            {
                ReleaseComObject(items);
                ReleaseComObject(folder);
                ReleaseComObject(shell);
            }
        }

        // =============================================================
        // SHELL: PRINTERS FOLDER
        // =============================================================
        private static void AddShellPrinterFolder(
            string server,
            List<PrintServerPrinterModel> result)
        {
            Type? shellType =
                Type.GetTypeFromProgID("Shell.Application");

            if (shellType == null)
                return;

            object? shellObject =
                Activator.CreateInstance(shellType);

            if (shellObject == null)
                return;

            dynamic shell = shellObject;

            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                folder =
                    shell.NameSpace("shell:PrintersFolder");

                if (folder == null)
                    return;

                items = folder.Items();

                int count =
                    Convert.ToInt32(items.Count);

                for (int i = 0; i < count; i++)
                {
                    dynamic? item = null;

                    try
                    {
                        item = items.Item(i);

                        if (item == null)
                            continue;

                        string name =
                            SafeDynamicString(item.Name);

                        string type =
                            SafeDynamicString(item.Type);

                        string path =
                            SafeDynamicString(item.Path);

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        if (!LooksLikePrinter(
                                name,
                                type,
                                path))
                        {
                            continue;
                        }

                        result.Add(
                            new PrintServerPrinterModel
                            {
                                ServerName = server,
                                PrinterName = name,
                                Source = "Printers Folder",
                                Status = "Unknown",
                                PrinterState = "Unknown",
                                State = "PnP Device",
                                Reason =
                                    "Printer found in the Windows Printers folder.",
                                JobCount = "0",
                                Jobs = "",
                                DeviceId = path,
                                Manufacturer =
                                    DetectManufacturer(name),
                                ClassGuid =
                                    PrinterClassGuid,
                                Present = true
                            });
                    }
                    catch
                    {
                    }
                    finally
                    {
                        ReleaseComObject(item);
                    }
                }
            }
            finally
            {
                ReleaseComObject(items);
                ReleaseComObject(folder);
                ReleaseComObject(shell);
            }
        }

        // =============================================================
        // WMI: WIN32_PRINTER
        // =============================================================
        private static void AddWin32Printers(
            string server,
            List<PrintServerPrinterModel> result)
        {
            ManagementScope scope =
                CreateScope(
                    server,
                    @"root\cimv2");

            scope.Connect();

            using var searcher =
                new ManagementObjectSearcher(
                    scope,
                    new ObjectQuery(
                        "SELECT * FROM Win32_Printer"));

            using ManagementObjectCollection collection =
                searcher.Get();

            foreach (ManagementObject printer in collection)
            {
                string name =
                    SafeManagementString(
                        printer,
                        "Name");

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string driver =
                    SafeManagementString(
                        printer,
                        "DriverName");

                string port =
                    SafeManagementString(
                        printer,
                        "PortName");

                string status =
                    SafeManagementString(
                        printer,
                        "Status");

                string shareName =
                    SafeManagementString(
                        printer,
                        "ShareName");

                bool shared =
                    SafeManagementBool(
                        printer,
                        "Shared");

                bool workOffline =
                    SafeManagementBool(
                        printer,
                        "WorkOffline");

                uint printerStatus =
                    SafeManagementUInt32(
                        printer,
                        "PrinterStatus");

                string state =
                    workOffline
                        ? "Work Offline"
                        : "Normal";

                string reason =
                    workOffline
                        ? "Windows reports this printer queue as Work Offline."
                        : "Print Queue was found.";

                result.Add(
                    new PrintServerPrinterModel
                    {
                        ServerName = server,
                        PrinterName = name,
                        ShareName = shareName,
                        DriverName = driver,
                        PortName = port,
                        PortMonitor = "",
                        Status = status,
                        PrinterState =
                            printerStatus == 0
                                ? status
                                : printerStatus.ToString(),
                        State = state,
                        Reason = reason,
                        JobCount = "0",
                        Jobs = "",
                        Source = "Print Queue",
                        DeviceId =
                            SafeManagementString(
                                printer,
                                "PNPDeviceID"),
                        Manufacturer =
                            DetectManufacturer(
                                name + " " + driver),
                        ClassGuid = PrinterClassGuid,
                        Present = true,
                        Shared = shared,
                        Published =
                            SafeManagementBool(
                                printer,
                                "Published"),
                        WorkOffline = workOffline
                    });

                printer.Dispose();
            }
        }

        // =============================================================
        // WMI: WIN32_PNPEntity
        // =============================================================
        private static void AddPnpPrinters(
            string server,
            List<PrintServerPrinterModel> result,
            bool local)
        {
            ManagementScope scope =
                CreateScope(
                    server,
                    @"root\cimv2");

            scope.Connect();

            using var searcher =
                new ManagementObjectSearcher(
                    scope,
                    new ObjectQuery(
                        "SELECT Name, Caption, Description, " +
                        "DeviceID, PNPDeviceID, Manufacturer, " +
                        "PNPClass, ClassGuid, Status, " +
                        "ConfigManagerErrorCode, Present, Service " +
                        "FROM Win32_PnPEntity"));

            using ManagementObjectCollection collection =
                searcher.Get();

            foreach (ManagementObject device in collection)
            {
                string name =
                    FirstNonEmpty(
                        SafeManagementString(device, "Name"),
                        SafeManagementString(device, "Caption"),
                        SafeManagementString(device, "Description"));

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string pnpClass =
                    SafeManagementString(
                        device,
                        "PNPClass");

                string classGuid =
                    SafeManagementString(
                        device,
                        "ClassGuid");

                string deviceId =
                    FirstNonEmpty(
                        SafeManagementString(
                            device,
                            "PNPDeviceID"),
                        SafeManagementString(
                            device,
                            "DeviceID"));

                string manufacturer =
                    SafeManagementString(
                        device,
                        "Manufacturer");

                string service =
                    SafeManagementString(
                        device,
                        "Service");

                string status =
                    SafeManagementString(
                        device,
                        "Status");

                bool? present =
                    SafeManagementNullableBool(
                        device,
                        "Present");

                if (!IsPrinterRelated(
                        name,
                        pnpClass,
                        classGuid,
                        manufacturer,
                        service))
                {
                    continue;
                }

                bool isPresent =
                    present ?? !LooksNonPresent(status);

                PrintServerPrinterModel? existing =
                    FindBestMatch(
                        result,
                        name);

                if (existing != null)
                {
                    // Enrich queue/shell row with PnP information.
                    existing.Source =
                        MergeSource(
                            existing.Source,
                            "PnP Device");

                    existing.DeviceId =
                        FirstNonEmpty(
                            deviceId,
                            existing.DeviceId);

                    existing.Manufacturer =
                        FirstNonEmpty(
                            manufacturer,
                            existing.Manufacturer);

                    existing.ClassGuid =
                        FirstNonEmpty(
                            classGuid,
                            existing.ClassGuid);

                    existing.Present =
                        isPresent;

                    existing.Status =
                        FirstNonEmpty(
                            status,
                            existing.Status);

                    existing.PrinterState =
                        FirstNonEmpty(
                            status,
                            existing.PrinterState);

                    if (!isPresent)
                    {
                        existing.State =
                            "Disconnected";

                        existing.Reason =
                            "Printer device remains in the Windows PnP inventory but is not currently present.";
                    }

                    continue;
                }

                string state =
                    isPresent
                        ? "PnP Device"
                        : "Disconnected";

                string reason =
                    isPresent
                        ? "Printer-related device was found in the Windows PnP inventory."
                        : "Printer-related device remains in Windows PnP inventory but is not currently present.";

                result.Add(
                    new PrintServerPrinterModel
                    {
                        ServerName = server,
                        PrinterName = name,
                        ShareName = "",
                        DriverName = "",
                        PortName = "",
                        PortMonitor = "",
                        Status = status,
                        PrinterState = status,
                        State = state,
                        Reason = reason,
                        JobCount = "0",
                        Jobs = "",
                        Source = "PnP Device",
                        DeviceId = deviceId,
                        Manufacturer = manufacturer,
                        ClassGuid =
                            string.IsNullOrWhiteSpace(classGuid)
                                ? PrinterClassGuid
                                : classGuid,
                        Present = isPresent,
                        Shared = false,
                        Published = false,
                        WorkOffline = false
                    });
            }
        }

        // =============================================================
        // FILTERING
        // =============================================================
        private static bool IsPrinterRelated(
            string name,
            string pnpClass,
            string classGuid,
            string manufacturer,
            string service)
        {
            if (string.Equals(
                    classGuid,
                    PrinterClassGuid,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(
                    pnpClass,
                    "Printer",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string text =
                $"{name} {manufacturer} {service}";

            if (ContainsAny(
                    text,
                    "printer",
                    "print to pdf",
                    "onenote",
                    "fax",
                    "dcp-",
                    "pixma",
                    "ecotank",
                    "workforce",
                    "smart tank",
                    "deskjet",
                    "laserjet",
                    "imageclass",
                    "imageRUNNER",
                    "imageclass",
                    "brother",
                    "canon",
                    "epson",
                    "hp ",
                    "lexmark",
                    "xerox",
                    "kyocera",
                    "pantum",
                    "ricoh",
                    "samsung ml",
                    "wSD"))
            {
                return true;
            }

            // Imaging class can include multifunction printers/scanners.
            if (string.Equals(
                    pnpClass,
                    "Image",
                    StringComparison.OrdinalIgnoreCase) &&
                ContainsAny(
                    text,
                    "canon",
                    "epson",
                    "brother",
                    "hp ",
                    "lexmark",
                    "xerox",
                    "kyocera",
                    "ricoh",
                    "pantum"))
            {
                return true;
            }

            return false;
        }

        private static bool LooksLikePrinter(
            string name,
            string type,
            string path)
        {
            string text =
                $"{name} {type} {path}";

            return ContainsAny(
                text,
                "printer",
                "print",
                "onenote",
                "fax",
                "dcp-",
                "pixma",
                "ecotank",
                "workforce",
                "smart tank",
                "deskjet",
                "laserjet",
                "imageclass",
                "imagerunner",
                "brother",
                "canon",
                "epson",
                "hp ",
                "lexmark",
                "xerox",
                "kyocera",
                "pantum",
                "ricoh");
        }

        // =============================================================
        // MERGE
        // =============================================================
        private static List<PrintServerPrinterModel> MergeRows(
            List<PrintServerPrinterModel> rows)
        {
            var result =
                new List<PrintServerPrinterModel>();

            foreach (PrintServerPrinterModel row in rows)
            {
                PrintServerPrinterModel? existing =
                    FindBestMatch(
                        result,
                        row.PrinterName);

                if (existing == null)
                {
                    result.Add(row);
                    continue;
                }

                existing.Source =
                    MergeSource(
                        existing.Source,
                        row.Source);

                existing.ShareName =
                    FirstNonEmpty(
                        existing.ShareName,
                        row.ShareName);

                existing.DriverName =
                    FirstNonEmpty(
                        existing.DriverName,
                        row.DriverName);

                existing.PortName =
                    FirstNonEmpty(
                        existing.PortName,
                        row.PortName);

                existing.PortMonitor =
                    FirstNonEmpty(
                        existing.PortMonitor,
                        row.PortMonitor);

                existing.Status =
                    FirstNonEmpty(
                        existing.Status,
                        row.Status);

                existing.PrinterState =
                    FirstNonEmpty(
                        existing.PrinterState,
                        row.PrinterState);

                existing.DeviceId =
                    FirstNonEmpty(
                        existing.DeviceId,
                        row.DeviceId);

                existing.Manufacturer =
                    FirstNonEmpty(
                        existing.Manufacturer,
                        row.Manufacturer);

                existing.ClassGuid =
                    FirstNonEmpty(
                        existing.ClassGuid,
                        row.ClassGuid);

                existing.Shared |=
                    row.Shared;

                existing.Published |=
                    row.Published;

                existing.WorkOffline |=
                    row.WorkOffline;

                // Prefer a disconnected state when any source says
                // the device is not present.
                if (!row.Present)
                {
                    existing.Present = false;
                    existing.State = "Disconnected";
                    existing.Reason =
                        FirstNonEmpty(
                            row.Reason,
                            existing.Reason);
                }
                else if (existing.State != "Disconnected" &&
                         existing.State == "PnP Device" &&
                         row.State == "Normal")
                {
                    // Queue + PnP is healthier than PnP-only.
                    existing.State = "Normal";
                }
            }

            return result;
        }

        private static PrintServerPrinterModel? FindBestMatch(
            List<PrintServerPrinterModel> rows,
            string name)
        {
            string key =
                NormalizeName(name);

            if (string.IsNullOrWhiteSpace(key))
                return null;

            return rows.FirstOrDefault(
                row =>
                    NormalizeName(
                        row.PrinterName) == key);
        }

        // =============================================================
        // UTILITIES
        // =============================================================
        private static ManagementScope CreateScope(
            string server,
            string namespacePath)
        {
            string path =
                IsLocalServer(server)
                    ? namespacePath
                    : $@"\\{server}\{namespacePath}";

            return new ManagementScope(
                $@"\\{server}\root\cimv2");
        }

        private static bool IsLocalServer(
            string server)
        {
            string local =
                Environment.MachineName;

            return
                server.Equals(
                    local,
                    StringComparison.OrdinalIgnoreCase)
                ||
                server.Equals(
                    "localhost",
                    StringComparison.OrdinalIgnoreCase)
                ||
                server.Equals(
                    ".",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeServerName(
            string value)
        {
            string server =
                value?.Trim() ?? "";

            if (server.StartsWith(
                    @"\\",
                    StringComparison.Ordinal))
            {
                server =
                    server.Substring(2);
            }

            int slash =
                server.IndexOf('\\');

            if (slash >= 0)
                server =
                    server.Substring(
                        0,
                        slash);

            return server.Trim();
        }

        private static string NormalizeName(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value
                    .Trim()
                    .Replace(
                        "  ",
                        " ")
                    .ToLowerInvariant();
        }

        private static string MergeSource(
            string first,
            string second)
        {
            var parts =
                new List<string>();

            void AddPart(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                foreach (string item in value.Split(
                    '+',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries))
                {
                    if (!parts.Contains(
                            item,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        parts.Add(item);
                    }
                }
            }

            AddPart(first);
            AddPart(second);

            return string.Join(
                " + ",
                parts);
        }

        private static string FirstNonEmpty(
            string first,
            string second)
        {
            return !string.IsNullOrWhiteSpace(first)
                ? first
                : second ?? "";
        }

        private static string FirstNonEmpty(
            string first,
            string second,
            string third)
        {
            return FirstNonEmpty(
                FirstNonEmpty(first, second),
                third);
        }

        private static bool ContainsAny(
            string text,
            params string[] values)
        {
            foreach (string value in values)
            {
                if (text.Contains(
                    value,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string DetectManufacturer(
            string text)
        {
            if (ContainsAny(text, "canon"))
                return "Canon";

            if (ContainsAny(text, "epson"))
                return "Epson";

            if (ContainsAny(text, "brother"))
                return "Brother";

            if (ContainsAny(text, "hp "))
                return "HP";

            if (ContainsAny(text, "lexmark"))
                return "Lexmark";

            if (ContainsAny(text, "xerox"))
                return "Xerox";

            if (ContainsAny(text, "kyocera"))
                return "Kyocera";

            if (ContainsAny(text, "ricoh"))
                return "Ricoh";

            if (ContainsAny(text, "pantum"))
                return "Pantum";

            if (ContainsAny(text, "microsoft"))
                return "Microsoft";

            return "";
        }

        private static bool LooksNonPresent(
            string status)
        {
            return status.Equals(
                       "Unknown",
                       StringComparison.OrdinalIgnoreCase) ||
                   status.Equals(
                       "Error",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string SafeManagementString(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value =
                    obj[property];

                return value?.ToString()?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static bool SafeManagementBool(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value =
                    obj[property];

                return value != null &&
                       Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        private static bool? SafeManagementNullableBool(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value =
                    obj[property];

                if (value == null)
                    return null;

                return Convert.ToBoolean(value);
            }
            catch
            {
                return null;
            }
        }

        private static uint SafeManagementUInt32(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value =
                    obj[property];

                return value == null
                    ? 0
                    : Convert.ToUInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        private static string SafeDynamicString(
            object? value)
        {
            try
            {
                return value?.ToString()?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static void ReleaseComObject(
            object? value)
        {
            try
            {
                if (value != null &&
                    Marshal.IsComObject(value))
                {
                    Marshal.FinalReleaseComObject(value);
                }
            }
            catch
            {
            }
        }
    }
}
