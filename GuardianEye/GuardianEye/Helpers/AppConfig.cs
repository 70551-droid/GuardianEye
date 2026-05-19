using System;
using System.IO;
using Newtonsoft.Json;

namespace GuardianEye.Helpers
{
    public class AppConfig
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GuardianEye", "config.json");

        public int DefaultSessionDurationMinutes { get; set; } = 15;
        public int DefaultMaxDailySessions { get; set; } = 2;
        public int InactivityTimeoutMinutes { get; set; } = 5;
        public int WarningTimeMinutes { get; set; } = 5;
        public bool EnableWebsiteFiltering { get; set; } = true;
        public bool EnableAppBlocking { get; set; } = true;
        public bool EnableScreenshots { get; set; } = true;
        public bool AutoStartWithWindows { get; set; } = true;
        public string DatabasePath { get; set; } = string.Empty;
        public string ScreenshotPath { get; set; } = string.Empty;
        public string LogPath { get; set; } = string.Empty;
        public string WallpaperPath { get; set; } = string.Empty;
        public bool LockScreenBlurEnabled { get; set; } = true;
        public double LockScreenBlurRadius { get; set; } = 20.0;
        public string Theme { get; set; } = "Dark";

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch { }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        public string GetDatabasePath()
        {
            if (!string.IsNullOrEmpty(DatabasePath)) return DatabasePath;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GuardianEye", "guardianeye.db");
        }

        public string GetScreenshotPath()
        {
            if (!string.IsNullOrEmpty(ScreenshotPath)) return ScreenshotPath;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GuardianEye", "Screenshots");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public string GetLogPath()
        {
            if (!string.IsNullOrEmpty(LogPath)) return LogPath;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GuardianEye", "Logs");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public string GetWallpaperPath()
        {
            if (!string.IsNullOrEmpty(WallpaperPath)) return WallpaperPath;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GuardianEye", "Wallpapers");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}