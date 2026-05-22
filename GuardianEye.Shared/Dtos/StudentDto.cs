using System;

namespace GuardianEye.Shared.Dtos
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Class { get; set; }
        public string? DeviceName { get; set; }
        public int SessionsUsedToday { get; set; }
        public int MaxDailySessions { get; set; }
        public int SessionDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public bool IsLoggedIn { get; set; }
        public string? CurrentDevice { get; set; }
    }

    public class CreateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Class { get; set; }
        public int MaxDailySessions { get; set; } = 2;
        public int SessionDurationMinutes { get; set; } = 15;
    }

    public class UpdateStudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Class { get; set; }
        public int MaxDailySessions { get; set; }
        public int SessionDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}