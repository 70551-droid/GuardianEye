using System.ComponentModel.DataAnnotations;

namespace GuardianEye.Shared.Dtos
{
    public class AdminLoginRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;
    }
}