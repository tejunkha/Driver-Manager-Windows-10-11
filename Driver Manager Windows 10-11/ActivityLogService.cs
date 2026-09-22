using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Driver_Manager_Windows_10_11
{
    /// <summary>
    /// One activity log entry.
    /// </summary>
    public sealed class ActivityLogEntry
    {
        public string Time { get; set; } = "";
        public string Action { get; set; } = "";
        public string Category { get; set; } = "";
        public string Item { get; set; } = "";
        public string Result { get; set; } = "";
        public string Details { get; set; } = "";
        public string User { get; set; } = "";
        public string Mode { get; set; } = "";
    }

    /// <summary>
    /// Stores activity history beside the published application.
    /// </summary>
    internal static class ActivityLogService
    {
        private static readonly object SyncRoot = new object();

        private static readonly string LogDirectory =
            Path.Combine(AppContext.BaseDirectory, "Logs");

        private static readonly string LogFile =
            Path.Combine(LogDirectory, "activity.jsonl");

        /// <summary>
        /// Full path to the activity log file.
        /// </summary>
        public static string FilePath => LogFile;

        /// <summary>
        /// Main API used by MainForm.
        /// </summary>
        public static void Log(
            string action,
            string category,
            string item,
            string result,
            string details = "")
        {
            try
            {
                EnsureLogDirectory();

                ActivityLogEntry entry = new ActivityLogEntry
                {
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Action = action ?? "",
                    Category = category ?? "",
                    Item = item ?? "",
                    Result = result ?? "",
                    Details = details ?? "",
                    User = Environment.UserName,
                    Mode = IsAdministrator()
                        ? "Administrator"
                        : "Standard User"
                };

                string json = JsonSerializer.Serialize(entry);

                lock (SyncRoot)
                {
                    File.AppendAllText(
                        LogFile,
                        json + Environment.NewLine);
                }
            }
            catch
            {
                // Logging must never crash or interrupt the main application.
            }
        }

        /// <summary>
        /// Compatibility API for code that may call Write().
        /// </summary>
        public static void Write(
            string action,
            string category,
            string item,
            string result,
            string details = "")
        {
            Log(action, category, item, result, details);
        }

        /// <summary>
        /// Returns newest entries first.
        /// </summary>
        public static List<ActivityLogEntry> GetEntries(
            int maxEntries = 1000)
        {
            List<ActivityLogEntry> entries =
                new List<ActivityLogEntry>();

            try
            {
                if (maxEntries <= 0 || !File.Exists(LogFile))
                    return entries;

                string[] lines;

                lock (SyncRoot)
                {
                    lines = File.ReadAllLines(LogFile);
                }

                foreach (string line in lines.Reverse().Take(maxEntries))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        ActivityLogEntry? entry =
                            JsonSerializer.Deserialize<ActivityLogEntry>(line);

                        if (entry != null)
                            entries.Add(entry);
                    }
                    catch
                    {
                        // Ignore malformed lines and continue.
                    }
                }
            }
            catch
            {
                // Return what could be read.
            }

            return entries;
        }

        /// <summary>
        /// Compatibility API returning raw JSON lines.
        /// </summary>
        public static List<string> ReadAll()
        {
            try
            {
                if (!File.Exists(LogFile))
                    return new List<string>();

                lock (SyncRoot)
                {
                    return File.ReadAllLines(LogFile).ToList();
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Deletes the current log file.
        /// </summary>
        public static void Clear()
        {
            try
            {
                lock (SyncRoot)
                {
                    if (File.Exists(LogFile))
                        File.Delete(LogFile);
                }
            }
            catch
            {
                // Ignore cleanup errors.
            }
        }

        /// <summary>
        /// Makes sure the Logs folder exists.
        /// </summary>
        private static void EnsureLogDirectory()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        private static bool IsAdministrator()
        {
            try
            {
                using System.Security.Principal.WindowsIdentity identity =
                    System.Security.Principal.WindowsIdentity.GetCurrent();

                System.Security.Principal.WindowsPrincipal principal =
                    new System.Security.Principal.WindowsPrincipal(identity);

                return principal.IsInRole(
                    System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
