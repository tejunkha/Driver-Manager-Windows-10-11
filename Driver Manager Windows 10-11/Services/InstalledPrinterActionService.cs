using System;
using System.Diagnostics;
using System.Text;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Delete actions for the "Printers (IPP / Vendor)" page only.
    /// This service is intentionally independent from DriverActionService
    /// so changes to installed-printer deletion cannot affect
    /// Devices and Printers.
    /// </summary>
    public static class InstalledPrinterActionService
    {
        // =========================================================
        // DELETE INSTALLED PRINTER
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

            string printerName =
                printer.Name?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(printerName))
            {
                message = "ไม่พบชื่อ Printer";
                return false;
            }

            string safeName =
                ToPowerShellSingleQuoted(printerName);

            string script =
                "Import-Module PrintManagement -ErrorAction Stop; " +
                "$name = '" + safeName + "'; " +
                "$p = Get-Printer -Name $name -ErrorAction SilentlyContinue; " +
                "if ($null -eq $p) { " +
                "throw ('ไม่พบ Printer: ' + $name); " +
                "}; " +
                "Remove-Printer -Name $name " +
                "-Confirm:$false " +
                "-ErrorAction Stop; " +
                "Write-Output ('REMOVED_PRINTER=' + $name);";

            return RunPowerShell(
                script,
                out message);
        }

        // =========================================================
        // POWERSHELL
        // =========================================================
        private static bool RunPowerShell(
            string script,
            out string message)
        {
            message = "";

            try
            {
                string encoded =
                    Convert.ToBase64String(
                        Encoding.Unicode.GetBytes(script));

                using var process =
                    new Process();

                process.StartInfo =
                    new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments =
                            "-NoLogo -NoProfile -NonInteractive " +
                            "-ExecutionPolicy Bypass " +
                            "-EncodedCommand " +
                            encoded,

                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                process.Start();

                string standardOutput =
                    process.StandardOutput.ReadToEnd();

                string standardError =
                    process.StandardError.ReadToEnd();

                if (!process.WaitForExit(60000))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                    }

                    message =
                        "การลบ Printer ใช้เวลานานเกินกำหนด";

                    return false;
                }

                string output =
                    string.IsNullOrWhiteSpace(standardError)
                        ? standardOutput
                        : standardOutput +
                          Environment.NewLine +
                          standardError;

                if (process.ExitCode == 0)
                {
                    message =
                        "ลบ Printer ออกจากระบบเรียบร้อยแล้ว\n" +
                        "Printer Driver ไม่ถูกลบ";

                    return true;
                }

                message =
                    string.IsNullOrWhiteSpace(output)
                        ? "Windows ไม่สามารถลบ Printer ได้"
                        : "Windows ไม่สามารถลบ Printer ได้\n\n" +
                          "รายละเอียดจาก Windows:\n" +
                          output.Trim();

                return false;
            }
            catch (Exception ex)
            {
                message =
                    "เกิดข้อผิดพลาดระหว่างการลบ Printer\n\n" +
                    ex.Message;

                return false;
            }
        }

        // =========================================================
        // POWERSHELL SINGLE QUOTE
        // =========================================================
        private static string ToPowerShellSingleQuoted(
            string value)
        {
            return (value ?? "").Replace("'", "''");
        }
    }
}
