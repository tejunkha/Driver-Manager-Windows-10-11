using System;
using System.Diagnostics;
using System.Text;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Delete actions for the "Printer Ports" page only.
    /// Kept separate from DriverActionService so changes to printer-port
    /// deletion cannot affect Printers (IPP / Vendor) or Devices and Printers.
    /// </summary>
    public static class PrinterPortActionService
    {
        // =========================================================
        // DELETE PRINTER PORT
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

            string portName =
                port.Name?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(portName))
            {
                message = "ไม่พบชื่อ Printer Port";
                return false;
            }

            string safeName =
                ToPowerShellSingleQuoted(portName);

            // ตรวจสอบก่อนว่า Port กำลังถูกใช้งานอยู่หรือไม่
            string script =
                "Import-Module PrintManagement -ErrorAction Stop; " +
                "$name = '" + safeName + "'; " +

                "$printers = @(" +
                "Get-Printer -ErrorAction SilentlyContinue | " +
                "Where-Object { " +
                "$_.PortName -eq $name " +
                "} " +
                "); " +

                "if ($printers.Count -gt 0) { " +
                "$list = ($printers | " +
                "Select-Object -ExpandProperty Name) -join [Environment]::NewLine; " +
                "throw (" +
                "'Printer Port ถูกใช้งานโดย Printer ต่อไปนี้:' + " +
                "[Environment]::NewLine + " +
                "[Environment]::NewLine + " +
                "$list" +
                "); " +
                "}; " +

                "Remove-PrinterPort " +
                "-Name $name " +
                "-Confirm:$false " +
                "-ErrorAction Stop; " +

                "Write-Output ('REMOVED_PORT=' + $name);";

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
                        "การลบ Printer Port ใช้เวลานานเกินกำหนด";

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
                        "ลบ Printer Port เรียบร้อยแล้ว";

                    return true;
                }

                message =
                    string.IsNullOrWhiteSpace(output)
                        ? "Windows ไม่สามารถลบ Printer Port ได้"
                        : "Windows ไม่สามารถลบ Printer Port ได้\n\n" +
                          "รายละเอียดจาก Windows:\n" +
                          output.Trim();

                return false;
            }
            catch (Exception ex)
            {
                message =
                    "เกิดข้อผิดพลาดระหว่างการลบ Printer Port\n\n" +
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
