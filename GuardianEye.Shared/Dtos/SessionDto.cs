using System;

namespace GuardianEye.Shared.Dtos
{
    public class SessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public int SessionNumber { get; set; }
        public string? Notes { get; set; }
    }
}