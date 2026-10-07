using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class InstalledPrinterRegistryService
    {
        // ============================================================
        // GET PRINTERS FROM REGISTRY
        // ============================================================

        public static List<InstalledPrinterModel> GetRegistryPrinters()
        {
            var result =
                new List<InstalledPrinterModel>();

            const string script = @"
$ErrorActionPreference = 'SilentlyContinue'

$roots = @(
    'HKLM:\SYSTEM\CurrentControlSet\Control\Print\Printers',
    'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Printers'
)

$items = @()

foreach ($root in $roots)
{
    if (-not (Test-Path $root))
    {
        continue
    }

    foreach ($key in Get-ChildItem $root -ErrorAction SilentlyContinue)
    {
        $name = $key.PSChildName

        if ([string]::IsNullOrWhiteSpace($name))
        {
            continue
        }

        $driverName =
            [string]$key.GetValue('PrinterDriverName', '')

        if ([string]::IsNullOrWhiteSpace($driverName))
        {
            $driverName =
                [string]$key.GetValue('DriverName', '')
        }

        $portName =
            [string]$key.GetValue('Port', '')

        if ([string]::IsNullOrWhiteSpace($portName))
        {
            $portName =
                [string]$key.GetValue('PortName', '')
        }

        $serverName =
            [string]$key.GetValue('ServerName', '')

        $shareName =
            [string]$key.GetValue('ShareName', '')

        $location =
            [string]$key.GetValue('Location', '')

        $comment =
            [string]$key.GetValue('Comment', '')

        $items += [PSCustomObject]@{
            Name       = $name
            DriverName = $driverName
            PortName   = $portName
            ServerName = $serverName
            ShareName  = $shareName
            Location   = $location
            Comment    = $comment
        }
    }
}

@($items |
    Sort-Object Name -Unique) |
    ConvertTo-Json -Compress
";

            string output =
                RunPowerShell(
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

                if (root.ValueKind ==
                    JsonValueKind.Array)
                {
                    foreach (JsonElement item
                        in root.EnumerateArray())
                    {
                        AddRegistryPrinter(
                            item,
                            result);
                    }
                }
                else if (root.ValueKind ==
                         JsonValueKind.Object)
                {
                    AddRegistryPrinter(
                        root,
                        result);
                }
            }
            catch (JsonException)
            {
                // ไม่ทำให้ InstalledPrinterService ตัวหลักเสีย
            }

            result.Sort((a, b) =>
                string.Compare(
                    a.Name,
                    b.Name,
                    StringComparison.OrdinalIgnoreCase));

            return result;
        }


        // ============================================================
        // GET ONLY REGISTRY-ONLY PRINTERS
        //
        // Registry มี แต่ Windows Get-Printer ไม่มี
        // ============================================================

        public static List<InstalledPrinterModel>
            GetRegistryOnlyPrinters()
        {
            var result =
                new List<InstalledPrinterModel>();

            List<InstalledPrinterModel> registryPrinters =
                GetRegistryPrinters();

            HashSet<string> windowsPrinters =
                GetWindowsPrinterNames();

            foreach (InstalledPrinterModel printer
                in registryPrinters)
            {
                if (string.IsNullOrWhiteSpace(printer.Name))
                    continue;

                if (!windowsPrinters.Contains(
                    printer.Name))
                {
                    printer.PrinterType =
                        "Registry Only";

                    printer.Status =
                        "Registry Only";

                    result.Add(printer);
                }
            }

            return result;
        }


        // ============================================================
        // GET WINDOWS PRINTER NAMES
        // ============================================================

        private static HashSet<string>
            GetWindowsPrinterNames()
        {
            var result =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            const string script = @"
$ErrorActionPreference = 'SilentlyContinue'

@(
    Get-Printer -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty Name
) |
    Where-Object {
        -not [string]::IsNullOrWhiteSpace($_)
    } |
    ForEach-Object {
        Write-Output $_
    }
";

            string output =
                RunPowerShell(
                    script,
                    out _);

            if (string.IsNullOrWhiteSpace(output))
                return result;

            foreach (string line
                in output.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries))
            {
                string name =
                    line.Trim();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    result.Add(name);
                }
            }

            return result;
        }


        // ============================================================
        // CREATE MODEL
        // ============================================================

        private static void AddRegistryPrinter(
            JsonElement item,
            List<InstalledPrinterModel> result)
        {
            string name =
                GetJsonString(
                    item,
                    "Name");

            if (string.IsNullOrWhiteSpace(name))
                return;

            string driverName =
                GetJsonString(
                    item,
                    "DriverName");

            string portName =
                GetJsonString(
                    item,
                    "PortName");

            string serverName =
                GetJsonString(
                    item,
                    "ServerName");

            string shareName =
                GetJsonString(
                    item,
                    "ShareName");

            string location =
                GetJsonString(
                    item,
                    "Location");

            string comment =
                GetJsonString(
                    item,
                    "Comment");

            result.Add(
                new InstalledPrinterModel
                {
                    Name = name,

                    DriverName =
                        driverName,

                    Vendor = "Unknown",

                    PrinterType =
                        "Registry",

                    PortName =
                        portName,

                    HostAddress =
                        ExtractHostAddress(
                            portName),

                    Status =
                        "Registry",

                    Default = "No",

                    Network =
                        string.IsNullOrWhiteSpace(
                            serverName)
                            ? "No"
                            : "Yes",

                    Shared =
                        string.IsNullOrWhiteSpace(
                            shareName)
                            ? "No"
                            : "Yes",

                    WorkOffline = "No",

                    ServerName =
                        serverName,

                    Location =
                        location,

                    Comment =
                        comment
                });
        }


        // ============================================================
        // HOST ADDRESS
        // ============================================================

        private static string ExtractHostAddress(
            string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return "";

            string value =
                portName.Trim();

            if (value.StartsWith(
                "IP_",
                StringComparison.OrdinalIgnoreCase))
            {
                return value.Substring(3);
            }

            if (
                System.Net.IPAddress.TryParse(
                    value,
                    out _))
            {
                return value;
            }

            if (
                Uri.TryCreate(
                    value,
                    UriKind.Absolute,
                    out Uri? uri))
            {
                return uri.Host;
            }

            return "";
        }


        // ============================================================
        // JSON STRING
        // ============================================================

        private static string GetJsonString(
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


        // ============================================================
        // POWERSHELL
        // ============================================================

        private static string RunPowerShell(
            string script,
            out string error)
        {
            error = "";

            try
            {
                string encoded =
                    Convert.ToBase64String(
                        Encoding.Unicode.GetBytes(
                            script));

                using var process =
                    new Process();

                process.StartInfo =
                    new ProcessStartInfo
                    {
                        FileName =
                            "powershell.exe",

                        Arguments =
                            "-NoLogo -NoProfile " +
                            "-NonInteractive " +
                            "-ExecutionPolicy Bypass " +
                            "-EncodedCommand " +
                            encoded,

                        UseShellExecute = false,

                        RedirectStandardOutput = true,

                        RedirectStandardError = true,

                        CreateNoWindow = true
                    };

                process.Start();

                string output =
                    process.StandardOutput.ReadToEnd();

                error =
                    process.StandardError.ReadToEnd();

                if (!process.WaitForExit(
                        30000))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                    }

                    error =
                        "Registry scan timed out.";

                    return "";
                }

                return output.Trim();
            }
            catch (Exception ex)
            {
                error =
                    ex.Message;

                return "";
            }
        }
    }
}