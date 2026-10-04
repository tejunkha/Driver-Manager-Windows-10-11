using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Destructive actions used by the Driver Manager.
    /// Windows objects are removed using native Windows management commands.
    /// Deleting a printer/device does NOT remove the Driver Store package.
    /// </summary>
    public static class DriverActionService
    {
        // =========================================================
        // ADMINISTRATOR
        // =========================================================
        public static bool IsAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // =========================================================
        // DRIVER STORE
        // =========================================================
        public static bool DeleteDriverStore(
            DriverStoreModel driver,
            out string message)
        {
            message = "";

            if (driver == null)
            {
                message = "ไม่พบข้อมูล Driver Store";
                return false;
            }

            if (!IsValidPublishedInf(driver.PublishedName))
            {
                message = "ชื่อ Driver Store Package ไม่ถูกต้อง";
                return false;
            }

            string output;

            int exitCode = RunProcess(
                "pnputil.exe",
                $"/delete-driver \"{driver.PublishedName.Trim()}\" /uninstall",
                out output,
                60_000);

            if (exitCode == 0)
            {
                message =
                    $"ลบ Driver Package {driver.PublishedName} เรียบร้อยแล้ว";
                return true;
            }

            message = string.IsNullOrWhiteSpace(output)
                ? $"Windows ไม่สามารถลบ {driver.PublishedName} ได้"
                : "Windows ไม่สามารถลบ Driver Package นี้ได้\n\n" +
                  "รายละเอียดจาก Windows:\n" + output.Trim();

            return false;
        }

        // =========================================================
        // PRINTER DRIVER
        // =========================================================
        public static bool DeletePrinterDriver(
            PrinterDriverModel driver,
            out string message)
        {
            message = "";

            if (driver == null)
            {
                message = "ไม่พบข้อมูล Printer Driver";
                return false;
            }

            if (string.IsNullOrWhiteSpace(driver.Name))
            {
                message = "ไม่พบชื่อ Printer Driver";
                return false;
            }

            string name = ToPowerShellSingleQuoted(driver.Name);

            string script =
                "Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-PrinterDriver -Name '{name}' " +
                "-RemoveFromDriverStore " +
                "-Confirm:$false " +
                "-ErrorAction Stop; " +
                "Write-Output 'Printer driver removed successfully.';";

            return RunPowerShell(script, out message);
        }

        // =========================================================
        // INSTALLED PRINTER
        // =========================================================
        public static bool DeletePrinter(
            InstalledPrinterModel printer,
            out string message)
        {
            message = "";

            if (printer == null)
            {
                message = "ไม่พบข้อมูล Printer";
                return false;
            }

            if (string.IsNullOrWhiteSpace(printer.Name))
            {
                message = "ไม่พบชื่อ Printer";
                return false;
            }

            string name = ToPowerShellSingleQuoted(printer.Name);

            string script =
                "Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-Printer -Name '{name}' " +
                "-Confirm:$false " +
                "-ErrorAction Stop; " +
                "Write-Output 'Printer removed successfully.';";

            return RunPowerShell(script, out message);
        }

        // =========================================================
        // PRINTER PORT
        // =========================================================
        public static bool DeletePrinterPort(
            PrinterPortModel port,
            out string message)
        {
            message = "";

            if (port == null)
            {
                message = "ไม่พบข้อมูล Printer Port";
                return false;
            }

            if (string.IsNullOrWhiteSpace(port.Name))
            {
                message = "ไม่พบชื่อ Printer Port";
                return false;
            }

            string name = ToPowerShellSingleQuoted(port.Name);

            string script =
                "Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-PrinterPort -Name '{name}' " +
                "-Confirm:$false " +
                "-ErrorAction Stop; " +
                "Write-Output 'Printer port removed successfully.';";

            return RunPowerShell(script, out message);
        }

        // =========================================================
        // GHOST PRINTER
        // =========================================================
        public static bool DeleteGhostPrinter(
            GhostPrinterModel printer,
            out string message)
        {
            message = "";

            if (printer == null)
            {
                message = "ไม่พบข้อมูล Ghost Printer";
                return false;
            }

            // Prefer removing the PnP Device.
            if (!string.IsNullOrWhiteSpace(printer.DeviceId))
            {
                string deviceId = printer.DeviceId.Trim();
                string output;

                int exitCode = RunProcess(
                    "pnputil.exe",
                    $"/remove-device \"{SanitizeCommandValue(deviceId)}\"",
                    out output,
                    60_000);

                if (exitCode == 0)
                {
                    message = "ลบ Ghost Printer Device เรียบร้อยแล้ว";
                    return true;
                }

                message = string.IsNullOrWhiteSpace(output)
                    ? "Windows ไม่สามารถลบ Ghost Printer Device นี้ได้"
                    : "Windows ไม่สามารถลบ Ghost Printer Device นี้ได้\n\n" +
                      "รายละเอียดจาก Windows:\n" + output.Trim();

                return false;
            }

            // Fallback: remove printer queue.
            if (!string.IsNullOrWhiteSpace(printer.Name))
            {
                string name = ToPowerShellSingleQuoted(printer.Name);

                string script =
                    "Import-Module PrintManagement -ErrorAction Stop; " +
                    $"Remove-Printer -Name '{name}' " +
                    "-Confirm:$false " +
                    "-ErrorAction Stop; " +
                    "Write-Output 'Stale printer queue removed successfully.';";

                return RunPowerShell(script, out message);
            }

            message =
                "Ghost Printer นี้ไม่มี Device ID หรือ Printer Name ที่สามารถลบได้";
            return false;
        }

        // =========================================================
        // DEVICES & PRINTERS
        // =========================================================
        public static bool DeleteDevicesAndPrintersPrinter(
            PrintServerPrinterModel printer,
            out string message)
        {
            message = "";

            if (printer == null)
            {
                message = "ไม่พบข้อมูล Printer / Device ที่เลือก";
                return false;
            }

            string printerName = printer.PrinterName?.Trim() ?? "";
            string deviceId = printer.DeviceId?.Trim() ?? "";

            // -----------------------------------------------------
            // 1. If DeviceId is a real PnP device ID, remove device.
            // -----------------------------------------------------
            if (IsLikelyPnpDeviceId(deviceId))
            {
                string output;

                int exitCode = RunProcess(
                    "pnputil.exe",
                    $"/remove-device \"{SanitizeCommandValue(deviceId)}\"",
                    out output,
                    60_000);

                if (exitCode == 0)
                {
                    message =
                        "ลบ Printer Device ออกจาก Windows เรียบร้อยแล้ว\n" +
                        "Driver Store ไม่ถูกลบ";

                    return true;
                }
            }

            // -----------------------------------------------------
            // 2. If a real printer queue exists, remove that queue.
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                int queueCount = GetPrinterQueueCount(
                    printerName,
                    out string queueError);

                if (queueCount > 1)
                {
                    message =
                        "พบ Printer Queue ชื่อเดียวกันมากกว่า 1 รายการ\n\n" +
                        "เพื่อป้องกันการลบผิดรายการ โปรแกรมจึงหยุดการทำงาน\n\n" +
                        "รายละเอียด:\n" + queueError;

                    return false;
                }

                if (queueCount == 1)
                {
                    string name = ToPowerShellSingleQuoted(printerName);

                    string script =
                        "Import-Module PrintManagement -ErrorAction Stop; " +
                        $"$p = Get-Printer -Name '{name}' -ErrorAction SilentlyContinue; " +
                        "if ($null -eq $p) { " +
                        "throw 'ไม่พบ Printer Queue ที่ระบุ'; " +
                        "}; " +
                        $"Remove-Printer -Name '{name}' " +
                        "-Confirm:$false " +
                        "-ErrorAction Stop; " +
                        "Write-Output 'Printer queue removed successfully.';";

                    bool success = RunPowerShell(script, out message);

                    if (success)
                    {
                        message =
                            "ลบ Printer ออกจาก Windows เรียบร้อยแล้ว\n" +
                            "Driver Store ไม่ถูกลบ";
                    }

                    return success;
                }

                // -------------------------------------------------
                // 3. No queue: try to locate one PnP printer device.
                // -------------------------------------------------
                List<string> pnpIds = FindPrinterPnpDeviceIds(printerName);

                if (pnpIds.Count == 1)
                {
                    string output;

                    int exitCode = RunProcess(
                        "pnputil.exe",
                        $"/remove-device \"{SanitizeCommandValue(pnpIds[0])}\"",
                        out output,
                        60_000);

                    if (exitCode == 0)
                    {
                        message =
                            "ลบ Printer Device ออกจาก Windows เรียบร้อยแล้ว\n" +
                            "Driver Store ไม่ถูกลบ";

                        return true;
                    }

                    message = string.IsNullOrWhiteSpace(output)
                        ? "Windows ไม่สามารถลบ Printer Device นี้ได้"
                        : "Windows ไม่สามารถลบ Printer Device นี้ได้\n\n" +
                          "รายละเอียดจาก Windows:\n" + output.Trim();

                    return false;
                }

                if (pnpIds.Count > 1)
                {
                    message =
                        "พบ Printer Device ที่ชื่อเหมือนกันหลายรายการ\n\n" +
                        "เพื่อป้องกันการลบผิด Device โปรแกรมจึงไม่ลบอัตโนมัติ";

                    return false;
                }

                message =
                    "ไม่พบ Printer Queue หรือ Printer Device ที่สามารถลบได้";

                if (!string.IsNullOrWhiteSpace(queueError))
                {
                    message +=
                        "\n\nรายละเอียด:\n" + queueError;
                }

                return false;
            }

            message =
                "รายการนี้ไม่มี Printer Name หรือ Device ID ที่สามารถลบได้";

            return false;
        }

        // =========================================================
        // CHECK PRINTER DRIVER IN USE
        // =========================================================
        public static bool IsPrinterDriverInUse(
            string driverName,
            out string printersUsingDriver)
        {
            printersUsingDriver = "";

            if (string.IsNullOrWhiteSpace(driverName))
                return false;

            try
            {
                var names = new List<string>();

                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, DriverName FROM Win32_Printer");

                using ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject printer in collection)
                {
                    string currentDriver =
                        Convert.ToString(printer["DriverName"])?.Trim() ?? "";

                    if (currentDriver.Equals(
                        driverName.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        string name =
                            Convert.ToString(printer["Name"])?.Trim() ?? "";

                        if (!string.IsNullOrWhiteSpace(name))
                            names.Add(name);
                    }
                }

                if (names.Count > 0)
                {
                    printersUsingDriver =
                        string.Join(Environment.NewLine, names);
                    return true;
                }
            }
            catch
            {
                // Advisory check only.
            }

            return false;
        }

        // =========================================================
        // FIND PRINTER QUEUE COUNT
        // =========================================================
        private static int GetPrinterQueueCount(
            string printerName,
            out string error)
        {
            error = "";

            try
            {
                string name = ToPowerShellSingleQuoted(printerName);

                string script =
                    "Import-Module PrintManagement -ErrorAction Stop; " +
                    $"$items = @(Get-Printer -Name '{name}' -ErrorAction SilentlyContinue); " +
                    "Write-Output ([string]$items.Count);";

                string output;

                int exitCode = RunProcess(
                    "powershell.exe",
                    BuildEncodedPowerShellArguments(script),
                    out output,
                    30_000);

                if (exitCode != 0)
                {
                    error = string.IsNullOrWhiteSpace(output)
                        ? "ไม่สามารถตรวจสอบ Printer Queue ได้"
                        : output.Trim();

                    return -1;
                }

                if (int.TryParse(output.Trim(), out int count))
                    return count;

                error =
                    "Windows ส่งค่าจำนวน Printer Queue ที่ไม่ถูกต้อง";
                return -1;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }
        }

        // =========================================================
        // FIND PNP PRINTER DEVICES BY NAME
        // =========================================================
        private static List<string> FindPrinterPnpDeviceIds(
            string printerName)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(printerName))
                return result;

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT PNPDeviceID, Name, Caption, ClassGuid, PNPClass " +
                    "FROM Win32_PnPEntity");

                using ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject device in collection)
                {
                    string pnpClass =
                        Convert.ToString(device["PNPClass"])?.Trim() ?? "";

                    string classGuid =
                        Convert.ToString(device["ClassGuid"])?.Trim() ?? "";

                    bool printerClass =
                        pnpClass.Equals(
                            "Printer",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        classGuid.Equals(
                            "{4D36E979-E325-11CE-BFC1-08002BE10318}",
                            StringComparison.OrdinalIgnoreCase);

                    if (!printerClass)
                        continue;

                    string pnpId =
                        Convert.ToString(device["PNPDeviceID"])?.Trim() ?? "";

                    string name =
                        Convert.ToString(device["Name"])?.Trim() ?? "";

                    string caption =
                        Convert.ToString(device["Caption"])?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(pnpId))
                        continue;

                    bool match =
                        name.Equals(
                            printerName,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        caption.Equals(
                            printerName,
                            StringComparison.OrdinalIgnoreCase);

                    if (!match)
                    {
                        match =
                            name.Contains(
                                printerName,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            caption.Contains(
                                printerName,
                                StringComparison.OrdinalIgnoreCase);
                    }

                    if (match &&
                        !result.Contains(
                            pnpId,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        result.Add(pnpId);
                    }
                }
            }
            catch
            {
                // Return empty list.
            }

            return result;
        }

        // =========================================================
        // CHECK LIKELY PNP DEVICE ID
        // =========================================================
        private static bool IsLikelyPnpDeviceId(
            string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return false;

            string value = deviceId.Trim();

            string[] prefixes =
            {
                "USBPRINT\\",
                "USB\\",
                "SWD\\",
                "ROOT\\",
                "BTH\\",
                "DOT4\\",
                "LPTENUM\\",
                "WSD\\",
                "PRINTENUM\\"
            };

            foreach (string prefix in prefixes)
            {
                if (value.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // VALIDATE DRIVER STORE INF
        // =========================================================
        private static bool IsValidPublishedInf(
            string value)
        {
            return Regex.IsMatch(
                value?.Trim() ?? "",
                @"^oem\d+\.inf$",
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant);
        }

        // =========================================================
        // POWERSHELL
        // =========================================================
        private static bool RunPowerShell(
            string script,
            out string message)
        {
            string encoded = Convert.ToBase64String(
                Encoding.Unicode.GetBytes(script));

            string output;

            int exitCode = RunProcess(
                "powershell.exe",
                $"-NoLogo -NoProfile -NonInteractive " +
                $"-ExecutionPolicy Bypass " +
                $"-EncodedCommand {encoded}",
                out output,
                60_000);

            string windowsOutput = output.Trim();

            if (exitCode == 0)
            {
                message = "ดำเนินการเรียบร้อยแล้ว";
                return true;
            }

            message = string.IsNullOrWhiteSpace(windowsOutput)
                ? "Windows ไม่สามารถดำเนินการตามคำสั่งได้"
                : "Windows ไม่สามารถดำเนินการตามคำสั่งได้\n\n" +
                  "รายละเอียดจาก Windows:\n" + windowsOutput;

            return false;
        }

        // =========================================================
        // BUILD POWERSHELL ARGUMENTS
        // =========================================================
        private static string BuildEncodedPowerShellArguments(
            string script)
        {
            string encoded = Convert.ToBase64String(
                Encoding.Unicode.GetBytes(script));

            return
                "-NoLogo -NoProfile -NonInteractive " +
                "-ExecutionPolicy Bypass " +
                "-EncodedCommand " +
                encoded;
        }

        // =========================================================
        // RUN PROCESS
        // =========================================================
        private static int RunProcess(
            string fileName,
            string arguments,
            out string output,
            int timeoutMilliseconds)
        {
            using var process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            process.Start();

            string standardOutput =
                process.StandardOutput.ReadToEnd();

            string standardError =
                process.StandardError.ReadToEnd();

            if (!process.WaitForExit(timeoutMilliseconds))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                }

                output = "การดำเนินการใช้เวลานานเกินกำหนด";
                return -1;
            }

            output = string.IsNullOrWhiteSpace(standardError)
                ? standardOutput
                : standardOutput + Environment.NewLine + standardError;

            return process.ExitCode;
        }

        // =========================================================
        // POWERSHELL SINGLE QUOTE
        // =========================================================
        private static string ToPowerShellSingleQuoted(
            string value)
        {
            return (value ?? "").Replace("'", "''");
        }

        // =========================================================
        // COMMAND VALUE
        // =========================================================
        private static string SanitizeCommandValue(
            string value)
        {
            return (value ?? "")
                .Replace("\"", "")
                .Trim();
        }
    }
}
