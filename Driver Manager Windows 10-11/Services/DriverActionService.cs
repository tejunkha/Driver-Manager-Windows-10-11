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
                message = "Invalid Driver Store package name.";
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
                message = string.IsNullOrWhiteSpace(output)
                    ? $"Driver package {driver.PublishedName} was removed."
                    : output.Trim();
                return true;
            }

            message = string.IsNullOrWhiteSpace(output)
                ? $"Windows could not remove {driver.PublishedName}."
                : output.Trim();
            return false;
        }

        public static bool DeletePrinterDriver(
            PrinterDriverModel driver,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(driver.Name))
            {
                message = "Printer driver name is empty.";
                return false;
            }

            string name = ToPowerShellSingleQuoted(driver.Name);
            string script =
                $"Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-PrinterDriver -Name '{name}' -RemoveFromDriverStore -Confirm:$false -ErrorAction Stop; " +
                "Write-Output 'Printer driver removed successfully.';";

            return RunPowerShell(script, out message);
        }

        public static bool DeletePrinter(
            InstalledPrinterModel printer,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(printer.Name))
            {
                message = "Printer name is empty.";
                return false;
            }

            string name = ToPowerShellSingleQuoted(printer.Name);
            string script =
                $"Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-Printer -Name '{name}' -Confirm:$false -ErrorAction Stop; " +
                "Write-Output 'Printer removed successfully.';";

            return RunPowerShell(script, out message);
        }

        public static bool DeletePrinterPort(
            PrinterPortModel port,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(port.Name))
            {
                message = "Printer port name is empty.";
                return false;
            }

            string name = ToPowerShellSingleQuoted(port.Name);
            string script =
                $"Import-Module PrintManagement -ErrorAction Stop; " +
                $"Remove-PrinterPort -Name '{name}' -Confirm:$false -ErrorAction Stop; " +
                "Write-Output 'Printer port removed successfully.';";

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
                    message = string.IsNullOrWhiteSpace(output)
                        ? "Ghost printer device removed successfully."
                        : output.Trim();
                    return true;
                }

                message = string.IsNullOrWhiteSpace(output)
                    ? "Windows could not remove the non-present printer device."
                    : output.Trim();
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
                    "Write-Output 'Stale printer queue removed successfully.';";

                return RunPowerShell(script, out message);
            }

            message = "This ghost printer does not have a removable device ID or printer name.";
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

            message = output.Trim();

            if (exitCode == 0)
            {
                if (string.IsNullOrWhiteSpace(message))
                    message = "Operation completed successfully.";
                return true;
            }

            if (string.IsNullOrWhiteSpace(message))
                message = "Windows could not complete the operation.";

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
                output = "Operation timed out.";
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
