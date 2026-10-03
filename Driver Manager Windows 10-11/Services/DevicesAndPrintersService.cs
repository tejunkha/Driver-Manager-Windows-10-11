using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Printer-only inventory.
    ///
    /// Sources:
    /// 1. Windows Devices and Printers Shell view.
    /// 2. Windows Printers folder.
    /// 3. Win32_Printer queue information.
    /// 4. Win32_PnPEntity printer information.
    ///
    /// Non-printer devices are never added to the result.
    /// The service is read-only.
    /// </summary>
    public static class DevicesAndPrintersService
    {
        private const string DevicesAndPrintersNamespace =
            "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\2\\::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";

        private const string PrinterClassGuid =
            "{4D36E979-E325-11CE-BFC1-08002BE10318}";

        public static List<PrintServerPrinterModel> GetAll(
            string serverName = "")
        {
            string server =
                NormalizeServerName(serverName);

            if (string.IsNullOrWhiteSpace(server))
                server = Environment.MachineName;

            if (!IsLocalServer(server))
            {
                throw new NotSupportedException(
                    "Printer Devices inventory is available for the local computer only.");
            }

            var rows =
                new List<PrintServerPrinterModel>();

            // 1) Devices and Printers Shell
            TryAdd(
                () => AddShellDeviceAndPrinterItems(server, rows));

            // 2) Printers folder
            TryAdd(
                () => AddShellPrinterFolder(server, rows));

            // 3) Win32_Printer queues
            TryAdd(
                () => AddWin32Printers(server, rows));

            // 4) PnP printer devices, including stored/non-present records
            TryAdd(
                () => AddPnpPrinters(server, rows));

            // Merge by printer name.
            rows =
                MergeRows(rows);

            foreach (PrintServerPrinterModel row in rows)
            {
                row.Category = "Printer";

                if (string.IsNullOrWhiteSpace(
                        row.State))
                {
                    row.State =
                        row.Present
                            ? "Printer"
                            : "Not Present";
                }
            }

            return rows
                .OrderBy(
                    x => x.PrinterName,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // =============================================================
        // SHELL: DEVICES AND PRINTERS
        // =============================================================
        private static void AddShellDeviceAndPrinterItems(
            string server,
            List<PrintServerPrinterModel> rows)
        {
            object? shellObject =
                CreateShellApplication();

            if (shellObject == null)
                return;

            dynamic shell =
                shellObject;

            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                folder =
                    shell.NameSpace(
                        DevicesAndPrintersNamespace);

                if (folder == null)
                    return;

                items =
                    folder.Items();

                int count =
                    Convert.ToInt32(
                        items.Count);

                for (int index = 0;
                     index < count;
                     index++)
                {
                    dynamic? item = null;

                    try
                    {
                        item =
                            items.Item(index);

                        if (item == null)
                            continue;

                        string name =
                            SafeString(item.Name);

                        string type =
                            SafeString(item.Type);

                        string path =
                            SafeString(item.Path);

                        if (!LooksLikePrinter(
                                name,
                                type,
                                path))
                        {
                            continue;
                        }

                        AddOrUpdate(
                            rows,
                            CreateShellRow(
                                server,
                                name,
                                path,
                                "Devices and Printers",
                                type));
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
        // SHELL: PRINTERS FOLDER
        // =============================================================
        private static void AddShellPrinterFolder(
            string server,
            List<PrintServerPrinterModel> rows)
        {
            object? shellObject =
                CreateShellApplication();

            if (shellObject == null)
                return;

            dynamic shell =
                shellObject;

            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                folder =
                    shell.NameSpace(
                        "shell:PrintersFolder");

                if (folder == null)
                    return;

                items =
                    folder.Items();

                int count =
                    Convert.ToInt32(
                        items.Count);

                for (int index = 0;
                     index < count;
                     index++)
                {
                    dynamic? item = null;

                    try
                    {
                        item =
                            items.Item(index);

                        if (item == null)
                            continue;

                        string name =
                            SafeString(item.Name);

                        string type =
                            SafeString(item.Type);

                        string path =
                            SafeString(item.Path);

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        AddOrUpdate(
                            rows,
                            CreateShellRow(
                                server,
                                name,
                                path,
                                "Printers Folder",
                                type));
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
            List<PrintServerPrinterModel> rows)
        {
            ManagementScope scope =
                CreateScope(server);

            scope.Connect();

            using var searcher =
                new ManagementObjectSearcher(
                    scope,
                    new ObjectQuery(
                        "SELECT Name, DriverName, PortName, Status, " +
                        "ShareName, Shared, Published, WorkOffline, " +
                        "PrinterStatus, PNPDeviceID " +
                        "FROM Win32_Printer"));

            using ManagementObjectCollection collection =
                searcher.Get();

            foreach (ManagementObject printer
                     in collection)
            {
                try
                {
                    string name =
                        SafeManagementString(
                            printer,
                            "Name");

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    PrintServerPrinterModel? row =
                        FindExact(
                            rows,
                            name);

                    if (row == null)
                    {
                        row =
                            new PrintServerPrinterModel
                            {
                                ServerName =
                                    server,

                                PrinterName =
                                    name,

                                Category =
                                    "Printer",

                                Source =
                                    "Win32_Printer",

                                Present =
                                    true,

                                State =
                                    "Printer"
                            };

                        rows.Add(row);
                    }

                    row.DriverName =
                        FirstNonEmpty(
                            row.DriverName,
                            SafeManagementString(
                                printer,
                                "DriverName"));

                    row.PortName =
                        FirstNonEmpty(
                            row.PortName,
                            SafeManagementString(
                                printer,
                                "PortName"));

                    row.ShareName =
                        FirstNonEmpty(
                            row.ShareName,
                            SafeManagementString(
                                printer,
                                "ShareName"));

                    row.Status =
                        FirstNonEmpty(
                            row.Status,
                            SafeManagementString(
                                printer,
                                "Status"));

                    row.PrinterState =
                        FirstNonEmpty(
                            row.PrinterState,
                            row.Status);

                    row.Shared |=
                        SafeManagementBool(
                            printer,
                            "Shared");

                    row.Published |=
                        SafeManagementBool(
                            printer,
                            "Published");

                    row.WorkOffline |=
                        SafeManagementBool(
                            printer,
                            "WorkOffline");

                    row.DeviceId =
                        FirstNonEmpty(
                            row.DeviceId,
                            SafeManagementString(
                                printer,
                                "PNPDeviceID"));

                    row.Manufacturer =
                        FirstNonEmpty(
                            row.Manufacturer,
                            DetectManufacturer(
                                name + " " +
                                row.DriverName));

                    if (row.WorkOffline)
                    {
                        row.State =
                            "Offline";

                        row.Reason =
                            "Windows reports this printer queue as Work Offline.";
                    }
                    else
                    {
                        row.State =
                            "Printer";

                        row.Reason =
                            "Printer queue found.";
                    }
                }
                finally
                {
                    printer.Dispose();
                }
            }
        }

        // =============================================================
        // WMI: PNP PRINTER DEVICES
        // =============================================================
        private static void AddPnpPrinters(
            string server,
            List<PrintServerPrinterModel> rows)
        {
            ManagementScope scope =
                CreateScope(server);

            scope.Connect();

            using var searcher =
                new ManagementObjectSearcher(
                    scope,
                    new ObjectQuery(
                        "SELECT Name, Caption, DeviceID, PNPDeviceID, " +
                        "Manufacturer, PNPClass, ClassGuid, Status, " +
                        "ConfigManagerErrorCode, Present " +
                        "FROM Win32_PnPEntity"));

            using ManagementObjectCollection collection =
                searcher.Get();

            foreach (ManagementObject device
                     in collection)
            {
                string name =
                    FirstNonEmpty(
                        SafeManagementString(
                            device,
                            "Name"),
                        SafeManagementString(
                            device,
                            "Caption"),
                        SafeManagementString(
                            device,
                            "DeviceID"));

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

                string manufacturer =
                    SafeManagementString(
                        device,
                        "Manufacturer");

                string deviceId =
                    FirstNonEmpty(
                        SafeManagementString(
                            device,
                            "PNPDeviceID"),
                        SafeManagementString(
                            device,
                            "DeviceID"));

                if (!IsPrinterRelated(
                        name,
                        pnpClass,
                        classGuid,
                        manufacturer))
                {
                    continue;
                }

                bool? present =
                    SafeManagementNullableBool(
                        device,
                        "Present");

                bool isPresent =
                    present ?? true;

                PrintServerPrinterModel? row =
                    FindExact(
                        rows,
                        name);

                if (row == null)
                {
                    row =
                        new PrintServerPrinterModel
                        {
                            ServerName =
                                server,

                            PrinterName =
                                name,

                            Category =
                                "Printer",

                            Source =
                                "PnP Device",

                            DeviceId =
                                deviceId,

                            Manufacturer =
                                manufacturer,

                            ClassGuid =
                                string.IsNullOrWhiteSpace(
                                    classGuid)
                                    ? PrinterClassGuid
                                    : classGuid,

                            Status =
                                SafeManagementString(
                                    device,
                                    "Status"),

                            Present =
                                isPresent,

                            State =
                                isPresent
                                    ? "Printer"
                                    : "Not Present",

                            Reason =
                                isPresent
                                    ? "Printer device found in Windows PnP inventory."
                                    : "Printer device remains in Windows PnP inventory but is not currently present."
                        };

                    rows.Add(row);
                }
                else
                {
                    row.Source =
                        MergeSource(
                            row.Source,
                            "PnP Device");

                    row.DeviceId =
                        FirstNonEmpty(
                            row.DeviceId,
                            deviceId);

                    row.Manufacturer =
                        FirstNonEmpty(
                            row.Manufacturer,
                            manufacturer);

                    row.ClassGuid =
                        FirstNonEmpty(
                            row.ClassGuid,
                            classGuid);

                    row.Status =
                        FirstNonEmpty(
                            row.Status,
                            SafeManagementString(
                                device,
                                "Status"));

                    if (present.HasValue)
                    {
                        row.Present =
                            present.Value;

                        if (!present.Value)
                        {
                            row.State =
                                "Not Present";

                            row.Reason =
                                "Printer device remains in Windows PnP inventory but is not currently present.";
                        }
                    }
                }
            }
        }

        // =============================================================
        // FILTER
        // =============================================================
        private static bool IsPrinterRelated(
            string name,
            string pnpClass,
            string classGuid,
            string manufacturer)
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
                $"{name} {manufacturer}";

            return ContainsAny(
                text,
                "printer",
                "print to pdf",
                "onenote",
                "fax",
                "dcp-",
                "pixma",
                "g2010",
                "g2020",
                "g2030",
                "ecotank",
                "workforce",
                "l3110",
                "l3150",
                "l3210",
                "l3250",
                "l4260",
                "l5290",
                "l550",
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
                "print to pdf",
                "onenote",
                "fax",
                "dcp-",
                "pixma",
                "g2010",
                "g2020",
                "g2030",
                "ecotank",
                "workforce",
                "l3110",
                "l3150",
                "l3210",
                "l3250",
                "l4260",
                "l5290",
                "l550",
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
                    FindExact(
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

                existing.Category =
                    "Printer";

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

                if (!row.Present)
                {
                    existing.Present =
                        false;

                    existing.State =
                        "Not Present";

                    existing.Reason =
                        row.Reason;
                }

                if (row.State == "Offline")
                {
                    existing.State =
                        "Offline";

                    existing.Reason =
                        row.Reason;
                }
            }

            return result;
        }

        private static void AddOrUpdate(
            List<PrintServerPrinterModel> rows,
            PrintServerPrinterModel newRow)
        {
            if (!LooksLikePrinter(
                    newRow.PrinterName,
                    newRow.Category,
                    newRow.DeviceId))
            {
                return;
            }

            PrintServerPrinterModel? existing =
                FindExact(
                    rows,
                    newRow.PrinterName);

            if (existing == null)
            {
                rows.Add(newRow);
                return;
            }

            existing.Source =
                MergeSource(
                    existing.Source,
                    newRow.Source);

            existing.Category =
                "Printer";

            existing.DeviceId =
                FirstNonEmpty(
                    existing.DeviceId,
                    newRow.DeviceId);

            existing.Manufacturer =
                FirstNonEmpty(
                    existing.Manufacturer,
                    newRow.Manufacturer);

            existing.Status =
                FirstNonEmpty(
                    existing.Status,
                    newRow.Status);

            existing.Reason =
                FirstNonEmpty(
                    existing.Reason,
                    newRow.Reason);
        }

        private static PrintServerPrinterModel CreateShellRow(
            string server,
            string name,
            string path,
            string source,
            string type)
        {
            return new PrintServerPrinterModel
            {
                ServerName =
                    server,

                PrinterName =
                    name,

                Category =
                    "Printer",

                Source =
                    source,

                DeviceId =
                    path,

                Manufacturer =
                    DetectManufacturer(
                        name),

                ClassGuid =
                    PrinterClassGuid,

                Status =
                    "Available",

                PrinterState =
                    "Available",

                State =
                    "Printer",

                Reason =
                    string.IsNullOrWhiteSpace(type)
                        ? "Printer found in Windows Devices and Printers."
                        : $"Printer found in Windows ({type}).",

                JobCount =
                    "0",

                Jobs =
                    "",

                Present =
                    true
            };
        }

        // =============================================================
        // HELPERS
        // =============================================================
        private static object? CreateShellApplication()
        {
            Type? shellType =
                Type.GetTypeFromProgID(
                    "Shell.Application");

            if (shellType == null)
                return null;

            return Activator.CreateInstance(
                shellType);
        }

        private static PrintServerPrinterModel? FindExact(
            List<PrintServerPrinterModel> rows,
            string name)
        {
            string key =
                NormalizeName(name);

            return rows.FirstOrDefault(
                item =>
                    NormalizeName(
                        item.PrinterName) == key);
        }

        private static string MergeSource(
            string first,
            string second)
        {
            var parts =
                new List<string>();

            AddPart(parts, first);
            AddPart(parts, second);

            return string.Join(
                " + ",
                parts);
        }

        private static void AddPart(
            List<string> parts,
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            foreach (string part in value.Split(
                '+',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries))
            {
                if (!parts.Contains(
                        part,
                        StringComparer.OrdinalIgnoreCase))
                {
                    parts.Add(part);
                }
            }
        }

        private static string FirstNonEmpty(
            string first,
            string second)
        {
            return !string.IsNullOrWhiteSpace(
                first)
                ? first
                : second ?? "";
        }

        private static string FirstNonEmpty(
            string first,
            string second,
            string third)
        {
            return FirstNonEmpty(
                FirstNonEmpty(
                    first,
                    second),
                third);
        }

        private static string NormalizeName(
            string value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? ""
                : value
                    .Trim()
                    .ToLowerInvariant();
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
            {
                server =
                    server.Substring(
                        0,
                        slash);
            }

            return server.Trim();
        }

        private static bool IsLocalServer(
            string server)
        {
            return
                server.Equals(
                    Environment.MachineName,
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

        private static ManagementScope CreateScope(
            string server)
        {
            return new ManagementScope(
                $@"\\{server}\root\cimv2");
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
            if (ContainsAny(
                    text,
                    "canon"))
                return "Canon";

            if (ContainsAny(
                    text,
                    "epson"))
                return "Epson";

            if (ContainsAny(
                    text,
                    "brother"))
                return "Brother";

            if (ContainsAny(
                    text,
                    "hp ",
                    "hewlett"))
                return "HP";

            if (ContainsAny(
                    text,
                    "lexmark"))
                return "Lexmark";

            if (ContainsAny(
                    text,
                    "xerox"))
                return "Xerox";

            if (ContainsAny(
                    text,
                    "kyocera"))
                return "Kyocera";

            if (ContainsAny(
                    text,
                    "ricoh"))
                return "Ricoh";

            if (ContainsAny(
                    text,
                    "pantum"))
                return "Pantum";

            if (ContainsAny(
                    text,
                    "microsoft"))
                return "Microsoft";

            return "";
        }

        private static string SafeString(
            object? value)
        {
            try
            {
                return value?.ToString()?.Trim()
                       ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static string SafeManagementString(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value =
                    obj[property];

                return value?.ToString()?.Trim()
                       ?? "";
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

                return Convert.ToBoolean(
                    value);
            }
            catch
            {
                return null;
            }
        }

        private static void TryAdd(
            Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // One source failing must not stop the other sources.
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
                    Marshal.FinalReleaseComObject(
                        value);
                }
            }
            catch
            {
            }
        }
    }
}
