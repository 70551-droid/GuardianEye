using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("Screenshots")]
    public class Screenshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? Reason { get; set; }

        public long FileSizeBytes { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}