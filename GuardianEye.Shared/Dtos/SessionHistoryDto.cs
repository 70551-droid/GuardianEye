namespace GuardianEye.Shared.Dtos
{
    public class SessionHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public int SessionDurationMinutes { get; set; }
        public string EndReason { get; set; } = string.Empty;
        public bool WasExtended { get; set; }
    }
}