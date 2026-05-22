namespace GuardianEye.Shared.Dtos
{
    public class DeviceStatusDto
    {
        public string DeviceName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StateColor { get; set; } = "#FF6B6B";
        public string SessionRemaining { get; set; } = "--:--";
        public string LastHeartbeat { get; set; } = string.Empty;
    }
}