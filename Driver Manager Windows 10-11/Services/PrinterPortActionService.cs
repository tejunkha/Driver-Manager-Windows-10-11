using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Printer Port deletion only.
    /// Keep this class in PrinterPortActionService.cs.
    /// </summary>
    public static class PrinterPortActionService
    {
        [DllImport(
            "winspool.drv",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern bool DeletePort(
            string? pName,
            IntPtr hWnd,
            string pPortName);

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

            // Only block when an actual installed printer queue uses
            // this port. Windows/Port Monitor can otherwise temporarily
            // hold a stale handle.
            List<string> printersUsingPort =
                FindPrintersUsingPort(portName);

            if (printersUsingPort.Count > 0)
            {
                message =
                    $"ไม่สามารถลบ Port '{portName}' ได้\n\n" +
                    "Port นี้กำลังถูกใช้งานโดย Printer จริง:\n" +
                    string.Join(
                        Environment.NewLine,
                        printersUsingPort) +
                    "\n\n" +
                    "กรุณาเปลี่ยน Port ของ Printer ก่อน";

                return false;
            }

            if (TryRemovePrinterPortPowerShell(
                portName,
                out string firstError))
            {
                message =
                    $"ลบ Printer Port '{portName}' เรียบร้อยแล้ว";

                return true;
            }

            if (IsPortInUseError(firstError))
            {
                if (RestartPrintSpooler(
                    out string spoolerError))
                {
                    System.Threading.Thread.Sleep(1500);

                    if (TryRemovePrinterPortPowerShell(
                        portName,
                        out string secondError))
                    {
                        message =
                            $"ลบ Printer Port '{portName}' เรียบร้อยแล้ว\n" +
                            "Print Spooler ถูกรีสตาร์ตเพื่อปลดการจับ Port";

                        return true;
                    }

                    if (TryDeletePortNative(
                        portName,
                        out string nativeError))
                    {
                        message =
                            $"ลบ Printer Port '{portName}' เรียบร้อยแล้ว";

                        return true;
                    }

                    message =
                        BuildFinalError(
                            portName,
                            secondError,
                            nativeError,
                            spoolerError);

                    return false;
                }

                if (TryDeletePortNative(
                    portName,
                    out string nativeErrorWithoutRestart))
                {
                    message =
                        $"ลบ Printer Port '{portName}' เรียบร้อยแล้ว";

                    return true;
                }

                message =
                    BuildFinalError(
                        portName,
                        firstError,
                        nativeErrorWithoutRestart,
                        spoolerError);

                return false;
            }

            if (TryDeletePortNative(
                portName,
                out string directError))
            {
                message =
                    $"ลบ Printer Port '{portName}' เรียบร้อยแล้ว";

                return true;
            }

            message =
                BuildFinalError(
                    portName,
                    firstError,
                    directError,
                    "");

            return false;
        }

        private static List<string> FindPrintersUsingPort(
            string portName)
        {
            var result = new List<string>();

            try
            {
                using var searcher =
                    new ManagementObjectSearcher(
                        "SELECT Name, PortName FROM Win32_Printer");

                using ManagementObjectCollection collection =
                    searcher.Get();

                foreach (ManagementObject printer in collection)
                {
                    try
                    {
                        string currentPort =
                            Convert.ToString(
                                printer["PortName"])?.Trim() ?? "";

                        if (!currentPort.Equals(
                            portName,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string printerName =
                            Convert.ToString(
                                printer["Name"])?.Trim() ?? "";

                        if (!string.IsNullOrWhiteSpace(printerName) &&
                            !result.Contains(
                                printerName,
                                StringComparer.OrdinalIgnoreCase))
                        {
                            result.Add(printerName);
                        }
                    }
                    finally
                    {
                        printer.Dispose();
                    }
                }
            }
            catch
            {
                // Do not block deletion if WMI cannot enumerate.
            }

            return result;
        }

        private static bool TryRemovePrinterPortPowerShell(
            string portName,
            out string errorMessage)
        {
            errorMessage = "";

            string script =
                "Import-Module PrintManagement -ErrorAction Stop; " +
                "$name = '" +
                ToPowerShellSingleQuoted(portName) +
                "'; " +
                "$port = Get-PrinterPort " +
                "-Name $name " +
                "-ErrorAction SilentlyContinue; " +
                "if ($null -eq $port) { " +
                "throw ('NOT_FOUND:' + $name); " +
                "}; " +
                "Remove-PrinterPort " +
                "-Name $name " +
                "-Confirm:$false " +
                "-ErrorAction Stop; " +
                "Write-Output 'PORT_REMOVED';";

            return RunPowerShell(
                script,
                out errorMessage);
        }

        private static bool RestartPrintSpooler(
            out string errorMessage)
        {
            errorMessage = "";

            bool stopped =
                RunServiceCommand(
                    "stop",
                    "spooler",
                    out string stopError);

            if (!stopped &&
                !ContainsIgnoreCase(
                    stopError,
                    "not started",
                    "service has not been started"))
            {
                errorMessage =
                    "ไม่สามารถหยุด Print Spooler ได้\n" +
                    stopError;

                return false;
            }

            System.Threading.Thread.Sleep(1000);

            bool started =
                RunServiceCommand(
                    "start",
                    "spooler",
                    out string startError);

            if (!started)
            {
                errorMessage =
                    "ไม่สามารถเริ่ม Print Spooler กลับมาได้\n" +
                    startError;

                return false;
            }

            return true;
        }

        private static bool RunServiceCommand(
            string action,
            string serviceName,
            out string errorMessage)
        {
            errorMessage = "";

            try
            {
                using var process =
                    new Process();

                process.StartInfo =
                    new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments =
                            $"{action} {serviceName}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                process.Start();

                string output =
                    process.StandardOutput.ReadToEnd();

                string error =
                    process.StandardError.ReadToEnd();

                if (!process.WaitForExit(30000))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                    }

                    errorMessage =
                        "Service command timeout";

                    return false;
                }

                string combined =
                    (output + Environment.NewLine + error).Trim();

                if (process.ExitCode == 0)
                    return true;

                errorMessage =
                    string.IsNullOrWhiteSpace(combined)
                        ? $"sc.exe {action} {serviceName} failed"
                        : combined;

                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static bool TryDeletePortNative(
            string portName,
            out string errorMessage)
        {
            errorMessage = "";

            try
            {
                bool result =
                    DeletePort(
                        null,
                        IntPtr.Zero,
                        portName);

                if (result)
                    return true;

                int win32Error =
                    Marshal.GetLastWin32Error();

                errorMessage =
                    $"WinSpool DeletePort failed. Win32Error={win32Error}";

                return false;
            }
            catch (Exception ex)
            {
                errorMessage =
                    "Native DeletePort error: " +
                    ex.Message;

                return false;
            }
        }

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
                        "PowerShell operation timed out";

                    return false;
                }

                string output =
                    string.IsNullOrWhiteSpace(standardError)
                        ? standardOutput
                        : standardOutput +
                          Environment.NewLine +
                          standardError;

                if (process.ExitCode == 0)
                    return true;

                message =
                    string.IsNullOrWhiteSpace(output)
                        ? "Windows could not remove the printer port."
                        : output.Trim();

                return false;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        private static bool IsPortInUseError(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return
                ContainsIgnoreCase(
                    text,
                    "port is in use",
                    "resource is in use",
                    "0x800700aa",
                    "hresult: 0x800700aa");
        }

        private static bool ContainsIgnoreCase(
            string text,
            params string[] values)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (string value in values)
            {
                if (text.IndexOf(
                    value,
                    StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildFinalError(
            string portName,
            string windowsError,
            string nativeError,
            string spoolerError)
        {
            var sb =
                new StringBuilder();

            sb.AppendLine(
                $"Windows ไม่สามารถลบ Printer Port '{portName}' ได้");

            sb.AppendLine();

            sb.AppendLine(
                "โปรแกรมตรวจแล้วว่าไม่มี Printer Queue " +
                "ที่กำลังใช้ Port นี้");

            sb.AppendLine(
                "จึงได้ลองปลดการจับจาก Print Spooler " +
                "และลองลบ Port โดยตรงแล้ว");

            if (!string.IsNullOrWhiteSpace(nativeError))
            {
                sb.AppendLine();
                sb.AppendLine("รายละเอียด:");
                sb.AppendLine(nativeError);
            }
            else if (!string.IsNullOrWhiteSpace(windowsError))
            {
                sb.AppendLine();
                sb.AppendLine("รายละเอียด:");
                sb.AppendLine(windowsError.Trim());
            }

            if (!string.IsNullOrWhiteSpace(spoolerError))
            {
                sb.AppendLine();
                sb.AppendLine("Print Spooler:");
                sb.AppendLine(spoolerError.Trim());
            }

            return sb.ToString().Trim();
        }

        private static string ToPowerShellSingleQuoted(
            string value)
        {
            return (value ?? "").Replace("'", "''");
        }
    }
}
