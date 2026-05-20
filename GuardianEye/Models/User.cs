using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Student";

        [MaxLength(50)]
        public string? StudentId { get; set; }

        [MaxLength(50)]
        public string? Class { get; set; }

        [MaxLength(100)]
        public string? DeviceId { get; set; }

        public int SessionsUsedToday { get; set; } = 0;

        public int MaxDailySessions { get; set; } = 2;

        public int SessionDurationMinutes { get; set; } = 15;

        public DateTime? LastLoginDate { get; set; }

        public DateTime? SessionEndTime { get; set; }

        public bool IsLoggedIn { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastActivityTime { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}