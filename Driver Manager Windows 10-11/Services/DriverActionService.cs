using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
        // Windows Devices & Printers -> Remove device
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

            // Use the exact ShellPath of the selected row first.
            // This follows the same Windows Devices & Printers item.
            if (!string.IsNullOrWhiteSpace(printer.ShellPath))
            {
                bool shellSuccess =
                    TryRemoveDeviceFromWindowsShell(
                        printer.ShellPath,
                        out string shellMessage);

                if (shellSuccess)
                {
                    message = shellMessage;
                    return true;
                }

                message = shellMessage;
            }

            // Fallback: use the exact DeviceInstanceId.
            string deviceInstanceId =
                printer.DeviceInstanceId?.Trim() ?? "";

            if (IsLikelyPnpDeviceId(deviceInstanceId))
            {
                string output;

                int exitCode = RunProcess(
                    "pnputil.exe",
                    $"/remove-device \"{SanitizeCommandValue(deviceInstanceId)}\"",
                    out output,
                    60_000);

                if (exitCode == 0)
                {
                    message =
                        "ลบ Printer Device ที่เลือกออกจาก Windows เรียบร้อยแล้ว\n" +
                        "Driver Store ไม่ถูกลบ";

                    return true;
                }

                message = string.IsNullOrWhiteSpace(output)
                    ? "Windows ไม่สามารถ Remove device ได้"
                    : "Windows ไม่สามารถ Remove device ได้\n\n" +
                      "รายละเอียดจาก Windows:\n" +
                      output.Trim();

                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message =
                    "ไม่สามารถระบุ Device Instance ของรายการที่เลือกได้";
            }

            message +=
                "\n\nโปรแกรมจึงไม่ลบ Printer Queue แทน " +
                "เพื่อป้องกันการลบผิดรายการ";

            return false;
        }

        // =========================================================
        // WINDOWS SHELL -> REMOVE DEVICE
        // =========================================================
        private static bool TryRemoveDeviceFromWindowsShell(
            string shellPath,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(shellPath))
            {
                message = "ShellPath ว่าง";
                return false;
            }

            const string devicesAndPrintersNamespace =
                "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\2\\::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";

            object? shellObject = null;
            dynamic? shell = null;
            dynamic? folder = null;
            dynamic? items = null;
            dynamic? selectedItem = null;

            try
            {
                Type? shellType =
                    Type.GetTypeFromProgID("Shell.Application");

                if (shellType == null)
                {
                    message = "ไม่พบ Shell.Application";
                    return false;
                }

                shellObject =
                    Activator.CreateInstance(shellType);

                if (shellObject == null)
                {
                    message =
                        "ไม่สามารถสร้าง Shell.Application ได้";
                    return false;
                }

                shell = shellObject;

                folder =
                    shell.NameSpace(
                        devicesAndPrintersNamespace);

                if (folder == null)
                {
                    message =
                        "ไม่สามารถเปิด Windows Devices & Printers ได้";
                    return false;
                }

                items = folder.Items();

                int count =
                    Convert.ToInt32(items.Count);

                // Find the exact Shell item using ShellPath.
                for (int i = 0; i < count; i++)
                {
                    dynamic? item = null;

                    try
                    {
                        item = items.Item(i);

                        if (item == null)
                            continue;

                        string itemPath =
                            SafeDynamicString(item.Path);

                        if (itemPath.Equals(
                                shellPath.Trim(),
                                StringComparison.OrdinalIgnoreCase))
                        {
                            selectedItem = item;
                            item = null;
                            break;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        ReleaseComObject(item);
                    }
                }

                if (selectedItem == null)
                {
                    message =
                        "ไม่พบ Printer ที่เลือกใน Windows Devices & Printers\n\n" +
                        "ShellPath:\n" +
                        shellPath;

                    return false;
                }

                // Look for the actual Remove device verb.
                dynamic? verbs = null;

                try
                {
                    verbs = selectedItem.Verbs();

                    int verbCount =
                        Convert.ToInt32(verbs.Count);

                    for (int i = 0; i < verbCount; i++)
                    {
                        dynamic? verb = null;

                        try
                        {
                            verb = verbs.Item(i);

                            if (verb == null)
                                continue;

                            string verbName =
                                SafeDynamicString(verb.Name);

                            if (!IsRemoveDeviceVerb(verbName))
                                continue;

                            verb.DoIt();

                            Thread.Sleep(1500);

                            message =
                                "ลบ Printer Device ออกจาก Windows Devices & Printers เรียบร้อยแล้ว\n" +
                                "Driver Store ไม่ถูกลบ";

                            return true;
                        }
                        catch
                        {
                            // Try the next Shell verb.
                        }
                        finally
                        {
                            ReleaseComObject(verb);
                        }
                    }
                }
                finally
                {
                    ReleaseComObject(verbs);
                }

                // Fallback to the canonical Shell verb.
                try
                {
                    selectedItem.InvokeVerb("remove");

                    Thread.Sleep(1500);

                    message =
                        "ลบ Printer Device ออกจาก Windows Devices & Printers เรียบร้อยแล้ว\n" +
                        "Driver Store ไม่ถูกลบ";

                    return true;
                }
                catch
                {
                }

                message =
                    "พบ Printer ที่เลือกแล้ว แต่ Windows ไม่เปิดคำสั่ง Remove device สำหรับรายการนี้";

                return false;
            }
            catch (Exception ex)
            {
                message =
                    "ไม่สามารถเรียก Windows Remove device ได้\n\n" +
                    ex.Message;

                return false;
            }
            finally
            {
                ReleaseComObject(selectedItem);
                ReleaseComObject(items);
                ReleaseComObject(folder);
                ReleaseComObject(shell);
            }
        }

        // =========================================================
        // CHECK REMOVE DEVICE VERB
        // =========================================================
        private static bool IsRemoveDeviceVerb(
            string verbName)
        {
            if (string.IsNullOrWhiteSpace(verbName))
                return false;

            string text =
                verbName
                    .Replace("&", "")
                    .Replace("...", "")
                    .Trim()
                    .ToLowerInvariant();

            if (text.Contains("remove device"))
                return true;

            if (text.Equals("remove"))
                return true;

            if (text.Contains("remove"))
                return true;

            if (text.Contains("ลบอุปกรณ์"))
                return true;

            if (text.Contains("ลบ"))
                return true;

            if (text.Contains("นำออก"))
                return true;

            return false;
        }

        // =========================================================
        // SAFE DYNAMIC STRING
        // =========================================================
        private static string SafeDynamicString(
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

        // =========================================================
        // FIND ACTUAL PRINTER QUEUE NAMES BY WMI
        // =========================================================
        private static List<string> FindPrinterQueueNames(
            string selectedName)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(selectedName))
                return result;

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, PNPDeviceID FROM Win32_Printer");

                using ManagementObjectCollection collection =
                    searcher.Get();

                var exact = new List<string>();
                var compatible = new List<string>();

                string selectedNormalized =
                    NormalizePrinterName(selectedName);

                foreach (ManagementObject printer in collection)
                {
                    string queueName =
                        Convert.ToString(printer["Name"])?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(queueName))
                        continue;

                    if (queueName.Equals(
                        selectedName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AddUnique(exact, queueName);
                        continue;
                    }

                    string queueNormalized =
                        NormalizePrinterName(queueName);

                    if (!string.IsNullOrWhiteSpace(selectedNormalized) &&
                        !string.IsNullOrWhiteSpace(queueNormalized))
                    {
                        // Safe-ish fuzzy matching only when one normalized
                        // name contains the other. Do NOT use this path when
                        // multiple queues match.
                        if (queueNormalized.Contains(selectedNormalized,
                                StringComparison.OrdinalIgnoreCase) ||
                            selectedNormalized.Contains(queueNormalized,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            AddUnique(compatible, queueName);
                        }
                    }
                }

                if (exact.Count > 0)
                    return exact;

                return compatible;
            }
            catch
            {
                return result;
            }
        }

        private static bool RemovePrinterQueue(
            string queueName,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(queueName))
            {
                message = "ไม่พบชื่อ Printer Queue";
                return false;
            }

            string name =
                ToPowerShellSingleQuoted(queueName);

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

            bool success =
                RunPowerShell(
                    script,
                    out message);

            if (success)
            {
                message =
                    "ลบ Printer Queue ออกจาก Windows เรียบร้อยแล้ว\n" +
                    "Driver Store ไม่ถูกลบ";
            }

            return success;
        }

        private static string NormalizePrinterName(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            string result =
                value.Trim().ToLowerInvariant();

            // Remove common Windows copy suffixes for matching only.
            result = Regex.Replace(
                result,
                @"\s*\(copy\s*\d*\)\s*$",
                "",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            result = Regex.Replace(
                result,
                @"\s+",
                " ",
                RegexOptions.CultureInvariant);

            return result.Trim();
        }

        private static void AddUnique(
            List<string> list,
            string value)
        {
            if (!list.Contains(
                    value,
                    StringComparer.OrdinalIgnoreCase))
            {
                list.Add(value);
            }
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
        // RESOLVE DEVICES & PRINTERS SHELL PATH -> PNP ID
        // =========================================================
        private static string TryResolvePnpDeviceIdFromShellPath(
            string shellPath)
        {
            if (string.IsNullOrWhiteSpace(shellPath))
                return "";

            string targetPath =
                shellPath.Trim();

            // A normal PnP ID does not need Shell resolution.
            if (IsLikelyPnpDeviceId(targetPath))
                return targetPath;

            if (!targetPath.StartsWith(
                    "::",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            const string devicesAndPrintersNamespace =
                "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\2\\::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";

            object? shellObject = null;
            dynamic? shell = null;
            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                Type? shellType =
                    Type.GetTypeFromProgID(
                        "Shell.Application");

                if (shellType == null)
                    return "";

                shellObject =
                    Activator.CreateInstance(
                        shellType);

                if (shellObject == null)
                    return "";

                shell = shellObject;

                folder =
                    shell.NameSpace(
                        devicesAndPrintersNamespace);

                if (folder == null)
                    return "";

                items =
                    folder.Items();

                int count =
                    Convert.ToInt32(
                        items.Count);

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
                            continue;

                        string itemPath =
                            Convert.ToString(
                                item.Path)?.Trim() ?? "";

                        if (!itemPath.Equals(
                                targetPath,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string deviceInstanceId =
                            Convert.ToString(
                                item.ExtendedProperty(
                                    "System.Devices.DeviceInstanceId"))
                            ?.Trim() ?? "";

                        if (IsLikelyPnpDeviceId(
                                deviceInstanceId))
                        {
                            return deviceInstanceId;
                        }

                        // Some Shell items expose the WMI/Device path
                        // via HardwareId instead. Try that as a fallback.
                        string hardwareId =
                            Convert.ToString(
                                item.ExtendedProperty(
                                    "System.Devices.HardwareId"))
                            ?.Trim() ?? "";

                        if (IsLikelyPnpDeviceId(
                                hardwareId))
                        {
                            return hardwareId;
                        }

                        return "";
                    }
                    catch
                    {
                        // Continue scanning remaining Shell items.
                    }
                    finally
                    {
                        ReleaseComObject(item);
                    }
                }
            }
            catch
            {
                return "";
            }
            finally
            {
                ReleaseComObject(items);
                ReleaseComObject(folder);
                ReleaseComObject(shell);
            }

            return "";
        }

        // =========================================================
        // RELEASE COM OBJECT
        // =========================================================
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
