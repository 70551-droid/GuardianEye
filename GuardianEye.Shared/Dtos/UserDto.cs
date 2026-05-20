using System;
using System.Collections.Generic;

namespace GuardianEye.Shared.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Class { get; set; }
        public string? DeviceId { get; set; }
        public int SessionsUsedToday { get; set; }
        public int MaxDailySessions { get; set; }
        public int SessionDurationMinutes { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastActivityTime { get; set; }
    }
}