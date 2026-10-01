using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    /// <summary>
    /// Printer inventory scanner.
    /// Finds printer devices first (Present + Not Present), then merges
    /// PrintManagement queue/port information when available.
    /// Read-only.
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

            string json = RunPowerShell(BuildScanScript(server));

            if (string.IsNullOrWhiteSpace(json))
                return new List<PrintServerPrinterModel>();

            return ParseJson(json, includeNormal);
        }

        private static string NormalizeServerName(string value)
        {
            string server = value?.Trim() ?? "";

            if (server.StartsWith(@"\\", StringComparison.Ordinal))
                server = server.Substring(2);

            int slash = server.IndexOf('\\');

            if (slash >= 0)
                server = server.Substring(0, slash);

            return server.Trim();
        }

        private static string BuildScanScript(string server)
        {
            string escaped = server.Replace("'", "''");

            string script = @"
$ErrorActionPreference = 'Stop'

Import-Module PrintManagement -ErrorAction SilentlyContinue
Import-Module PnpDevice -ErrorAction SilentlyContinue

$server = '__SERVER__'

$result = New-Object System.Collections.Generic.List[object]

# ============================================================
# 1. PRINT QUEUES
# ============================================================
$queues = @()

try {
    $queues = @(
        Get-Printer -ComputerName $server -Full -ErrorAction Stop
    )
}
catch {
    $queues = @()
}

# ============================================================
# 2. PRINTER PORTS
# ============================================================
$ports = @()

try {
    $ports = @(
        Get-PrinterPort -ComputerName $server -ErrorAction Stop
    )
}
catch {
    $ports = @()
}

$portMap = @{}

foreach ($port in $ports) {
    $portName = [string]$port.Name

    if (-not [string]::IsNullOrWhiteSpace($portName)) {
        $portMap[$portName] = [string]$port.PortMonitor
    }
}

# ============================================================
# 3. PNP PRINTER DEVICES
#
# IMPORTANT: no -PresentOnly filter.
# ============================================================
$pnp = @()

try {
    if ($server -eq $env:COMPUTERNAME) {
        $pnp = @(
            Get-PnpDevice -Class Printer -PresentOnly:$false -ErrorAction Stop
        )
    }
}
catch {
    $pnp = @()
}

# ============================================================
# 4. WIN32_PNPEntity
# ============================================================
$entities = @()

try {
    $entities = @(
        Get-CimInstance `
            -ComputerName $server `
            -ClassName Win32_PnPEntity `
            -Filter ""ClassGuid = '{4D36E979-E325-11CE-BFC1-08002BE10318}'"" `
            -ErrorAction Stop
    )
}
catch {
    $entities = @()
}

# ============================================================
# 5. INDEX QUEUES BY NAME
# ============================================================
$queueMap = @{}

foreach ($q in $queues) {
    $name = [string]$q.Name

    if (-not [string]::IsNullOrWhiteSpace($name)) {
        $queueMap[$name.ToLowerInvariant()] = $q
    }
}

# ============================================================
# 6. INDEX PNP DATA BY INSTANCE ID
# ============================================================
$pnpMap = @{}

foreach ($d in $pnp) {
    $id = [string]$d.InstanceId

    if (-not [string]::IsNullOrWhiteSpace($id)) {
        $pnpMap[$id.ToLowerInvariant()] = $d
    }
}

foreach ($d in $entities) {
    $id = [string]$d.PNPDeviceID

    if (-not [string]::IsNullOrWhiteSpace($id)) {
        $pnpMap[$id.ToLowerInvariant()] = $d
    }
}

$usedNames = @{}

# ============================================================
# 7. ADD PNP DEVICES FIRST
# ============================================================
foreach ($device in $pnpMap.Values) {

    $name = [string]$device.FriendlyName

    if ([string]::IsNullOrWhiteSpace($name)) {
        $name = [string]$device.Name
    }

    if ([string]::IsNullOrWhiteSpace($name)) {
        $name = [string]$device.Caption
    }

    if ([string]::IsNullOrWhiteSpace($name)) {
        continue
    }

    $instanceId = [string]$device.InstanceId

    if ([string]::IsNullOrWhiteSpace($instanceId)) {
        $instanceId = [string]$device.PNPDeviceID
    }

    $entityId = [string]$device.PNPDeviceID
    $present = $true
    $status = [string]$device.Status
    $manufacturer = [string]$device.Manufacturer
    $classGuid = [string]$device.ClassGuid
    $errorCode = [string]$device.ConfigManagerErrorCode

    if ($null -ne $device.Present) {
        $present = [bool]$device.Present
    }

    $queue = $null
    $queueKey = $name.ToLowerInvariant()

    if ($queueMap.ContainsKey($queueKey)) {
        $queue = $queueMap[$queueKey]
    }

    $portName = ''
    $driverName = ''
    $shareName = ''
    $portMonitor = ''
    $jobCount = '0'
    $jobsText = ''
    $shared = $false
    $published = $false
    $workOffline = $false
    $state = ''
    $reason = ''

    if ($null -ne $queue) {

        $driverName = [string]$queue.DriverName
        $portName = [string]$queue.PortName
        $shareName = [string]$queue.ShareName
        $shared = [bool]$queue.Shared
        $published = [bool]$queue.Published
        $workOffline = [bool]$queue.WorkOffline

        if (-not [string]::IsNullOrWhiteSpace($portName) -and
            $portMap.ContainsKey($portName)) {
            $portMonitor = [string]$portMap[$portName]
        }

        $jobs = @()

        try {
            $jobs = @(
                Get-PrintJob `
                    -ComputerName $server `
                    -PrinterName $name `
                    -ErrorAction Stop
            )
        }
        catch {
            $jobs = @()
        }

        $jobCount = [string]@($jobs).Count

        $jobDescriptions = @(
            $jobs | ForEach-Object {
                $id = $_.ID
                $document = [string]$_.DocumentName
                $jobStatus = [string]$_.JobStatus

                if ([string]::IsNullOrWhiteSpace($jobStatus)) {
                    $jobStatus = [string]$_.Status
                }

                if ([string]::IsNullOrWhiteSpace($jobStatus)) {
                    $jobStatus = 'Queued'
                }

                ""#$id | $document | $jobStatus""
            }
        )

        $jobsText = $jobDescriptions -join ""`n""

        $jobProblem = $false

        foreach ($job in $jobs) {
            $jobText = ""$($job.JobStatus) $($job.Status)""

            if ($jobText -match
                '(?i)error|failed|blocked|paused|offline|paper|stuck|deleting') {
                $jobProblem = $true
                break
            }
        }

        if ($workOffline) {
            $state = 'Work Offline'
            $reason = 'Windows reports this printer queue as Work Offline.'
        }
        elseif ($jobProblem) {
            $state = 'Print Job Problem'
            $reason = 'One or more print jobs have a problem state.'
        }
        elseif (@($jobs).Count -gt 0) {
            $state = 'Has Jobs'
            $reason = ""Printer currently has $(@($jobs).Count) print job(s).""
        }
        elseif (-not $present) {
            $state = 'Disconnected'
            $reason = 'Printer device is stored in Windows but is not currently present.'
        }
        else {
            $state = 'Normal'
            $reason = 'Printer device and Print Queue were found.'
        }
    }
    else {
        if ($present) {
            $state = 'PnP Device'
            $reason = 'Printer device was found in the PnP inventory; no matching Print Queue was found.'
        }
        else {
            $state = 'Disconnected'
            $reason = 'Printer device remains in Windows PnP inventory but is not currently present.'
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($errorCode) -and
        $errorCode -ne '0') {
        $reason += "" ConfigManagerErrorCode=$errorCode.""
    }

    $source = 'PnP Device'

    if ($null -ne $queue) {
        $source = 'PnP + Queue'
    }

    $result.Add(
        [PSCustomObject]@{
            ServerName = $server
            PrinterName = $name
            ShareName = $shareName
            DriverName = $driverName
            PortName = $portName
            PortMonitor = $portMonitor
            Status = $status
            PrinterState = if ($null -ne $queue) { [string]$queue.PrinterStatus } else { $status }
            State = $state
            Reason = $reason
            JobCount = $jobCount
            Jobs = $jobsText
            Source = $source
            DeviceId = if (-not [string]::IsNullOrWhiteSpace($entityId)) { $entityId } else { $instanceId }
            Manufacturer = $manufacturer
            ClassGuid = $classGuid
            Present = $present
            Shared = $shared
            Published = $published
            WorkOffline = $workOffline
        }
    )

    $usedNames[$name.ToLowerInvariant()] = $true
}

# ============================================================
# 8. ADD QUEUE-ONLY PRINTERS
# ============================================================
foreach ($queue in $queues) {

    $name = [string]$queue.Name

    if ([string]::IsNullOrWhiteSpace($name)) {
        continue
    }

    $key = $name.ToLowerInvariant()

    if ($usedNames.ContainsKey($key)) {
        continue
    }

    $portName = [string]$queue.PortName
    $monitor = ''

    if (-not [string]::IsNullOrWhiteSpace($portName) -and
        $portMap.ContainsKey($portName)) {
        $monitor = [string]$portMap[$portName]
    }

    $jobs = @()

    try {
        $jobs = @(
            Get-PrintJob `
                -ComputerName $server `
                -PrinterName $name `
                -ErrorAction Stop
        )
    }
    catch {
        $jobs = @()
    }

    $state = 'Normal'
    $reason = 'Print Queue was found, but no matching PnP printer device was found.'

    if ($queue.WorkOffline) {
        $state = 'Work Offline'
        $reason = 'Windows reports this printer queue as Work Offline.'
    }
    elseif (@($jobs).Count -gt 0) {
        $state = 'Has Jobs'
        $reason = ""Printer currently has $(@($jobs).Count) print job(s).""
    }

    $result.Add(
        [PSCustomObject]@{
            ServerName = $server
            PrinterName = $name
            ShareName = [string]$queue.ShareName
            DriverName = [string]$queue.DriverName
            PortName = $portName
            PortMonitor = $monitor
            Status = [string]$queue.Status
            PrinterState = [string]$queue.PrinterStatus
            State = $state
            Reason = $reason
            JobCount = [string]@($jobs).Count
            Jobs = ''
            Source = 'Print Queue'
            DeviceId = ''
            Manufacturer = ''
            ClassGuid = ''
            Present = $true
            Shared = [bool]$queue.Shared
            Published = [bool]$queue.Published
            WorkOffline = [bool]$queue.WorkOffline
        }
    )
}

$result |
    Sort-Object PrinterName |
    ConvertTo-Json -Depth 10 -Compress
";

            return script.Replace("__SERVER__", escaped);
        }

        private static string RunPowerShell(string script)
        {
            string encoded =
                Convert.ToBase64String(
                    Encoding.Unicode.GetBytes(script));

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
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
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!process.WaitForExit(30000))
            {
                try { process.Kill(true); }
                catch { }

                throw new TimeoutException(
                    "Printer device scan timed out after 30 seconds.");
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(error)
                        ? "Windows could not query printer devices."
                        : error.Trim());
            }

            return output.Trim();
        }

        private static List<PrintServerPrinterModel> ParseJson(
            string json,
            bool includeNormal)
        {
            var result = new List<PrintServerPrinterModel>();

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                PrintServerPrinterModel? item =
                    root.Deserialize<PrintServerPrinterModel>();

                if (item != null &&
                    (includeNormal ||
                     !string.Equals(
                         item.State,
                         "Normal",
                         StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(item);
                }

                return result;
            }

            if (root.ValueKind != JsonValueKind.Array)
                return result;

            foreach (JsonElement element in root.EnumerateArray())
            {
                PrintServerPrinterModel? item =
                    element.Deserialize<PrintServerPrinterModel>();

                if (item == null)
                    continue;

                if (!includeNormal &&
                    string.Equals(
                        item.State,
                        "Normal",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(item);
            }

            return result;
        }
    }
}
