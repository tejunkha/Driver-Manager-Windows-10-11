using System;
using System.Collections.Generic;
using System.Management;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class SystemDriverService
    {
        public static List<SystemDriverModel> GetDrivers()
        {
            var drivers = new List<SystemDriverModel>();

            try
            {
                // =====================================================
                // โหลดสถานะ Device จาก Win32_PnPEntity
                // =====================================================
                var deviceStatus = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

                using (var statusSearcher = new ManagementObjectSearcher(
                    @"root\CIMV2",
                    "SELECT DeviceID, Status, ConfigManagerErrorCode FROM Win32_PnPEntity"))
                {
                    foreach (ManagementObject device in statusSearcher.Get())
                    {
                        try
                        {
                            string deviceId =
                                GetString(device, "DeviceID");

                            if (string.IsNullOrWhiteSpace(deviceId))
                                continue;

                            uint errorCode =
                                GetUInt32(device, "ConfigManagerErrorCode");

                            string status;

                            if (errorCode == 0)
                            {
                                status = "OK";
                            }
                            else
                            {
                                status = $"Problem ({errorCode})";
                            }

                            deviceStatus[deviceId] = status;
                        }
                        catch
                        {
                            // ข้าม device ที่อ่านไม่ได้
                        }
                    }
                }

                // =====================================================
                // โหลด Driver จาก Win32_PnPSignedDriver
                // =====================================================
                const string query = @"
                    SELECT
                        DeviceName,
                        Manufacturer,
                        DriverVersion,
                        DriverDate,
                        InfName,
                        IsSigned,
                        DeviceID,
                        DriverName,
                        DeviceClass,
                        Location
                    FROM Win32_PnPSignedDriver
                    WHERE DeviceID IS NOT NULL
                ";

                using (var searcher = new ManagementObjectSearcher(
                    @"root\CIMV2",
                    query))
                {
                    foreach (ManagementObject driver in searcher.Get())
                    {
                        try
                        {
                            string deviceId =
                                GetString(driver, "DeviceID");

                            string deviceName =
                                GetString(driver, "DeviceName");

                            if (string.IsNullOrWhiteSpace(deviceName))
                            {
                                deviceName =
                                    GetString(driver, "Description");
                            }

                            string manufacturer =
                                GetString(driver, "Manufacturer");

                            string driverVersion =
                                GetString(driver, "DriverVersion");

                            string infName =
                                GetString(driver, "InfName");

                            string driverName =
                                GetString(driver, "DriverName");

                            string deviceClass =
                                GetString(driver, "DeviceClass");

                            string location =
                                GetString(driver, "Location");

                            bool signed =
                                GetBoolean(driver, "IsSigned");

                            string driverDate =
                                FormatWmiDate(
                                    GetString(driver, "DriverDate"));

                            string status = "Unknown";

                            if (!string.IsNullOrWhiteSpace(deviceId) &&
                                deviceStatus.TryGetValue(
                                    deviceId,
                                    out string? foundStatus))
                            {
                                status = foundStatus;
                            }

                            drivers.Add(new SystemDriverModel
                            {
                                Device = deviceName,
                                Manufacturer = manufacturer,
                                DriverVersion = driverVersion,
                                DriverDate = driverDate,
                                InfName = infName,
                                Status = status,
                                Signed = signed ? "Yes" : "No",
                                DeviceId = deviceId,
                                DriverName = driverName,
                                DeviceClass = deviceClass,
                                Location = location
                            });
                        }
                        catch
                        {
                            // ข้ามรายการที่อ่านไม่ได้
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                throw new Exception(
                    "ไม่สามารถอ่านข้อมูล Windows WMI ได้\n\n" +
                    "ตรวจสอบว่า Windows Management Instrumentation " +
                    "ทำงานอยู่");
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception(
                    "ไม่มีสิทธิ์เข้าถึงข้อมูล Driver");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "เกิดข้อผิดพลาดในการอ่าน Driver:\n" +
                    ex.Message);
            }

            // =====================================================
            // เรียงตามชื่อ Device
            // =====================================================
            drivers.Sort((a, b) =>
                string.Compare(
                    a.Device,
                    b.Device,
                    StringComparison.OrdinalIgnoreCase));

            return drivers;
        }

        // ==========================================================
        // Get String
        // ==========================================================
        private static string GetString(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value = obj[property];

                return value?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        // ==========================================================
        // Get UInt32
        // ==========================================================
        private static uint GetUInt32(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value = obj[property];

                if (value == null)
                    return 0;

                return Convert.ToUInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        // ==========================================================
        // Get Boolean
        // ==========================================================
        private static bool GetBoolean(
            ManagementObject obj,
            string property)
        {
            try
            {
                object? value = obj[property];

                if (value == null)
                    return false;

                return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        // ==========================================================
        // WMI DateTime
        // ==========================================================
        private static string FormatWmiDate(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            try
            {
                DateTime date =
                    ManagementDateTimeConverter.ToDateTime(value);

                return date.ToString("dd/MM/yyyy");
            }
            catch
            {
                // บางเครื่องอาจส่งค่าที่ไม่ใช่ WMI datetime
                if (value.Length >= 8)
                    return value.Substring(0, 8);

                return value;
            }
        }
    }
}