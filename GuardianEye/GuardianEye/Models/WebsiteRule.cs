using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("WebsiteRules")]
    public class WebsiteRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string UrlPattern { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string RuleType { get; set; } = "Blacklist";

        public bool IsEnabled { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public User? CreatedByUser { get; set; }
    }
}