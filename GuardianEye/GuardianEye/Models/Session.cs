using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("Sessions")]
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        public TimeSpan? Duration { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        public int SessionNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}