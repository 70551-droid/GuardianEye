using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        [MaxLength(200)]
        public string ActivityType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? ApplicationName { get; set; }

        [MaxLength(500)]
        public string? WindowTitle { get; set; }

        [MaxLength(500)]
        public string? Url { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public TimeSpan? Duration { get; set; }

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [MaxLength(50)]
        public string? Severity { get; set; }
    }
}