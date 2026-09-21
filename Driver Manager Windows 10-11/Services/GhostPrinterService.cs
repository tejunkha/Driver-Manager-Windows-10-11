using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class GhostPrinterService
    {
        // Windows Printer setup class GUID.
        private const string PrinterClassGuid =
            "{4D36E979-E325-11CE-BFC1-08002BE10318}";

        public static List<GhostPrinterModel> GetGhostPrinters()
        {
            string pnputilPath =
                Path.Combine(
                    Environment.SystemDirectory,
                    "pnputil.exe");

            if (!File.Exists(pnputilPath))
            {
                throw new Exception(
                    "ไม่พบ pnputil.exe ใน Windows System32");
            }

            string output = RunPnpUtil(pnputilPath);

            if (string.IsNullOrWhiteSpace(output))
            {
                return new List<GhostPrinterModel>();
            }

            return ParseOutput(output);
        }

        private static string RunPnpUtil(string pnputilPath)
        {
            // /disconnected = device is not currently present.
            // /class GUID   = only Printer device setup class.
            string arguments =
                $"/enum-devices /disconnected /class \"{PrinterClassGuid}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = pnputilPath,
                Arguments = arguments,

                UseShellExecute = false,
                CreateNoWindow = true,

                RedirectStandardOutput = true,
                RedirectStandardError = true,

                // PnPUtil follows the active Windows console code page.
                StandardOutputEncoding = Encoding.Default,
                StandardErrorEncoding = Encoding.Default
            };

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
                    // Ignore kill errors.
                }

                throw new Exception(
                    "ตรวจสอบ Ghost Printer ใช้เวลานานเกิน 10 วินาที");
            }

            string output =
                outputTask.GetAwaiter().GetResult();

            string error =
                errorTask.GetAwaiter().GetResult();

            // PnPUtil returns non-zero for unsupported/invalid arguments.
            if (process.ExitCode != 0)
            {
                string combined =
                    string.IsNullOrWhiteSpace(error)
                        ? output
                        : error;

                // "No matching devices found" is a normal empty result.
                if (ContainsNoMatchingDevices(combined))
                {
                    return "";
                }

                throw new Exception(
                    "ไม่สามารถตรวจสอบ Ghost Printer ได้\n\n" +
                    combined.Trim() +
                    $"\n\nExit Code: {process.ExitCode}");
            }

            return output.Trim();
        }

        private static bool ContainsNoMatchingDevices(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            return text.Contains(
                       "No matching devices found",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   text.Contains(
                       "No devices found",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static List<GhostPrinterModel> ParseOutput(
            string output)
        {
            var result =
                new List<GhostPrinterModel>();

            string normalized =
                output.Replace("\r\n", "\n")
                      .Replace('\r', '\n');

            // Remove the PnPUtil title/header from the first block.
            string[] blocks =
                Regex.Split(
                    normalized.Trim(),
                    @"\n\s*\n");

            foreach (string block in blocks)
            {
                GhostPrinterModel? item =
                    ParseBlock(block);

                if (item == null)
                    continue;

                bool duplicate = false;

                foreach (GhostPrinterModel existing in result)
                {
                    if (!string.IsNullOrWhiteSpace(item.DeviceId) &&
                        existing.DeviceId.Equals(
                            item.DeviceId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        duplicate = true;
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(item.DeviceId) &&
                        existing.Name.Equals(
                            item.Name,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    result.Add(item);
                }
            }

            result.Sort((a, b) =>
                string.Compare(
                    a.Name,
                    b.Name,
                    StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private static GhostPrinterModel? ParseBlock(
            string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            string[] lines =
                block.Split(
                    '\n',
                    StringSplitOptions.RemoveEmptyEntries);

            var values = new List<string>();

            string? instanceId = null;
            string? description = null;
            string? manufacturer = null;
            string? className = null;
            string? classGuid = null;
            string? status = null;
            uint problemCode = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.Length == 0)
                    continue;

                // Ignore program title.
                if (line.Equals(
                        "Microsoft PnP Utility",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int colon =
                    line.IndexOf(':');

                if (colon < 0)
                    continue;

                string label =
                    line[..colon].Trim();

                string value =
                    line[(colon + 1)..].Trim();

                if (value.Length == 0)
                    continue;

                values.Add(value);

                // English labels used by PnPUtil.
                if (label.Equals(
                        "Instance ID",
                        StringComparison.OrdinalIgnoreCase))
                {
                    instanceId = value;
                }
                else if (label.Equals(
                        "Device Description",
                        StringComparison.OrdinalIgnoreCase))
                {
                    description = value;
                }
                else if (label.Equals(
                        "Manufacturer Name",
                        StringComparison.OrdinalIgnoreCase))
                {
                    manufacturer = value;
                }
                else if (label.Equals(
                        "Class Name",
                        StringComparison.OrdinalIgnoreCase))
                {
                    className = value;
                }
                else if (label.Equals(
                        "Class GUID",
                        StringComparison.OrdinalIgnoreCase))
                {
                    classGuid = value;
                }
                else if (label.Equals(
                        "Status",
                        StringComparison.OrdinalIgnoreCase))
                {
                    status = value;
                }
                else if (label.Equals(
                        "Problem Code",
                        StringComparison.OrdinalIgnoreCase))
                {
                    problemCode = ParseProblemCode(value);
                }
            }

            // ---------------------------------------------------------
            // Locale-safe fallbacks
            // ---------------------------------------------------------
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                foreach (string value in values)
                {
                    if (LooksLikeInstanceId(value))
                    {
                        instanceId = value;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(classGuid))
            {
                foreach (string value in values)
                {
                    if (LooksLikeGuid(value))
                    {
                        classGuid = value;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                // With the Printer class filter, the first ordinary
                // value after the utility title is normally the device
                // description.
                foreach (string value in values)
                {
                    if (LooksLikeInstanceId(value) ||
                        LooksLikeGuid(value) ||
                        LooksLikeProblemCode(value) ||
                        LooksLikeStatus(value))
                    {
                        continue;
                    }

                    description = value;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                // Fallback is intentionally conservative.
                manufacturer = "";
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                className = "Printer";
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                status = "Disconnected";
            }

            if (string.IsNullOrWhiteSpace(description) &&
                string.IsNullOrWhiteSpace(instanceId))
            {
                return null;
            }

            return new GhostPrinterModel
            {
                Name =
                    string.IsNullOrWhiteSpace(description)
                        ? "Unknown Printer"
                        : description,

                DeviceId =
                    instanceId ?? "",

                Manufacturer =
                    manufacturer ?? "",

                ClassGuid =
                    classGuid ?? "",

                Status =
                    "Disconnected",

                ProblemCode =
                    problemCode,

                Description =
                    className ?? "Printer"
            };
        }

        private static bool LooksLikeInstanceId(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Typical instance IDs: USB\..., WSD\..., ROOT\..., LPTENUM\...
            return value.Contains('\\') &&
                   !LooksLikeGuid(value) &&
                   value.Length >= 5;
        }

        private static bool LooksLikeGuid(
            string value)
        {
            return Regex.IsMatch(
                value,
                @"^\{[0-9A-Fa-f]{8}-" +
                @"[0-9A-Fa-f]{4}-" +
                @"[0-9A-Fa-f]{4}-" +
                @"[0-9A-Fa-f]{4}-" +
                @"[0-9A-Fa-f]{12}\}$");
        }

        private static bool LooksLikeProblemCode(
            string value)
        {
            return Regex.IsMatch(
                value,
                @"^\d+\s*\(0x[0-9A-Fa-f]+\)");
        }

        private static bool LooksLikeStatus(
            string value)
        {
            return value.Equals(
                       "Started",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   value.Equals(
                       "Stopped",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   value.Equals(
                       "Problem",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   value.Equals(
                       "Disconnected",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   value.Equals(
                       "Disabled",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static uint ParseProblemCode(
            string value)
        {
            Match match =
                Regex.Match(
                    value ?? "",
                    @"^\s*(\d+)");

            if (match.Success &&
                uint.TryParse(
                    match.Groups[1].Value,
                    out uint result))
            {
                return result;
            }

            return 0;
        }
    }
}
