using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Server.Models;

[Table("ActiveSessions")]
public class ActiveSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
