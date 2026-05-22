using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuardianEye.Server.Models;

[Table("SessionHistories")]
public class SessionHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DeviceName { get; set; } = string.Empty;

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public int SessionDurationMinutes { get; set; }

    [MaxLength(20)]
    public string EndReason { get; set; } = "Normal";

    public bool WasExtended { get; set; } = false;

    public int? TimeRequestId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(TimeRequestId))]
    public TimeRequest? TimeRequest { get; set; }
}