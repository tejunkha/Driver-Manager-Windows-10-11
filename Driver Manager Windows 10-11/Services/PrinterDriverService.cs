using System;
using System.Collections.Generic;
using System.Management;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class PrinterDriverService
    {
        public static List<PrinterDriverModel> GetDrivers()
        {
            var drivers = new List<PrinterDriverModel>();

            try
            {
                // =====================================================
                // สร้างข้อมูล Provider จาก PnP Signed Driver
                // =====================================================
                var providerByInf =
                    new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase);

                var providerByDriverName =
                    new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase);

                using (var searcher = new ManagementObjectSearcher(
                    @"root\CIMV2",
                    @"
                    SELECT
                        InfName,
                        Manufacturer,
                        DriverName,
                        DeviceName
                    FROM Win32_PnPSignedDriver
                    WHERE InfName IS NOT NULL
                    "))
                {
                    foreach (ManagementObject item in searcher.Get())
                    {
                        try
                        {
                            string infName =
                                GetString(item, "InfName");

                            string manufacturer =
                                GetString(item, "Manufacturer");

                            string driverName =
                                GetString(item, "DriverName");

                            string deviceName =
                                GetString(item, "DeviceName");

                            if (!string.IsNullOrWhiteSpace(manufacturer))
                            {
                                if (!string.IsNullOrWhiteSpace(infName) &&
                                    !providerByInf.ContainsKey(infName))
                                {
                                    providerByInf[infName] =
                                        manufacturer;
                                }

                                if (!string.IsNullOrWhiteSpace(driverName) &&
                                    !providerByDriverName.ContainsKey(driverName))
                                {
                                    providerByDriverName[driverName] =
                                        manufacturer;
                                }

                                if (!string.IsNullOrWhiteSpace(deviceName) &&
                                    !providerByDriverName.ContainsKey(deviceName))
                                {
                                    providerByDriverName[deviceName] =
                                        manufacturer;
                                }
                            }
                        }
                        catch
                        {
                            // ข้ามรายการที่อ่านไม่ได้
                        }
                    }
                }

                // =====================================================
                // อ่าน Printer Driver จริง
                // =====================================================
                const string query = @"
                    SELECT
                        Name,
                        InfName,
                        DriverPath,
                        FilePath,
                        DataFile,
                        ConfigFile,
                        MonitorName,
                        SupportedPlatform,
                        Version,
                        Status
                    FROM Win32_PrinterDriver
                ";

                using (var searcher = new ManagementObjectSearcher(
                    @"root\CIMV2",
                    query))
                {
                    foreach (ManagementObject item
                        in searcher.Get())
                    {
                        try
                        {
                            string name =
                                GetString(item, "Name");

                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            string infName =
                                GetString(item, "InfName");

                            string provider = "";

                            // =================================================
                            // 1. หา Provider จาก INF
                            // =================================================
                            if (!string.IsNullOrWhiteSpace(infName))
                            {
                                providerByInf.TryGetValue(
                                    infName,
                                    out provider);
                            }

                            // =================================================
                            // 2. ถ้าไม่พบ ให้หาโดยชื่อ Driver
                            // =================================================
                            if (string.IsNullOrWhiteSpace(provider))
                            {
                                providerByDriverName.TryGetValue(
                                    name,
                                    out provider);
                            }

                            // =================================================
                            // 3. Fallback จากชื่อ Driver
                            // =================================================
                            if (string.IsNullOrWhiteSpace(provider))
                            {
                                provider =
                                    GetProviderFromName(name);
                            }

                            // =================================================
                            // Win32_PrinterDriver.Version
                            // ใช้เป็น Driver Type
                            // =================================================
                            string version =
                                GetString(item, "Version");

                            string driverType =
                                GetDriverType(version);

                            // =================================================
                            // Status
                            // =================================================
                            string status =
                                GetString(item, "Status");

                            if (string.IsNullOrWhiteSpace(status))
                            {
                                status = "Installed";
                            }

                            string driverPath =
                                GetString(
                                    item,
                                    "DriverPath");

                            string filePath =
                                GetString(
                                    item,
                                    "FilePath");

                            string dataFile =
                                GetString(
                                    item,
                                    "DataFile");

                            string configFile =
                                GetString(
                                    item,
                                    "ConfigFile");

                            string monitorName =
                                GetString(
                                    item,
                                    "MonitorName");

                            string environment =
                                GetString(
                                    item,
                                    "SupportedPlatform");

                            drivers.Add(
                                new PrinterDriverModel
                                {
                                    Name = name,

                                    Provider =
                                        provider ?? "",

                                    // เก็บ Type 3 / Type 4
                                    Version =
                                        driverType,

                                    InfName =
                                        infName,

                                    DriverPath =
                                        driverPath,

                                    FilePath =
                                        filePath,

                                    DataFile =
                                        dataFile,

                                    ConfigFile =
                                        configFile,

                                    MonitorName =
                                        monitorName,

                                    SupportedPlatform =
                                        environment,

                                    Status =
                                        status
                                });
                        }
                        catch
                        {
                            // ข้ามรายการที่อ่านไม่ได้
                        }
                    }
                }

                // =====================================================
                // เรียงตามชื่อ
                // =====================================================
                drivers.Sort((a, b) =>
                    string.Compare(
                        a.Name,
                        b.Name,
                        StringComparison.OrdinalIgnoreCase));

                return drivers;
            }
            catch (ManagementException ex)
            {
                throw new Exception(
                    "ไม่สามารถอ่าน Printer Driver จาก Windows ได้\n\n" +
                    ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception(
                    "ไม่มีสิทธิ์เข้าถึงข้อมูล Printer Driver");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "เกิดข้อผิดพลาดในการอ่าน Printer Driver\n\n" +
                    ex.Message);
            }
        }

        // =========================================================
        // Get String
        // =========================================================
        private static string GetString(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value = obj[property];

                return value?.ToString()?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        // =========================================================
        // Driver Type
        // =========================================================
        private static string GetDriverType(
            string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "Unknown";

            if (version == "3")
                return "Type 3";

            if (version == "4")
                return "Type 4";

            return $"Type {version}";
        }

        // =========================================================
        // Provider Fallback
        // =========================================================
        private static string GetProviderFromName(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            string upper =
                name.ToUpperInvariant();

            string[] providers =
            {
                "MICROSOFT",
                "HP",
                "CANON",
                "BROTHER",
                "EPSON",
                "LEXMARK",
                "RICOH",
                "XEROX",
                "KYOCERA",
                "SAMSUNG",
                "OKI",
                "SHARP",
                "TOSHIBA",
                "KONICA MINOLTA"
            };

            foreach (string provider in providers)
            {
                if (upper.StartsWith(provider))
                {
                    return provider
                        .Replace(
                            "MICROSOFT",
                            "Microsoft");
                }
            }

            return "";
        }
    }
}