using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11.Services
{
    public static class DriverStoreService
    {
        public static List<DriverStoreModel> GetDrivers()
        {
            string pnputilPath = Path.Combine(
                Environment.SystemDirectory,
                "pnputil.exe");

            if (!File.Exists(pnputilPath))
            {
                throw new Exception(
                    "ไม่พบ pnputil.exe\n\n" +
                    "ตำแหน่งที่ตรวจสอบ:\n" +
                    pnputilPath);
            }

            string output;
            string errorOutput;
            int exitCode;

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = pnputilPath,
                    Arguments = "/enum-drivers",

                    UseShellExecute = false,
                    CreateNoWindow = true,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,

                    // รองรับข้อความจาก console ของ Windows
                    StandardOutputEncoding = Encoding.Default,
                    StandardErrorEncoding = Encoding.Default
                };

                process.Start();

                output = process.StandardOutput.ReadToEnd();
                errorOutput = process.StandardError.ReadToEnd();

                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
            {
                string message =
                    string.IsNullOrWhiteSpace(errorOutput)
                        ? output
                        : errorOutput;

                throw new Exception(
                    "ไม่สามารถอ่าน Windows Driver Store ได้\n\n" +
                    message.Trim());
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                return new List<DriverStoreModel>();
            }

            return ParseOutput(output);
        }

        // =========================================================
        // Parse PnPUtil output
        // =========================================================
        private static List<DriverStoreModel> ParseOutput(
            string output)
        {
            var drivers =
                new List<DriverStoreModel>();

            // ---------------------------------------------------------
            // แยกแต่ละ Driver Package ด้วยบรรทัดว่าง
            // ---------------------------------------------------------
            string normalized =
                output.Replace("\r\n", "\n")
                     .Replace('\r', '\n');

            string[] blocks =
                Regex.Split(
                    normalized,
                    @"\n\s*\n");

            foreach (string block in blocks)
            {
                DriverStoreModel? driver =
                    ParseBlock(block);

                if (driver == null)
                    continue;

                // กันข้อมูลซ้ำ
                bool duplicate =
                    drivers.Any(x =>
                        string.Equals(
                            x.PublishedName,
                            driver.PublishedName,
                            StringComparison.OrdinalIgnoreCase));

                if (!duplicate)
                {
                    drivers.Add(driver);
                }
            }

            // เรียงตาม oem number
            drivers =
                drivers
                    .OrderBy(
                        x => GetOemNumber(x.PublishedName))
                    .ThenBy(
                        x => x.PublishedName,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            return drivers;
        }

        // =========================================================
        // Parse หนึ่ง Driver Package
        // =========================================================
        private static DriverStoreModel? ParseBlock(
            string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return null;

            var values =
                new List<string>();

            string[] lines =
                block.Split(
                    '\n',
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // -------------------------------------------------
                // ตัดข้อมูลก่อน :
                // รองรับ label ที่เป็นภาษาไทย / อังกฤษ / ภาษาอื่น
                // -------------------------------------------------
                int colon =
                    line.IndexOf(':');

                if (colon < 0)
                    continue;

                string value =
                    line[(colon + 1)..].Trim();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }

            if (values.Count < 2)
                return null;

            // =====================================================
            // หา Published Name
            // เช่น oem12.inf
            // =====================================================
            int publishedIndex =
                -1;

            for (int i = 0; i < values.Count; i++)
            {
                if (Regex.IsMatch(
                    values[i],
                    @"^oem\d+\.inf$",
                    RegexOptions.IgnoreCase))
                {
                    publishedIndex = i;
                    break;
                }
            }

            if (publishedIndex < 0)
                return null;

            string publishedName =
                values[publishedIndex];

            // =====================================================
            // ค่าโดยปกติของ PnPUtil จะเรียงประมาณ:
            //
            // Published Name
            // Original Name
            // Provider Name
            // Class Name
            // Class GUID
            // Driver Version
            // Signer Name
            // =====================================================

            string originalName =
                GetValue(values, publishedIndex + 1);

            string providerName =
                GetValue(values, publishedIndex + 2);

            string className =
                GetValue(values, publishedIndex + 3);

            string classGuid =
                GetValue(values, publishedIndex + 4);

            string version =
                GetValue(values, publishedIndex + 5);

            string signerName =
                GetValue(values, publishedIndex + 6);

            // =====================================================
            // ตรวจสอบ Class GUID
            // =====================================================
            if (!IsGuidValue(classGuid))
            {
                // เผื่อมี field เพิ่ม/ขาด
                for (int i = publishedIndex + 1;
                     i < values.Count;
                     i++)
                {
                    if (IsGuidValue(values[i]))
                    {
                        classGuid = values[i];

                        int guidIndex = i;

                        if (string.IsNullOrWhiteSpace(providerName) &&
                            guidIndex > publishedIndex + 1)
                        {
                            providerName =
                                GetValue(
                                    values,
                                    publishedIndex + 2);
                        }

                        if (string.IsNullOrWhiteSpace(className) &&
                            guidIndex > publishedIndex + 1)
                        {
                            className =
                                GetValue(
                                    values,
                                    publishedIndex + 3);
                        }

                        break;
                    }
                }
            }

            // =====================================================
            // ถ้า version ไม่ตรงตำแหน่ง
            // ค้นหาค่าที่ดูเหมือน version/date
            // =====================================================
            if (!LooksLikeDriverVersion(version))
            {
                for (int i = publishedIndex + 1;
                     i < values.Count;
                     i++)
                {
                    if (LooksLikeDriverVersion(values[i]))
                    {
                        version = values[i];
                        break;
                    }
                }
            }

            // =====================================================
            // Signer
            // =====================================================
            if (string.IsNullOrWhiteSpace(signerName) &&
                values.Count > 0)
            {
                signerName =
                    values[^1];
            }

            return new DriverStoreModel
            {
                PublishedName = publishedName,
                OriginalName = originalName,
                ProviderName = providerName,
                ClassName = className,
                Version = version,
                Date = ExtractDate(version),
                SignerName = signerName,
                ClassGuid = classGuid
            };
        }

        // =========================================================
        // Get value safely
        // =========================================================
        private static string GetValue(
            List<string> values,
            int index)
        {
            if (index < 0 || index >= values.Count)
                return "";

            return values[index];
        }

        // =========================================================
        // Check GUID
        // =========================================================
        private static bool IsGuidValue(
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

        // =========================================================
        // Check Driver Version
        // =========================================================
        private static bool LooksLikeDriverVersion(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // ตัวอย่าง:
            //
            // 10.0.22621.1
            // 31.0.101.5590
            // 12/01/2025 10.0.22621.1
            //
            return Regex.IsMatch(
                value,
                @"\d+\.\d+\.\d+\.\d+");
        }

        // =========================================================
        // Extract Date
        // =========================================================
        private static string ExtractDate(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            // =====================================================
            // รูปแบบ 1:
            // 12/01/2025 10.0.22621.1
            // =====================================================
            Match dateMatch =
                Regex.Match(
                    value,
                    @"\b\d{1,2}/\d{1,2}/\d{4}\b");

            if (dateMatch.Success)
            {
                return dateMatch.Value;
            }

            // =====================================================
            // รูปแบบ 2:
            // พยายาม Parse DateTime
            // =====================================================
            if (DateTime.TryParse(
                value,
                out DateTime date))
            {
                return date.ToString("dd/MM/yyyy");
            }

            return "";
        }

        // =========================================================
        // OEM Number
        // =========================================================
        private static int GetOemNumber(
            string value)
        {
            Match match =
                Regex.Match(
                    value ?? "",
                    @"oem(\d+)\.inf",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
                return int.MaxValue;

            if (int.TryParse(
                match.Groups[1].Value,
                out int number))
            {
                return number;
            }

            return int.MaxValue;
        }
    }
}