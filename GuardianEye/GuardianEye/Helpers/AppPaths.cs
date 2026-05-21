using System;
using System.Collections.Generic;
using System.IO;

namespace GuardianEye.Helpers
{
    public static class AppPaths
    {
        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GuardianEye");

        static AppPaths()
        {
            if (!Directory.Exists(AppDataDir)) Directory.CreateDirectory(AppDataDir);
        }

        public static string AppData => AppDataDir;
        public static string Database => Path.Combine(AppDataDir, "guardianeye.db");
        public static string Logs => Path.Combine(AppDataDir, "Logs");
        public static string Screenshots => Path.Combine(AppDataDir, "Screenshots");
        public static string Wallpapers => Path.Combine(AppDataDir, "Wallpapers");
        public static string Config => Path.Combine(AppDataDir, "config.json");
        public static string CrashDumps => Path.Combine(AppDataDir, "Dumps");

        public static void EnsureDirectories()
        {
            foreach (var dir in new[] { Logs, Screenshots, Wallpapers, CrashDumps })
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
    }
}