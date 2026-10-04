using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// STANDARD Devices & Printers printer inventory.
    ///
    /// The Windows Shell Devices & Printers namespace is the authoritative
    /// source for the visible printer list.
    ///
    /// Printer detection does NOT use printer model names, manufacturers,
    /// ports, or other machine-specific strings. It uses the standard
    /// System.Devices.CategoryIds Shell property and the PrintFax category.
    ///
    /// Win32_Printer is used only to enrich the Shell rows with driver,
    /// port, queue, and status information.
    ///
    /// IMPORTANT:
    /// Win32_Printer / PnP never creates additional rows here.
    /// Therefore the list stays aligned with the printers displayed by
    /// Windows Devices and Printers.
    /// </summary>
    public static class DevicesAndPrintersService
    {
        private const string DevicesAndPrintersNamespace =
            "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\2\\::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";

        private const string PrinterClassGuid =
            "{4D36E979-E325-11CE-BFC1-08002BE10318}";

        private const string CategoryIdsProperty =
            "System.Devices.CategoryIds";

        public static List<PrintServerPrinterModel> GetAll(
            string serverName = "")
        {
            string server =
                NormalizeServerName(serverName);

            if (string.IsNullOrWhiteSpace(server))
            {
                server = Environment.MachineName;
            }

            if (!IsLocalServer(server))
            {
                throw new NotSupportedException(
                    "Devices & Printers inventory is supported for the local computer.");
            }

            Dictionary<string, QueueInfo> queues =
                GetPrinterQueues(server);

            var result =
                new List<PrintServerPrinterModel>();

            EnumerateWindowsPrinterItems(
                server,
                queues,
                result);

            // IMPORTANT:
            // Do NOT de-duplicate by printer name.
            // Windows Devices & Printers can legitimately contain multiple
            // printer containers with the same display name, for example
            // multiple copies of the same printer model.
            //
            // Therefore every Shell item remains a separate row.
            return result
                .OrderBy(
                    item => item.PrinterName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    item => item.DeviceId,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // =============================================================
        // WINDOWS DEVICES & PRINTERS
        // =============================================================
        private static void EnumerateWindowsPrinterItems(
            string server,
            Dictionary<string, QueueInfo> queues,
            List<PrintServerPrinterModel> result)
        {
            object? shellObject =
                CreateShellApplication();

            if (shellObject == null)
            {
                throw new InvalidOperationException(
                    "Shell.Application is not available.");
            }

            dynamic shell = shellObject;
            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                folder =
                    shell.NameSpace(
                        DevicesAndPrintersNamespace);

                if (folder == null)
                {
                    throw new InvalidOperationException(
                        "Windows Devices and Printers namespace could not be opened.");
                }

                items =
                    folder.Items();

                int count =
                    Convert.ToInt32(items.Count);

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
                        {
                            continue;
                        }

                        string name =
                            SafeString(item.Name);

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        string type =
                            SafeString(item.Type);

                        string path =
                            SafeString(item.Path);

                        string deviceInstanceId =
                            GetShellPropertyString(
                                item,
                                "System.Devices.DeviceInstanceId");

                        string containerId =
                            GetShellPropertyString(
                                item,
                                "System.Devices.ContainerId");

                        if (string.IsNullOrWhiteSpace(containerId))
                        {
                            containerId =
                                GetShellPropertyString(
                                    item,
                                    "System.Devices.Aep.ContainerId");
                        }

                        string hardwareIds =
                            GetShellPropertyString(
                                item,
                                "System.Devices.HardwareIds");

                        // STANDARD FILTER:
                        // Windows DeviceContainer category IDs identify
                        // printer/fax devices using the PrintFax category.
                        bool isPrinter =
                            IsPrinterCategory(item);

                        // Some Windows builds expose "Printer" in Type
                        // while CategoryIds are unavailable. This is still
                        // standard metadata, not a model/vendor keyword.
                        if (!isPrinter &&
                            type.Contains(
                                "printer",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            isPrinter = true;
                        }

                        if (!isPrinter)
                        {
                            continue;
                        }

                        string key =
                            NormalizeName(name);

                        QueueInfo? queue =
                            queues.TryGetValue(
                                key,
                                out QueueInfo? found)
                                    ? found
                                    : FindQueueByFriendlyName(
                                        name,
                                        queues);

                        var row =
                            new PrintServerPrinterModel
                            {
                                ServerName =
                                    server,

                                PrinterName =
                                    name,

                                Category =
                                    "Printer",

                                Source =
                                    "Devices & Printers",

                                DeviceId =
                                    FirstNonEmpty(
                                        deviceInstanceId,
                                        hardwareIds,
                                        Safe(queue?.PnpDeviceId)),

                                ShellPath =
                                    path,

                                DeviceInstanceId =
                                    deviceInstanceId,

                                ContainerId =
                                    containerId,

                                Manufacturer =
                                    Safe(queue?.Manufacturer),

                                DriverName =
                                    Safe(queue?.DriverName),

                                PortName =
                                    Safe(queue?.PortName),

                                ShareName =
                                    Safe(queue?.ShareName),

                                Status =
                                    Safe(queue?.Status),

                                PrinterState =
                                    Safe(queue?.Status),

                                State =
                                    "Printer",

                                Reason =
                                    string.IsNullOrWhiteSpace(type)
                                        ? "Printer shown in Windows Devices & Printers."
                                        : $"Printer shown in Windows Devices & Printers ({type}).",

                                JobCount =
                                    "0",

                                Jobs =
                                    "",

                                Present =
                                    true,

                                Shared =
                                    queue?.Shared ?? false,

                                Published =
                                    queue?.Published ?? false,

                                WorkOffline =
                                    queue?.WorkOffline ?? false,

                                ClassGuid =
                                    PrinterClassGuid
                            };

                        if (row.WorkOffline)
                        {
                            row.State =
                                "Offline";

                            row.Reason =
                                "Windows reports this printer queue as Work Offline.";
                        }

                        result.Add(row);
                    }
                    catch
                    {
                        // Ignore one Shell item and continue.
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

        private static bool IsPrinterCategory(
            dynamic item)
        {
            try
            {
                object? value =
                    item.ExtendedProperty(
                        CategoryIdsProperty);

                foreach (string category in
                         ToStringValues(value))
                {
                    // Windows' standard printer category is in the
                    // PrintFax category family. No vendor/model names
                    // are required.
                    if (category.StartsWith(
                            "PrintFax",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Property may be unavailable for a particular Shell item.
            }

            return false;
        }

        private static IEnumerable<string> ToStringValues(
            object? value)
        {
            if (value == null)
            {
                yield break;
            }

            if (value is string text)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    yield return text.Trim();
                }

                yield break;
            }

            if (value is Array array)
            {
                foreach (object? element in array)
                {
                    string? textValue =
                        element?.ToString();

                    if (!string.IsNullOrWhiteSpace(
                            textValue))
                    {
                        yield return textValue.Trim();
                    }
                }

                yield break;
            }

            string fallback =
                value.ToString() ?? "";

            foreach (string line in fallback
                .Split(
                    new[] { ';', ',', '|', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries))
            {
                yield return line;
            }
        }

        private static string GetShellPropertyString(
            dynamic item,
            string propertyName)
        {
            try
            {
                object? value =
                    item.ExtendedProperty(propertyName);

                if (value == null)
                    return "";

                if (value is Array array)
                {
                    foreach (object? element in array)
                    {
                        string text =
                            SafeString(element);

                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }

                    return "";
                }

                return SafeString(value);
            }
            catch
            {
                return "";
            }
        }

        // =============================================================
        // WIN32_PRINTER - ENRICHMENT ONLY
        // =============================================================
        private static Dictionary<string, QueueInfo> GetPrinterQueues(
            string server)
        {
            var result =
                new Dictionary<string, QueueInfo>(
                    StringComparer.OrdinalIgnoreCase);

            try
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
                            "PNPDeviceID FROM Win32_Printer"));

                using ManagementObjectCollection collection =
                    searcher.Get();

                foreach (ManagementObject printer in collection)
                {
                    try
                    {
                        string name =
                            SafeManagementString(
                                printer,
                                "Name");

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        result[
                            NormalizeName(name)] =
                            new QueueInfo
                            {
                                Name =
                                    name,

                                DriverName =
                                    SafeManagementString(
                                        printer,
                                        "DriverName"),

                                PortName =
                                    SafeManagementString(
                                        printer,
                                        "PortName"),

                                ShareName =
                                    SafeManagementString(
                                        printer,
                                        "ShareName"),

                                Status =
                                    SafeManagementString(
                                        printer,
                                        "Status"),

                                Shared =
                                    SafeManagementBool(
                                        printer,
                                        "Shared"),

                                Published =
                                    SafeManagementBool(
                                        printer,
                                        "Published"),

                                WorkOffline =
                                    SafeManagementBool(
                                        printer,
                                        "WorkOffline"),

                                PnpDeviceId =
                                    SafeManagementString(
                                        printer,
                                        "PNPDeviceID")
                            };
                    }
                    finally
                    {
                        printer.Dispose();
                    }
                }
            }
            catch
            {
                // Devices & Printers remains usable even if WMI
                // enrichment is unavailable.
            }

            return result;
        }

        private static QueueInfo? FindQueueByFriendlyName(
            string shellName,
            Dictionary<string, QueueInfo> queues)
        {
            string shellKey =
                NormalizeName(shellName);

            foreach (QueueInfo queue
                     in queues.Values)
            {
                if (NormalizeName(queue.Name) ==
                    shellKey)
                {
                    return queue;
                }
            }

            return null;
        }

        // =============================================================
        // HELPERS
        // =============================================================
        private sealed class QueueInfo
        {
            public string Name { get; set; } = "";
            public string DriverName { get; set; } = "";
            public string PortName { get; set; } = "";
            public string ShareName { get; set; } = "";
            public string Status { get; set; } = "";
            public string PnpDeviceId { get; set; } = "";
            public string Manufacturer { get; set; } = "";
            public bool Shared { get; set; }
            public bool Published { get; set; }
            public bool WorkOffline { get; set; }

            // Kept for future enrichment without changing the standard
            // detection logic.
        }

        private static object? CreateShellApplication()
        {
            Type? shellType =
                Type.GetTypeFromProgID(
                    "Shell.Application");

            if (shellType == null)
            {
                return null;
            }

            return Activator.CreateInstance(
                shellType);
        }

        private static ManagementScope CreateScope(
            string server)
        {
            return new ManagementScope(
                $@"\\{server}\root\cimv2");
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

        private static string NormalizeName(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value
                    .Trim()
                    .ToLowerInvariant();
        }

        private static string Safe(
            string? value)
        {
            return value?.Trim() ?? "";
        }

        private static string SafeString(
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

        private static string FirstNonEmpty(
            params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return "";
        }
    }
}
