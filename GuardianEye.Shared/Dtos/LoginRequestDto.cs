using System.ComponentModel.DataAnnotations;

namespace GuardianEye.Shared.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public string DeviceName { get; set; } = string.Empty;
    }
}