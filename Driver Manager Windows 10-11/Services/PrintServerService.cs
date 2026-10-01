using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Reads printer queues, ports and print jobs from a Windows Print Server.
    /// This service is read-only.
    /// </summary>
    public static class PrintServerService
    {
        public static List<PrintServerPrinterModel> GetServerPrinters(
            string serverName,
            bool includeNormal = true)
        {
            string server = NormalizeServerName(serverName);

            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentException(
                    "Print Server name is required.",
                    nameof(serverName));

            string json =
                RunPowerShell(
                    BuildScanScript(server));

            if (string.IsNullOrWhiteSpace(json))
                return new List<PrintServerPrinterModel>();

            return ParseJson(
                json,
                includeNormal);
        }

        private static string NormalizeServerName(
            string value)
        {
            string server =
                value?.Trim() ?? "";

            if (server.StartsWith(
                @"\\",
                StringComparison.Ordinal))
            {
                server =
                    server.Substring(2);
            }

            int slash =
                server.IndexOf('\\');

            if (slash >= 0)
                server =
                    server.Substring(
                        0,
                        slash);

            return server.Trim();
        }

        private static string BuildScanScript(
            string server)
        {
            string escaped =
                server.Replace(
                    "'",
                    "''");

            return $@"
$ErrorActionPreference = 'Stop'

Import-Module PrintManagement -ErrorAction Stop

$server = '{escaped}'

$printers = @(
    Get-Printer `
        -ComputerName $server `
        -Full `
        -ErrorAction Stop
)

$ports = @()

try {{
    $ports = @(
        Get-PrinterPort `
            -ComputerName $server `
            -ErrorAction Stop
    )
}}
catch {{
    $ports = @()
}}

$portMap = @{{}}

foreach ($port in $ports) {{
    if (-not [string]::IsNullOrWhiteSpace([string]$port.Name)) {{
        $portMap[[string]$port.Name] =
            [string]$port.PortMonitor
    }}
}}

$result = foreach ($printer in $printers) {{

    $jobs = @()

    try {{
        $jobs = @(
            Get-PrintJob `
                -ComputerName $server `
                -PrinterName ([string]$printer.Name) `
                -ErrorAction Stop
        )
    }}
    catch {{
        $jobs = @()
    }}

    $jobDescriptions = @(
        $jobs |
        ForEach-Object {{
            $id = $_.ID
            $document = [string]$_.DocumentName
            $status = [string]$_.JobStatus

            if ([string]::IsNullOrWhiteSpace($status)) {{
                $status = [string]$_.Status
            }}

            if ([string]::IsNullOrWhiteSpace($status)) {{
                $status = 'Queued'
            }}

            ""#$id | $document | $status""
        }}
    )

    $jobProblem = $false

    foreach ($job in $jobs) {{
        $jobText =
            ""$($job.JobStatus) $($job.Status)""

        if ($jobText -match
            '(?i)error|failed|blocked|paused|offline|paper|stuck|deleting') {{
            $jobProblem = $true
            break
        }}
    }}

    $state = 'Normal'
    $reason = 'Printer queue is available on the Print Server.'

    if ($printer.WorkOffline) {{
        $state = 'Work Offline'
        $reason = 'Windows reports this queue as Work Offline.'
    }}
    elseif ($jobProblem) {{
        $state = 'Print Job Problem'
        $reason = 'One or more print jobs have a problem state.'
    }}
    elseif (@($jobs).Count -gt 0) {{
        $state = 'Has Jobs'
        $reason =
            ""Printer currently has $(@($jobs).Count) print job(s).""
    }}

    $portName =
        [string]$printer.PortName

    $monitor = ''

    if (-not [string]::IsNullOrWhiteSpace($portName) -and
        $portMap.ContainsKey($portName)) {{
        $monitor =
            [string]$portMap[$portName]
    }}

    [PSCustomObject]@{{
        ServerName =
            $server

        PrinterName =
            [string]$printer.Name

        ShareName =
            [string]$printer.ShareName

        DriverName =
            [string]$printer.DriverName

        PortName =
            $portName

        PortMonitor =
            $monitor

        Status =
            [string]$printer.Status

        PrinterState =
            [string]$printer.PrinterStatus

        State =
            $state

        Reason =
            $reason

        JobCount =
            [string]@($jobs).Count

        Jobs =
            ($jobDescriptions -join ""`n"")

        Shared =
            [bool]$printer.Shared

        Published =
            [bool]$printer.Published

        WorkOffline =
            [bool]$printer.WorkOffline
    }}
}}

$result |
    ConvertTo-Json `
        -Depth 8 `
        -Compress
";
        }

        private static string RunPowerShell(
            string script)
        {
            string encoded =
                Convert.ToBase64String(
                    Encoding.Unicode.GetBytes(
                        script));

            var startInfo =
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
                    CreateNoWindow = true,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,

                    StandardOutputEncoding =
                        Encoding.UTF8,

                    StandardErrorEncoding =
                        Encoding.UTF8
                };

            using var process =
                new Process
                {
                    StartInfo = startInfo
                };

            process.Start();

            var outputTask =
                process.StandardOutput.ReadToEndAsync();

            var errorTask =
                process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(30000))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                }

                throw new TimeoutException(
                    "Print Server scan timed out after 30 seconds.");
            }

            string output =
                outputTask.GetAwaiter().GetResult();

            string error =
                errorTask.GetAwaiter().GetResult();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(error)
                        ? "Windows could not query the Print Server."
                        : error.Trim());
            }

            return output.Trim();
        }

        private static List<PrintServerPrinterModel> ParseJson(
            string json,
            bool includeNormal)
        {
            var result =
                new List<PrintServerPrinterModel>();

            using JsonDocument document =
                JsonDocument.Parse(json);

            JsonElement root =
                document.RootElement;

            if (root.ValueKind ==
                JsonValueKind.Object)
            {
                AddItem(root, result);
            }
            else if (root.ValueKind ==
                     JsonValueKind.Array)
            {
                foreach (JsonElement item
                    in root.EnumerateArray())
                {
                    AddItem(item, result);
                }
            }

            if (!includeNormal)
            {
                result.RemoveAll(item =>
                    item.State.Equals(
                        "Normal",
                        StringComparison.OrdinalIgnoreCase));
            }

            result.Sort((a, b) =>
                string.Compare(
                    a.PrinterName,
                    b.PrinterName,
                    StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private static void AddItem(
            JsonElement item,
            List<PrintServerPrinterModel> result)
        {
            var model =
                new PrintServerPrinterModel
                {
                    ServerName =
                        GetString(item, "ServerName"),

                    PrinterName =
                        GetString(item, "PrinterName"),

                    ShareName =
                        GetString(item, "ShareName"),

                    DriverName =
                        GetString(item, "DriverName"),

                    PortName =
                        GetString(item, "PortName"),

                    PortMonitor =
                        GetString(item, "PortMonitor"),

                    Status =
                        GetString(item, "Status"),

                    PrinterState =
                        GetString(item, "PrinterState"),

                    State =
                        GetString(item, "State"),

                    Reason =
                        GetString(item, "Reason"),

                    JobCount =
                        GetString(item, "JobCount"),

                    Jobs =
                        GetString(item, "Jobs"),

                    Shared =
                        GetBool(item, "Shared"),

                    Published =
                        GetBool(item, "Published"),

                    WorkOffline =
                        GetBool(item, "WorkOffline")
                };

            if (!string.IsNullOrWhiteSpace(
                model.PrinterName))
            {
                result.Add(model);
            }
        }

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
                JsonValueKind.Null)
            {
                return "";
            }

            return value.ToString().Trim();
        }

        private static bool GetBool(
            JsonElement item,
            string property)
        {
            if (!item.TryGetProperty(
                    property,
                    out JsonElement value))
            {
                return false;
            }

            if (value.ValueKind ==
                JsonValueKind.True)
            {
                return true;
            }

            if (value.ValueKind ==
                JsonValueKind.False)
            {
                return false;
            }

            return bool.TryParse(
                value.ToString(),
                out bool result) &&
                   result;
        }
    }
}
