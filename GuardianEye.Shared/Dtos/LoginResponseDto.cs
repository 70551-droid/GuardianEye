using System.ComponentModel.DataAnnotations;

namespace GuardianEye.Shared.Dtos
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserDto? User { get; set; }
        public string? Token { get; set; } // For future JWT token
    }
}