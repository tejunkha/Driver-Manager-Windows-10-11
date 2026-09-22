using System;
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
    /// All operations are non-force by default and return a readable error message.
    /// </summary>
    public static class DriverActionService
    {
        public static bool IsAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool DeleteDriverStore(
            DriverStoreModel driver,
            out string message)
        {
            message = "";

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
                message = $"ลบ Driver Package {driver.PublishedName} เรียบร้อยแล้ว";
                return true;
            }

            message = $"ไม่สามารถลบ Driver Package {driver.PublishedName} ได้\n\n" +
                       "กรุณาตรวจสอบว่า Driver นี้กำลังถูกใช้งานอยู่หรือไม่ และเปิดโปรแกรมด้วยสิทธิ์ Administrator";
            return false;
        }

        public static bool DeletePrinterDriver(
            PrinterDriverModel driver,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(driver.Name))
            {
                message = "ไม่พบชื่อ Printer Driver";
                return false;
            }

            string name = ToPowerShellSingleQuoted(driver.Name);
            string script =
                $"Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-PrinterDriver -Name '{name}' -RemoveFromDriverStore -Confirm:$false -ErrorAction Stop; " +
                "Write-Output 'ลบ Printer Driver เรียบร้อยแล้ว';";

            return RunPowerShell(script, out message);
        }

        public static bool DeletePrinter(
            InstalledPrinterModel printer,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(printer.Name))
            {
                message = "ไม่พบชื่อ Printer";
                return false;
            }

            string name = ToPowerShellSingleQuoted(printer.Name);
            string script =
                $"Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-Printer -Name '{name}' -Confirm:$false -ErrorAction Stop; " +
                "Write-Output 'ลบ Printer เรียบร้อยแล้ว';";

            return RunPowerShell(script, out message);
        }

        public static bool DeletePrinterPort(
            PrinterPortModel port,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(port.Name))
            {
                message = "ไม่พบชื่อ Printer Port";
                return false;
            }

            string name = ToPowerShellSingleQuoted(port.Name);
            string script =
                $"Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-PrinterPort -Name '{name}' -Confirm:$false -ErrorAction Stop; " +
                "Write-Output 'ลบ Printer Port เรียบร้อยแล้ว';";

            return RunPowerShell(script, out message);
        }

        public static bool DeleteGhostPrinter(
            GhostPrinterModel printer,
            out string message)
        {
            message = "";

            // Prefer removing the non-present PnP device. This does not delete
            // the driver package from the Driver Store.
            if (!string.IsNullOrWhiteSpace(printer.DeviceId))
            {
                string deviceId = printer.DeviceId.Trim();
                string output;
                int exitCode = RunProcess(
                    "pnputil.exe",
                    $"/remove-device \"{deviceId.Replace("\"", "")}" + "\"",
                    out output,
                    60_000);

                if (exitCode == 0)
                {
                    message = "ลบ Ghost Printer Device เรียบร้อยแล้ว";
                    return true;
                }

                message = "ไม่สามารถลบ Ghost Printer Device นี้ได้\n\n" +
                           "กรุณาตรวจสอบสิทธิ์ Administrator และลอง Scan ใหม่อีกครั้ง";
                return false;
            }

            // Some stale printer queues may not expose a PnP device ID.
            // In that case remove the printer queue itself.
            if (!string.IsNullOrWhiteSpace(printer.Name))
            {
                string name = ToPowerShellSingleQuoted(printer.Name);
                string script =
                    $"Import-Module PrintManagement -ErrorAction Stop; " +
                    $"Remove-Printer -Name '{name}' -Confirm:$false -ErrorAction Stop; " +
                    "Write-Output 'ลบ Printer Queue ที่ค้างอยู่เรียบร้อยแล้ว';";

                return RunPowerShell(script, out message);
            }

            message = "Ghost Printer นี้ไม่มี Device ID หรือ Printer Name ที่สามารถลบได้";
            return false;
        }

        public static bool IsPrinterDriverInUse(
            string driverName,
            out string printersUsingDriver)
        {
            printersUsingDriver = "";

            if (string.IsNullOrWhiteSpace(driverName))
                return false;

            try
            {
                var names = new System.Collections.Generic.List<string>();

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
                    printersUsingDriver = string.Join(
                        Environment.NewLine,
                        names);
                    return true;
                }
            }
            catch
            {
                // Usage check is advisory; actual Windows removal still verifies the state.
            }

            return false;
        }

        private static bool IsValidPublishedInf(string value)
        {
            return Regex.IsMatch(
                value?.Trim() ?? "",
                @"^oem\d+\.inf$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool RunPowerShell(
            string script,
            out string message)
        {
            string encoded = Convert.ToBase64String(
                Encoding.Unicode.GetBytes(script));

            string output;
            int exitCode = RunProcess(
                "powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}",
                out output,
                60_000);

            string windowsOutput = output.Trim();

            if (exitCode == 0)
            {
                message = "ดำเนินการเรียบร้อยแล้ว";
                return true;
            }

            message = "ไม่สามารถดำเนินการตามคำสั่งได้\n\n" +
                       "กรุณาตรวจสอบสิทธิ์ Administrator และตรวจสอบว่ารายการนั้นกำลังถูกใช้งานอยู่หรือไม่";

            return false;
        }

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
                StandardOutputEncoding = Encoding.Unicode,
                StandardErrorEncoding = Encoding.Unicode
            };

            process.Start();

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();

            if (!process.WaitForExit(timeoutMilliseconds))
            {
                try { process.Kill(true); } catch { }
                output = "การดำเนินการใช้เวลานานเกินกำหนด";
                return -1;
            }

            output = string.IsNullOrWhiteSpace(standardError)
                ? standardOutput
                : standardOutput + Environment.NewLine + standardError;

            return process.ExitCode;
        }

        private static string ToPowerShellSingleQuoted(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
