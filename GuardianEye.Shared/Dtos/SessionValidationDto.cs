using System;

namespace GuardianEye.Shared.Dtos
{
    public class SessionValidationDto
    {
        public bool CanLogin { get; set; }
        public string? Reason { get; set; }
        public int SessionsUsedToday { get; set; }
        public int MaxDailySessions { get; set; }
        public int RemainingSessions { get; set; }
        public DateTime? NextAvailableLogin { get; set; }
    }
}