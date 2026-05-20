using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Models
{
    [Table("Devices")]
    public class Device
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CpuId { get; set; }

        [MaxLength(200)]
        public string? MotherboardId { get; set; }

        [MaxLength(200)]
        public string? DiskId { get; set; }

        [MaxLength(500)]
        public string? HardwareHash { get; set; }

        public bool IsAuthorized { get; set; } = false;

        public int? AssignedUserId { get; set; }

        [ForeignKey(nameof(AssignedUserId))]
        public User? AssignedUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSeen { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";
    }
}