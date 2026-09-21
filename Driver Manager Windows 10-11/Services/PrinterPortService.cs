using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class PrinterPortService
    {
        public static List<PrinterPortModel> GetPorts()
        {
            string json = RunPowerShell();

            if (string.IsNullOrWhiteSpace(json))
                return new List<PrinterPortModel>();

            return ParseJson(json);
        }

        private static string RunPowerShell()
        {
            const string script = @"
$ErrorActionPreference = 'Stop'

# Force PowerShell output to UTF-8 so Thai / Unicode descriptions
# are transferred to the C# application without mojibake.
$utf8 = New-Object System.Text.UTF8Encoding($false)
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

$ports = Get-PrinterPort | Select-Object `
    Name,
    Description,
    PrinterHostAddress,
    PortNumber,
    Protocol,
    SNMPEnabled,
    ComputerName,
    PortMonitor

if ($null -eq $ports) {
    Write-Output '[]'
}
else {
    @($ports) | ConvertTo-Json -Compress
}
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                // IMPORTANT:
                // PowerShell is forced to UTF-8 above, so C# must read UTF-8 too.
                StandardOutputEncoding = new UTF8Encoding(false),
                StandardErrorEncoding = new UTF8Encoding(false)
            };

            startInfo.ArgumentList.Add("-NoLogo");
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add(script);

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync();

            Task<string> errorTask =
                process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    // Ignore cleanup errors.
                }

                throw new Exception(
                    "Unable to read Printer Ports. Operation timed out after 10 seconds.");
            }

            string output =
                outputTask.GetAwaiter().GetResult();

            string error =
                errorTask.GetAwaiter().GetResult();

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    "Unable to read Printer Ports.\n\n" +
                    error.Trim());
            }

            return output.Trim();
        }

        private static List<PrinterPortModel> ParseJson(string json)
        {
            var result = new List<PrinterPortModel>();

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);

                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in root.EnumerateArray())
                        AddItem(item, result);
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    AddItem(root, result);
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

        private static void AddItem(
            JsonElement item,
            List<PrinterPortModel> result)
        {
            string name = GetString(item, "Name");
            string description = GetString(item, "Description");
            string portMonitor = GetString(item, "PortMonitor");

            // Keep the UI English-first. If Windows returns a localized
            // description, use a readable English description derived from
            // the port monitor instead of showing broken/localized text.
            description = GetEnglishDescription(name, description, portMonitor);

            result.Add(
                new PrinterPortModel
                {
                    Name = name,
                    Description = description,
                    PrinterHostAddress =
                        GetString(item, "PrinterHostAddress"),
                    PortNumber = GetString(item, "PortNumber"),
                    Protocol = GetString(item, "Protocol"),
                    SNMPEnabled = GetString(item, "SNMPEnabled"),
                    ComputerName = GetString(item, "ComputerName"),
                    PortMonitor = portMonitor
                });
        }

        private static string GetEnglishDescription(
            string name,
            string originalDescription,
            string portMonitor)
        {
            string monitor = portMonitor.Trim();

            if (monitor.Equals("Local Monitor", StringComparison.OrdinalIgnoreCase))
                return "Local Port";

            if (monitor.IndexOf("TCP/IP", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Standard TCP/IP Port";

            if (monitor.IndexOf("WSD", StringComparison.OrdinalIgnoreCase) >= 0)
                return "WSD Port";

            if (monitor.IndexOf("IPP", StringComparison.OrdinalIgnoreCase) >= 0)
                return "IPP Port";

            // If the original text is present and contains no replacement
            // characters, keep it. Otherwise use a safe English fallback.
            if (!string.IsNullOrWhiteSpace(originalDescription) &&
                !originalDescription.Contains('\uFFFD'))
            {
                return originalDescription.Trim();
            }

            if (!string.IsNullOrWhiteSpace(monitor))
                return monitor;

            return "Printer Port";
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

            if (value.ValueKind == JsonValueKind.Null ||
                value.ValueKind == JsonValueKind.Undefined)
            {
                return "";
            }

            return value.ToString().Trim();
        }
    }
}
