using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ReportType { get; set; } = "Daily";

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        [Column(TypeName = "ntext")]
        public string? Data { get; set; }

        public int TotalStudents { get; set; }

        public int TotalSessions { get; set; }

        public int Violations { get; set; }

        public double AverageUsageMinutes { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public int? GeneratedByUserId { get; set; }

        [ForeignKey(nameof(GeneratedByUserId))]
        public User? GeneratedByUser { get; set; }
    }
}