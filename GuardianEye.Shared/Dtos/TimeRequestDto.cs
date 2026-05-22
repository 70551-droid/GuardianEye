using System;

namespace GuardianEye.Shared.Dtos
{
    public class TimeRequestDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentUsername { get; set; }
        public int RequestedMinutes { get; set; }
        public int? ApprovedMinutes { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByName { get; set; }
    }

    public class CreateTimeRequestDto
    {
        public int StudentId { get; set; }
        public int RequestedMinutes { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class TimeRequestApprovalDto
    {
        public int RequestId { get; set; }
        public bool IsApproved { get; set; }
        public int? ApprovedMinutes { get; set; }
    }
}