using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("AppBlocks")]
    public class AppBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProcessName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DisplayName { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public bool AutoKill { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}