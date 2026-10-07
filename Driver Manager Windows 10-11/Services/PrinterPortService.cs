using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class PrinterPortService
    {
        // ============================================================
        // GET PRINTER PORTS
        // ============================================================

        public static List<PrinterPortModel> GetPorts()
        {
            string json = RunPowerShell();

            List<PrinterPortModel> result;

            if (string.IsNullOrWhiteSpace(json))
                result = new List<PrinterPortModel>();
            else
                result = ParseJson(json);

            // Merge USB Monitor\UsbPortList.
            // These entries may exist only in Registry and may no longer
            // be returned by Get-PrinterPort.
            MergeUsbRegistryPorts(result);

            result.Sort((a, b) =>
                string.Compare(
                    a.Name,
                    b.Name,
                    StringComparison.OrdinalIgnoreCase));

            return result;
        }

        // ============================================================
        // DELETE USB PRINTER PORT
        // ============================================================

        public static bool DeleteUsbPort(
            string portName,
            out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(portName))
            {
                message = "Printer Port name is empty.";
                return false;
            }

            portName = portName.Trim();

            // Force delete is intentionally restricted to USB ports.
            if (!IsUsbPort(portName))
            {
                message =
                    $"Port '{portName}' cannot be force deleted.\n\n" +
                    "Only USB printer ports can be force deleted.";

                return false;
            }

            if (!IsAdministrator())
            {
                message =
                    "Administrator privileges are required.\n\n" +
                    "Please run Driver Manager as Administrator.";

                return false;
            }

            // Never remove a port that is currently assigned to a real printer queue.
            List<string> printers = GetPrintersUsingPort(portName);

            if (printers.Count > 0)
            {
                message =
                    $"Port '{portName}' is currently in use.\n\n" +
                    "Printer(s) using this port:\n\n" +
                    string.Join("\n", printers) +
                    "\n\nThe port was NOT deleted.";

                return false;
            }

            // First try the normal Windows PrintManagement command.
            bool normalDelete = RemovePrinterPort(portName);

            if (normalDelete)
            {
                Thread.Sleep(500);

                if (!PortExists(portName))
                {
                    message =
                        $"USB Printer Port '{portName}' was deleted successfully.";

                    return true;
                }
            }

            // If the normal method fails (for example Win32 error 50),
            // force-clean only the exact USB port entry from Print Monitor registry.
            bool registryDelete = false;
            string registryMessage = "";
            bool spoolerStopped = false;

            try
            {
                if (!StopSpooler(out string stopError))
                {
                    message =
                        "Unable to stop Print Spooler.\n\n" +
                        stopError;

                    return false;
                }

                spoolerStopped = true;

                registryDelete = ForceRemoveUsbPortRegistry(
                    portName,
                    out registryMessage);


                bool usbMonitorDelete =
                    RemoveUsbMonitorUserPort(
                        portName,
                        out string usbMonitorMessage);

                if (!StartSpooler(out string startError))
                {
                    message =
                        "The USB Port registry cleanup completed, but Print Spooler could not be started again.\n\n" +
                        startError;

                    return false;
                }

                spoolerStopped = false;
                Thread.Sleep(1500);

                // Verify the actual Windows Printer Port list.
                if (!PortExists(portName))
                {
                    message =
                        $"ลบ USB Printer Port '{portName}' สำเร็จแล้ว\n\n" +
                        $"ลบแบบปกติ: {(normalDelete ? "สำเร็จ" : "ไม่พบใน Windows Printer Port")}\n" +
                        $"ลบ Registry ของ Printer Port: {(registryDelete ? "สำเร็จ" : "ไม่พบ")}\n" +
                        $"ลบ USB Monitor UsbPortList: {(usbMonitorDelete ? "สำเร็จ" : "ไม่พบ")}\n" +
                        "Print Spooler: เริ่มการทำงานใหม่แล้ว";

                    return true;
                }

                // One final normal removal after the monitor has restarted.
                bool finalDelete = RemovePrinterPort(portName);
                Thread.Sleep(500);

                if (finalDelete && !PortExists(portName))
                {
                    message =
                        $"USB Printer Port '{portName}' was removed successfully.";

                    return true;
                }

                message =
                    $"Windows still reports USB Printer Port '{portName}'.\n\n" +
                    $"Normal Remove: {(normalDelete || finalDelete ? "Tried" : "Failed")}\n" +
                    $"Registry Cleanup: {(registryDelete ? "Success" : "Not found")}\n\n" +
                    (string.IsNullOrWhiteSpace(registryMessage)
                        ? "The USB Port may be recreated by its Port Monitor or printer driver."
                        : registryMessage);

                return false;
            }
            finally
            {
                // Safety: never leave Print Spooler stopped.
                if (spoolerStopped)
                {
                    StartSpooler(out _);
                }
            }
        }

        // ============================================================
        // MERGE USB MONITOR REGISTRY PORTS
        // ============================================================

        private static void MergeUsbRegistryPorts(
            List<PrinterPortModel> result)
        {
            List<PrinterPortModel> registryPorts =
                GetUsbRegistryPorts();

            if (registryPorts.Count == 0)
                return;

            var existing =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (PrinterPortModel port in result)
            {
                if (!string.IsNullOrWhiteSpace(port.Name))
                    existing.Add(port.Name.Trim());
            }

            foreach (PrinterPortModel registryPort in registryPorts)
            {
                string name =
                    registryPort.Name?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (existing.Contains(name))
                    continue;

                result.Add(registryPort);
                existing.Add(name);
            }
        }


        // ============================================================
        // READ USB MONITOR\USBPORTLIST
        // ============================================================

        private static List<PrinterPortModel> GetUsbRegistryPorts()
        {
            var result =
                new List<PrinterPortModel>();

            const string script = @"
$ErrorActionPreference = 'SilentlyContinue'

$path =
    'HKLM:\SYSTEM\CurrentControlSet\Control\Print\Monitors\USB Monitor\UsbPortList'

if (-not (Test-Path $path))
{
    Write-Output '[]'
    exit 0
}

$key =
    Get-Item -Path $path -ErrorAction SilentlyContinue

if ($null -eq $key)
{
    Write-Output '[]'
    exit 0
}

$items = @(
    foreach ($name in $key.GetValueNames())
    {
        if ($name -match '^USB\d+$')
        {
            [PSCustomObject]@{
                Name = $name
            }
        }
    }
)

if ($items.Count -eq 0)
{
    Write-Output '[]'
}
else
{
    $items | ConvertTo-Json -Compress
}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out _);

            if (string.IsNullOrWhiteSpace(output))
                return result;

            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(output);

                JsonElement root =
                    document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        string name =
                            GetString(item, "Name");

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            result.Add(
                                CreateRegistryOnlyPort(name));
                        }
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    string name =
                        GetString(root, "Name");

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        result.Add(
                            CreateRegistryOnlyPort(name));
                    }
                }
            }
            catch (JsonException)
            {
                // Keep normal Windows Printer Port results if the
                // Registry data cannot be parsed.
            }

            return result;
        }


        // ============================================================
        // CREATE REGISTRY-ONLY PORT MODEL
        // ============================================================

        private static PrinterPortModel CreateRegistryOnlyPort(
            string name)
        {
            return new PrinterPortModel
            {
                Name = name,
                Description = "USB Port (Registry Only)",
                PrinterHostAddress = "",
                PortNumber = "",
                Protocol = "",
                SNMPEnabled = "",
                ComputerName = "Registry Only",
                PortMonitor = "USB Monitor"
            };
        }


        // ============================================================
        // REMOVE USB MONITOR\USBPORTLIST VALUE
        // ============================================================

        private static bool RemoveUsbMonitorUserPort(
            string portName,
            out string message)
        {
            message = "";

            string script = $@"
$ErrorActionPreference = 'Stop'

$portName = '{EscapePowerShell(portName)}'

$path =
    'HKLM:\SYSTEM\CurrentControlSet\Control\Print\Monitors\USB Monitor\UsbPortList'

if (-not (Test-Path $path))
{{
    Write-Output 'NOT_FOUND'
    exit 0
}}

$key =
    Get-Item -Path $path -ErrorAction Stop

$found = $false

foreach ($valueName in $key.GetValueNames())
{{
    if ($valueName -ieq $portName)
    {{
        Remove-ItemProperty `
            -Path $path `
            -Name $valueName `
            -Force `
            -ErrorAction Stop

        $found = $true
    }}
}}

if ($found)
{{
    Write-Output 'REMOVED'
}}
else
{{
    Write-Output 'NOT_FOUND'
}}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out string error);

            if (output.Contains(
                "REMOVED",
                StringComparison.OrdinalIgnoreCase))
            {
                message =
                    $"USB Monitor UsbPortList entry '{portName}' was removed.";

                return true;
            }

            if (output.Contains(
                "NOT_FOUND",
                StringComparison.OrdinalIgnoreCase))
            {
                message =
                    $"USB Monitor UsbPortList entry '{portName}' was not found.";

                return false;
            }

            message =
                string.IsNullOrWhiteSpace(error)
                    ? $"Unable to remove USB Monitor UsbPortList entry '{portName}'."
                    : error.Trim();

            return false;
        }


        // ============================================================
        // CHECK USB PORT
        // ============================================================

        private static bool IsUsbPort(string portName)
        {
            return portName.StartsWith(
                "USB",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // CHECK ADMINISTRATOR
        // ============================================================

        private static bool IsAdministrator()
        {
            try
            {
                using WindowsIdentity identity =
                    WindowsIdentity.GetCurrent();

                WindowsPrincipal principal =
                    new WindowsPrincipal(identity);

                return principal.IsInRole(
                    WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }


        // ============================================================
        // FIND PRINTERS USING PORT
        // ============================================================

        private static List<string> GetPrintersUsingPort(
            string portName)
        {
            var result =
                new List<string>();

            string script = $@"
$ErrorActionPreference = 'SilentlyContinue'

$portName = '{EscapePowerShell(portName)}'

$printers = Get-Printer |
    Where-Object {{
        $_.PortName -eq $portName
    }}

@($printers) |
    ForEach-Object {{
        $_.Name
    }}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out _);

            if (string.IsNullOrWhiteSpace(output))
                return result;


            foreach (string line in output.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                string printer =
                    line.Trim();

                if (!string.IsNullOrWhiteSpace(printer))
                    result.Add(printer);
            }

            return result;
        }


        // ============================================================
        // CHECK PORT EXISTS
        // ============================================================

        private static bool PortExists(
            string portName)
        {
            string script = $@"
$ErrorActionPreference = 'SilentlyContinue'

$port = Get-PrinterPort `
    -Name '{EscapePowerShell(portName)}' `
    -ErrorAction SilentlyContinue

if ($null -ne $port)
{{
    Write-Output 'EXISTS'
}}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out _);

            return output.Trim().Equals(
                "EXISTS",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // NORMAL REMOVE PRINTER PORT
        // ============================================================

        private static bool RemovePrinterPort(
            string portName)
        {
            string script = $@"
$ErrorActionPreference = 'Stop'

Remove-PrinterPort `
    -Name '{EscapePowerShell(portName)}' `
    -ErrorAction Stop

Write-Output 'REMOVED'
";

            string output =
                RunPowerShellCommand(
                    script,
                    out _);

            return output.Contains(
                "REMOVED",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // FORCE DELETE PORT FROM PRINT MONITOR REGISTRY
        // ============================================================

        private static bool ForceRemoveUsbPortRegistry(
            string portName,
            out string message)
        {
            message = "";

            string script = $@"
$ErrorActionPreference = 'SilentlyContinue'

$portName = '{EscapePowerShell(portName)}'
$removed = $false

$roots = @(
    'HKLM:\SYSTEM\CurrentControlSet\Control\Print\Monitors',
    'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Monitors'
)

foreach ($root in $roots)
{{
    if (-not (Test-Path $root))
    {{
        continue
    }}

    $keys = @()

    try
    {{
        $keys = @(
            Get-ChildItem `
                -Path $root `
                -Recurse `
                -ErrorAction SilentlyContinue
        )
    }}
    catch
    {{
        $keys = @()
    }}

    foreach ($key in $keys)
    {{
        $path = $key.PSPath

        if ($path -notmatch '\\Ports($|\\)')
        {{
            continue
        }}

        # Case 1: ...\Ports\USBxxx
        if ($key.PSChildName -ieq $portName)
        {{
            try
            {{
                Remove-Item `
                    -Path $key.PSPath `
                    -Recurse `
                    -Force `
                    -ErrorAction Stop

                if (-not (Test-Path $key.PSPath))
                {{
                    $removed = $true
                }}
            }}
            catch
            {{
            }}

            continue
        }}

        # Case 2: ...\Ports contains a value named USBxxx
        try
        {{
            foreach ($valueName in $key.GetValueNames())
            {{
                if ($valueName -ieq $portName)
                {{
                    Remove-ItemProperty `
                        -Path $key.PSPath `
                        -Name $valueName `
                        -Force `
                        -ErrorAction SilentlyContinue

                    $removed = $true
                }}
            }}
        }}
        catch
        {{
        }}
    }}
}}

if ($removed)
{{
    Write-Output 'REGISTRY_REMOVED'
}}
else
{{
    Write-Output 'NOT_FOUND'
}}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out string error);

            if (output.Contains(
                "REGISTRY_REMOVED",
                StringComparison.OrdinalIgnoreCase))
            {
                message =
                    $"Registry entry for '{portName}' was removed.";

                return true;
            }

            message =
                string.IsNullOrWhiteSpace(error)
                    ? $"Registry entry for '{portName}' was not found."
                    : error.Trim();

            return false;
        }

        // ============================================================
        // STOP PRINT SPOOLER
        // ============================================================

        private static bool StopSpooler(
            out string error)
        {
            error = "";

            string script = @"
$ErrorActionPreference = 'Stop'

$service =
    Get-Service -Name Spooler

if ($service.Status -ne 'Stopped')
{
    Stop-Service `
        -Name Spooler `
        -Force `
        -ErrorAction Stop
}

$service.Refresh()

$counter = 0

while (
    $service.Status -ne 'Stopped' -and
    $counter -lt 40
)
{
    Start-Sleep -Milliseconds 250

    $service.Refresh()

    $counter++
}

if ($service.Status -eq 'Stopped')
{
    Write-Output 'STOPPED'
}
else
{
    throw 'Print Spooler did not stop.'
}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out error);

            return output.Contains(
                "STOPPED",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // START PRINT SPOOLER
        // ============================================================

        private static bool StartSpooler(
            out string error)
        {
            error = "";

            string script = @"
$ErrorActionPreference = 'Stop'

$service =
    Get-Service -Name Spooler

if ($service.Status -ne 'Running')
{
    Start-Service `
        -Name Spooler `
        -ErrorAction Stop
}

$service.Refresh()

$counter = 0

while (
    $service.Status -ne 'Running' -and
    $counter -lt 40
)
{
    Start-Sleep -Milliseconds 250

    $service.Refresh()

    $counter++
}

if ($service.Status -eq 'Running')
{
    Write-Output 'STARTED'
}
else
{
    throw 'Print Spooler did not start.'
}
";

            string output =
                RunPowerShellCommand(
                    script,
                    out error);

            return output.Contains(
                "STARTED",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // RESTART + VERIFY
        // ============================================================

        private static bool RestartSpoolerAndVerify(
            string portName,
            string successMessage,
            out string finalMessage)
        {
            StartSpooler(out _);

            Thread.Sleep(1500);

            if (!PortExists(portName))
            {
                finalMessage =
                    successMessage +
                    "\n\nPrint Spooler restarted.";

                return true;
            }

            finalMessage =
                $"Port '{portName}' is still present after restart.";

            return false;
        }


        // ============================================================
        // POWERSHELL EXECUTION
        // ============================================================

        private static string RunPowerShellCommand(
            string script,
            out string error)
        {
            error = "";

            var startInfo =
                new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding =
                        new UTF8Encoding(false),
                    StandardErrorEncoding =
                        new UTF8Encoding(false)
                };

            startInfo.ArgumentList.Add("-NoLogo");
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add(script);


            using var process =
                new Process
                {
                    StartInfo = startInfo
                };


            try
            {
                process.Start();

                string output =
                    process.StandardOutput.ReadToEnd();

                error =
                    process.StandardError.ReadToEnd();

                process.WaitForExit(15000);

                return output.Trim();
            }
            catch (Exception ex)
            {
                error = ex.Message;

                return "";
            }
        }


        // ============================================================
        // ESCAPE POWERSHELL STRING
        // ============================================================

        private static string EscapePowerShell(
            string value)
        {
            return value.Replace(
                "'",
                "''");
        }


        // ============================================================
        // EXISTING GET PORT POWERSHELL
        // ============================================================

        private static string RunPowerShell()
        {
            const string script = @"
$ErrorActionPreference = 'Stop'

$utf8 =
    New-Object System.Text.UTF8Encoding($false)

[Console]::OutputEncoding =
    $utf8

$OutputEncoding =
    $utf8


$ports =
    Get-PrinterPort |
    Select-Object `
        Name,
        Description,
        PrinterHostAddress,
        PortNumber,
        Protocol,
        SNMPEnabled,
        ComputerName,
        PortMonitor


if ($null -eq $ports)
{
    Write-Output '[]'
}
else
{
    @($ports) |
        ConvertTo-Json -Compress
}
";


            var startInfo =
                new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding =
                        new UTF8Encoding(false),
                    StandardErrorEncoding =
                        new UTF8Encoding(false)
                };


            startInfo.ArgumentList.Add("-NoLogo");
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add(script);


            using var process =
                new Process
                {
                    StartInfo = startInfo
                };


            process.Start();


            string output =
                process.StandardOutput.ReadToEnd();

            string error =
                process.StandardError.ReadToEnd();


            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                }

                throw new Exception(
                    "Unable to read Printer Ports. " +
                    "Operation timed out after 10 seconds.");
            }


            if (process.ExitCode != 0)
            {
                throw new Exception(
                    "Unable to read Printer Ports.\n\n" +
                    error.Trim());
            }


            return output.Trim();
        }


        // ============================================================
        // JSON PARSER
        // ============================================================

        private static List<PrinterPortModel> ParseJson(
            string json)
        {
            var result =
                new List<PrinterPortModel>();


            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(json);


                JsonElement root =
                    document.RootElement;


                if (root.ValueKind ==
                    JsonValueKind.Array)
                {
                    foreach (JsonElement item
                        in root.EnumerateArray())
                    {
                        AddItem(
                            item,
                            result);
                    }
                }
                else if (
                    root.ValueKind ==
                    JsonValueKind.Object)
                {
                    AddItem(
                        root,
                        result);
                }


                result.Sort((a, b) =>
                    string.Compare(
                        a.Name,
                        b.Name,
                        StringComparison.OrdinalIgnoreCase));


                return result;
            }
            catch (JsonException ex)
            {
                throw new Exception(
                    "Unable to parse Printer Port information.\n\n" +
                    ex.Message);
            }
        }


        // ============================================================
        // ADD PORT MODEL
        // ============================================================

        private static void AddItem(
            JsonElement item,
            List<PrinterPortModel> result)
        {
            string name =
                GetString(
                    item,
                    "Name");


            string description =
                GetString(
                    item,
                    "Description");


            string portMonitor =
                GetString(
                    item,
                    "PortMonitor");


            description =
                GetEnglishDescription(
                    name,
                    description,
                    portMonitor);


            result.Add(
                new PrinterPortModel
                {
                    Name = name,

                    Description =
                        description,

                    PrinterHostAddress =
                        GetString(
                            item,
                            "PrinterHostAddress"),

                    PortNumber =
                        GetString(
                            item,
                            "PortNumber"),

                    Protocol =
                        GetString(
                            item,
                            "Protocol"),

                    SNMPEnabled =
                        GetString(
                            item,
                            "SNMPEnabled"),

                    ComputerName =
                        GetString(
                            item,
                            "ComputerName"),

                    PortMonitor =
                        portMonitor
                });
        }


        // ============================================================
        // ENGLISH DESCRIPTION
        // ============================================================

        private static string GetEnglishDescription(
            string name,
            string originalDescription,
            string portMonitor)
        {
            string monitor =
                portMonitor.Trim();


            if (monitor.Equals(
                "Local Monitor",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Local Port";
            }


            if (monitor.IndexOf(
                "TCP/IP",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Standard TCP/IP Port";
            }


            if (monitor.IndexOf(
                "WSD",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "WSD Port";
            }


            if (monitor.IndexOf(
                "IPP",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "IPP Port";
            }


            if (!string.IsNullOrWhiteSpace(
                originalDescription) &&
                !originalDescription.Contains('\uFFFD'))
            {
                return originalDescription.Trim();
            }


            if (!string.IsNullOrWhiteSpace(
                monitor))
            {
                return monitor;
            }


            return "Printer Port";
        }


        // ============================================================
        // GET JSON STRING
        // ============================================================

        private static string GetString(
            JsonElement item,
            string property)
        {
            if (!item.TryGetProperty(
                property,
                out JsonElement value))
            {
                return "";
            }


            if (value.ValueKind ==
                JsonValueKind.Null ||
                value.ValueKind ==
                JsonValueKind.Undefined)
            {
                return "";
            }


            return value.ToString().Trim();
        }
    }
}