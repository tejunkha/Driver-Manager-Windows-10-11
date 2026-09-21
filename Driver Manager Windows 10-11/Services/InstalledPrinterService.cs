using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class InstalledPrinterService
    {
        public static List<InstalledPrinterModel> GetPrinters()
        {
            var result = new List<InstalledPrinterModel>();

            // Reuse the existing printer-driver service so Vendor/Provider
            // can be resolved consistently with the Printer Drivers page.
            var driverProviders = PrinterDriverService.GetDrivers()
                .Where(d => !string.IsNullOrWhiteSpace(d.Name))
                .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            using var searcher = new ManagementObjectSearcher(
                @"SELECT Name, DriverName, PortName, PrinterStatus, WorkOffline,
                         Default, Network, Shared, ShareName, ServerName,
                         Location, Comment, Status, StatusInfo
                  FROM Win32_Printer");

            using ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject printer in collection)
            {
                string name = GetString(printer, "Name");
                string driverName = GetString(printer, "DriverName");
                string portName = GetString(printer, "PortName");

                string vendor = ResolveVendor(driverName, driverProviders);
                string printerType = DetectPrinterType(name, driverName, portName, vendor);

                result.Add(new InstalledPrinterModel
                {
                    Name = name,
                    DriverName = driverName,
                    Vendor = vendor,
                    PrinterType = printerType,
                    PortName = portName,
                    HostAddress = ExtractHostAddress(portName),
                    Status = ResolveStatus(printer),
                    Default = GetBoolText(printer, "Default"),
                    Network = GetBoolText(printer, "Network"),
                    Shared = GetBoolText(printer, "Shared"),
                    WorkOffline = GetBoolText(printer, "WorkOffline"),
                    ServerName = GetString(printer, "ServerName"),
                    Location = GetString(printer, "Location"),
                    Comment = GetString(printer, "Comment")
                });
            }

            result.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private static string ResolveVendor(
            string driverName,
            List<PrinterDriverModel> drivers)
        {
            if (string.IsNullOrWhiteSpace(driverName))
                return "Unknown";

            PrinterDriverModel? exact = drivers.FirstOrDefault(d =>
                d.Name.Equals(driverName, StringComparison.OrdinalIgnoreCase));

            if (exact != null && !string.IsNullOrWhiteSpace(exact.Provider))
                return exact.Provider.Trim();

            // Common Microsoft inbox driver names.
            if (driverName.Contains("Microsoft", StringComparison.OrdinalIgnoreCase))
                return "Microsoft";

            return "Unknown";
        }

        private static string DetectPrinterType(
            string printerName,
            string driverName,
            string portName,
            string vendor)
        {
            string text = $"{printerName} {driverName} {portName}";

            if (text.Contains("IPP", StringComparison.OrdinalIgnoreCase) ||
                portName.Contains("/ipp", StringComparison.OrdinalIgnoreCase) ||
                portName.Contains("ipp/", StringComparison.OrdinalIgnoreCase))
            {
                return vendor.Equals("Microsoft", StringComparison.OrdinalIgnoreCase)
                    ? "IPP (Microsoft)"
                    : "IPP";
            }

            if (text.Contains("WSD", StringComparison.OrdinalIgnoreCase))
                return "WSD";

            if (portName.StartsWith("IP_", StringComparison.OrdinalIgnoreCase) ||
                portName.StartsWith("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
                IsIpAddress(portName))
            {
                return "TCP/IP";
            }

            if (!vendor.Equals("Unknown", StringComparison.OrdinalIgnoreCase) &&
                !vendor.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
            {
                return "Vendor";
            }

            return "Local / Other";
        }

        private static string ExtractHostAddress(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return "";

            string value = portName.Trim();

            // IP_192.168.1.100 style Windows Standard TCP/IP port.
            if (value.StartsWith("IP_", StringComparison.OrdinalIgnoreCase))
                return value.Substring(3);

            if (IPAddress.TryParse(value, out _))
                return value;

            // URI/IPP/WSD style ports.
            if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
                return uri.Host;

            // WSD-xxxxxxxx style name has no reliable host address.
            return "";
        }

        private static bool IsIpAddress(string value)
        {
            return IPAddress.TryParse(value, out _);
        }

        private static string ResolveStatus(ManagementObject printer)
        {
            uint printerStatus = GetUInt32(printer, "PrinterStatus");
            bool offline = GetBool(printer, "WorkOffline");

            if (offline)
                return "Offline";

            return printerStatus switch
            {
                1 => "Other",
                2 => "Unknown",
                3 => "Idle",
                4 => "Printing",
                5 => "Warmup",
                6 => "Stopped",
                7 => "Offline",
                _ => "Unknown"
            };
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
                return value is bool boolValue && boolValue;
            }
            catch
            {
                return false;
            }
        }

        private static string GetBoolText(ManagementObject obj, string property)
        {
            return GetBool(obj, property) ? "Yes" : "No";
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
    }
}
