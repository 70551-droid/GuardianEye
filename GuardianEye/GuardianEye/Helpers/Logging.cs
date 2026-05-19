using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace GuardianEye.Helpers
{
    public static class Logging
    {
        private static Logger? _logger;
        private static readonly object _lock = new();

        public static Logger GetLogger()
        {
            if (_logger != null) return _logger;
            lock (_lock)
            {
                if (_logger != null) return _logger;
                AppPaths.EnsureDirectories();
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .WriteTo.File(
                        Path.Combine(AppPaths.Logs, "guardianeye-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.Debug()
                    .CreateLogger();
                return _logger;
            }
        }

        public static void Info(string message) => GetLogger().Information(message);
        public static void Warn(string message) => GetLogger().Warning(message);
        public static void Error(string message, Exception? ex = null) => GetLogger().Error(ex, message);
        public static void Debug(string message) => GetLogger().Debug(message);
        public static void Fatal(string message, Exception? ex = null) => GetLogger().Fatal(ex, message);
    }
}