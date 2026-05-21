using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace GuardianEye.Helpers
{
    public static class HardwareInfo
    {
        public static string GetMachineName() => Environment.MachineName;

        public static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                    return obj["ProcessorId"]?.ToString() ?? "Unknown";
            }
            catch { }
            return "Unknown";
        }

        public static string GetMotherboardId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var obj in searcher.Get())
                    return obj["SerialNumber"]?.ToString() ?? "Unknown";
            }
            catch { }
            return "Unknown";
        }

        public static string GetDiskId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index=0");
                foreach (var obj in searcher.Get())
                    return obj["SerialNumber"]?.ToString()?.Trim() ?? "Unknown";
            }
            catch { }
            return "Unknown";
        }

        public static string GetHardwareHash()
        {
            string raw = $"{GetCpuId()}|{GetMotherboardId()}|{GetDiskId()}";
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static string GetDeviceIdentifier()
        {
            return $"{GetMachineName()}_{GetHardwareHash()[..16]}";
        }
    }
}