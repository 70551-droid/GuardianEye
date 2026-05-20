using System;
using System.ComponentModel.DataAnnotations;

namespace GuardianEye.Shared.Dtos
{
    public class SessionValidationDto
    {
        public bool IsValid { get; set; }
        public string? Status { get; set; }
        public bool CanLogin { get; set; }
        public string? Reason { get; set; }
        public int SessionsUsedToday { get; set; }
        public int MaxDailySessions { get; set; }
        public int RemainingSessions { get; set; }
        public DateTime? NextAvailableLogin { get; set; }
    }
}
