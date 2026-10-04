using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
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

            string printerName =
                printer.PrinterName?.Trim() ?? "";

            // -----------------------------------------------------
            // 1. Prefer the exact Device Instance ID captured from
            //    the selected Devices & Printers Shell item.
            // -----------------------------------------------------
            string exactPnpId =
                FirstNonEmpty(
                    printer.DeviceInstanceId,
                    printer.DeviceId);

            if (IsLikelyPnpDeviceId(exactPnpId))
            {
                if (RemovePnpDevice(
                        exactPnpId,
                        out message))
                {
                    return true;
                }

                // We found an exact PnP identity but Windows rejected it.
                // Do not silently delete another printer.
                return false;
            }

            // -----------------------------------------------------
            // 2. Try the exact Shell item and its Remove/Uninstall
            //    context verbs. This operates on the selected row,
            //    so duplicate display names remain safe.
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(printer.ShellPath))
            {
                if (TryRemoveShellPrinter(
                        printer.ShellPath,
                        out message))
                {
                    return true;
                }
            }

            // -----------------------------------------------------
            // 3. If exactly one queue matches, remove the queue.
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                List<string> matchingQueues =
                    FindPrinterQueueNames(printerName);

                if (matchingQueues.Count == 1)
                {
                    return RemovePrinterQueue(
                        matchingQueues[0],
                        out message);
                }

                if (matchingQueues.Count > 1)
                {
                    message =
                        "พบ Printer Queue ที่ตรงกับรายการนี้มากกว่า 1 รายการ\n\n" +
                        "เพื่อป้องกันการลบผิด Printer โปรแกรมจึงหยุดการทำงาน\n\n" +
                        string.Join(
                            Environment.NewLine,
                            matchingQueues);

                    return false;
                }
            }

            // -----------------------------------------------------
            // 4. Last fallback: find matching printer-related PnP
            //    instances even when Windows reports the device under
            //    SoftwareDevice / WSD / PrintQueue instead of Printer.
            // -----------------------------------------------------
            List<string> pnpIds =
                FindPrinterPnpDeviceIdsExpanded(
                    printerName);

            if (pnpIds.Count == 1)
            {
                return RemovePnpDevice(
                    pnpIds[0],
                    out message);
            }

            if (pnpIds.Count > 1)
            {
                message =
                    "พบ Printer Device ที่ชื่อเหมือนกันหลายรายการ\n\n" +
                    "เพื่อป้องกันการลบผิด Device โปรแกรมจึงไม่ลบอัตโนมัติ";

                return false;
            }

            message =
                "ไม่พบ Printer Queue หรือ Printer Device ที่สามารถลบได้\n\n" +
                "รายการจาก Windows Devices & Printers ไม่มี Device Instance ID " +
                "หรือ Printer Queue ที่สามารถผูกกับรายการที่เลือกได้";

            return false;
        }

        private static bool RemovePnpDevice(
            string deviceId,
            out string message)
        {
            message = "";

            if (!IsLikelyPnpDeviceId(deviceId))
            {
                message =
                    "Device Instance ID ไม่ใช่ PnP Device ID ที่ Windows รองรับ";
                return false;
            }

            string output;

            int exitCode =
                RunProcess(
                    "pnputil.exe",
                    $"/remove-device \"{SanitizeCommandValue(deviceId)}\"",
                    out output,
                    60_000);

            if (exitCode == 0)
            {
                message =
                    "ลบ Printer Device ที่เลือกออกจาก Windows เรียบร้อยแล้ว\n" +
                    "Driver Store ไม่ถูกลบ";

                return true;
            }

            message =
                string.IsNullOrWhiteSpace(output)
                    ? "Windows ไม่สามารถลบ Printer Device ที่เลือกได้"
                    : "Windows ไม่สามารถลบ Printer Device ที่เลือกได้\n\n" +
                      "รายละเอียดจาก Windows:\n" +
                      output.Trim();

            return false;
        }

        private static bool TryRemoveShellPrinter(
            string shellPath,
            out string message)
        {
            message = "";

            object? shellObject = null;
            dynamic? shell = null;
            dynamic? folder = null;
            dynamic? items = null;

            try
            {
                Type? shellType =
                    Type.GetTypeFromProgID("Shell.Application");

                if (shellType == null)
                {
                    message = "Shell.Application ไม่พร้อมใช้งาน";
                    return false;
                }

                shellObject =
                    Activator.CreateInstance(shellType);

                if (shellObject == null)
                {
                    message = "ไม่สามารถเปิด Windows Shell ได้";
                    return false;
                }

                shell = shellObject;

                const string namespacePath =
                    "::{26EE0668-A00A-44D7-9371-BEB064C98683}\\2\\::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";

                folder = shell.NameSpace(namespacePath);

                if (folder == null)
                    return false;

                items = folder.Items();

                int count =
                    Convert.ToInt32(items.Count);

                for (int index = 0; index < count; index++)
                {
                    dynamic? item = null;
                    dynamic? verbs = null;

                    try
                    {
                        item = items.Item(index);

                        if (item == null)
                            continue;

                        string path =
                            Convert.ToString(item.Path)?.Trim() ?? "";

                        if (!path.Equals(
                                shellPath.Trim(),
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Canonical verbs first.
                        string[] canonicalVerbs =
                        {
                            "remove",
                            "uninstall",
                            "delete",
                            "remove device",
                            "uninstall device"
                        };

                        foreach (string verbName in canonicalVerbs)
                        {
                            try
                            {
                                item.InvokeVerb(verbName);

                                message =
                                    "ลบ Printer / Device ที่เลือกจาก Windows Devices & Printers เรียบร้อยแล้ว\n" +
                                    "Driver Store ไม่ถูกลบ";

                                return true;
                            }
                            catch
                            {
                                // Try the next verb.
                            }
                        }

                        // Localized display verbs.
                        try
                        {
                            verbs = item.Verbs();
                            int verbCount =
                                Convert.ToInt32(verbs.Count);

                            for (int v = 0; v < verbCount; v++)
                            {
                                dynamic? verb = null;

                                try
                                {
                                    verb = verbs.Item(v);

                                    string verbText =
                                        Convert.ToString(verb.Name)?.Trim() ?? "";

                                    if (!LooksLikeRemoveVerb(verbText))
                                        continue;

                                    verb.DoIt();

                                    message =
                                        "ลบ Printer / Device ที่เลือกจาก Windows Devices & Printers เรียบร้อยแล้ว\n" +
                                        "Driver Store ไม่ถูกลบ";

                                    return true;
                                }
                                catch
                                {
                                }
                                finally
                                {
                                    ReleaseComObject(verb);
                                }
                            }
                        }
                        catch
                        {
                        }

                        return false;
                    }
                    finally
                    {
                        ReleaseComObject(verbs);
                        ReleaseComObject(item);
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                ReleaseComObject(items);
                ReleaseComObject(folder);
                ReleaseComObject(shell);
            }

            return false;
        }

        private static bool LooksLikeRemoveVerb(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string value =
                text.ToLowerInvariant();

            string[] keywords =
            {
                "remove",
                "uninstall",
                "delete",
                "เอาออก",
                "ลบ",
                "ถอนการติดตั้ง"
            };

            foreach (string keyword in keywords)
            {
                if (value.Contains(keyword,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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

        private static List<string> FindPrinterPnpDeviceIdsExpanded(
            string printerName)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(printerName))
                return result;

            string selected =
                NormalizePrinterName(printerName);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT PNPDeviceID, Name, Caption, ClassGuid, PNPClass, " +
                    "Manufacturer, Status FROM Win32_PnPEntity");

                using ManagementObjectCollection collection =
                    searcher.Get();

                foreach (ManagementObject device in collection)
                {
                    string pnpId =
                        Convert.ToString(device["PNPDeviceID"])?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(pnpId))
                        continue;

                    string name =
                        Convert.ToString(device["Name"])?.Trim() ?? "";

                    string caption =
                        Convert.ToString(device["Caption"])?.Trim() ?? "";

                    string pnpClass =
                        Convert.ToString(device["PNPClass"])?.Trim() ?? "";

                    string classGuid =
                        Convert.ToString(device["ClassGuid"])?.Trim() ?? "";

                    string manufacturer =
                        Convert.ToString(device["Manufacturer"])?.Trim() ?? "";

                    string normalizedName =
                        NormalizePrinterName(name);

                    string normalizedCaption =
                        NormalizePrinterName(caption);

                    bool exact =
                        normalizedName.Equals(
                            selected,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        normalizedCaption.Equals(
                            selected,
                            StringComparison.OrdinalIgnoreCase);

                    bool contains =
                        !exact &&
                        (normalizedName.Contains(selected,
                            StringComparison.OrdinalIgnoreCase)
                         ||
                         normalizedCaption.Contains(selected,
                            StringComparison.OrdinalIgnoreCase));

                    if (!exact && !contains)
                        continue;

                    bool printerRelated =
                        pnpClass.Equals("Printer",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        pnpClass.Equals("PrintQueue",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        pnpClass.Equals("SoftwareDevice",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        classGuid.Equals(
                            "{4D36E979-E325-11CE-BFC1-08002BE10318}",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        IsPrinterLikeInstanceId(pnpId)
                        ||
                        ContainsPrinterTerm(
                            name + " " + manufacturer);

                    if (!printerRelated)
                        continue;

                    AddUnique(
                        result,
                        pnpId);
                }
            }
            catch
            {
            }

            return result;
        }

        private static bool IsPrinterLikeInstanceId(
            string deviceId)
        {
            string value =
                deviceId ?? "";

            return
                value.StartsWith("USBPRINT\\",
                    StringComparison.OrdinalIgnoreCase)
                ||
                value.StartsWith("PRINTENUM\\",
                    StringComparison.OrdinalIgnoreCase)
                ||
                value.StartsWith("SWD\\PRINTENUM\\",
                    StringComparison.OrdinalIgnoreCase)
                ||
                value.StartsWith("SWD\\DAFUPnPProvider\\",
                    StringComparison.OrdinalIgnoreCase)
                ||
                value.StartsWith("WSDPRINT\\",
                    StringComparison.OrdinalIgnoreCase)
                ||
                value.StartsWith("DOT4PRINT\\",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsPrinterTerm(
            string text)
        {
            string value =
                text ?? "";

            string[] terms =
            {
                "printer",
                "print queue",
                "printqueue",
                "dcp-",
                "pixma",
                "deskjet",
                "laserjet",
                "imageclass",
                "imagerunner",
                "ecotank",
                "workforce",
                "smart tank"
            };

            foreach (string term in terms)
            {
                if (value.Contains(term,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
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

        private static string FirstNonEmpty(
            params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
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
