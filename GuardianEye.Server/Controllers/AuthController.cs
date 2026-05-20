using GuardianEye.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GuardianEye.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto loginRequest)
        {
            // For now, return a mock successful response for student
            // In a real implementation, this would validate credentials against a database
            var response = new LoginResponseDto
            {
                Success = true,
                Message = "Login successful",
                User = new UserDto
                {
                    Id = 1,
                    FullName = "Test Student",
                    Username = loginRequest.Username,
                    Role = "Student",
                    StudentId = "STU001",
                    Class = "Grade 10",
                    DeviceId = loginRequest.DeviceId,
                    SessionsUsedToday = 0,
                    MaxDailySessions = 2,
                    SessionDurationMinutes = 15,
                    IsLoggedIn = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            return Ok(response);
        }

        // POST: api/auth/admin-login
        [HttpPost("admin-login")]
        public IActionResult AdminLogin([FromBody] AdminLoginRequestDto adminLoginRequest)
        {
            // Mock admin login - in reality, validate against database
            if (adminLoginRequest.Username == "admin" && adminLoginRequest.Password == "admin")
            {
                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Admin login successful",
                    User = new UserDto
                    {
                        Id = 2,
                        FullName = "Administrator",
                        Username = "admin",
                        Role = "Admin",
                        StudentId = null,
                        Class = null,
                        DeviceId = null,
                        SessionsUsedToday = 0,
                        MaxDailySessions = 2,
                        SessionDurationMinutes = 15,
                        IsLoggedIn = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                return Ok(response);
            }

            return Unauthorized(new LoginResponseDto { Success = false, Message = "Invalid admin credentials" });
        }

        // POST: api/auth/validate-session
        [HttpPost("validate-session")]
        public IActionResult ValidateSession([FromBody] SessionValidationDto validationRequest)
        {
            // For now, return a mock validation response
            // In a real implementation, this would check global session limits
            var response = new SessionValidationDto
            {
                CanLogin = true,
                Reason = "Session validation passed",
                SessionsUsedToday = validationRequest.SessionsUsedToday,
                MaxDailySessions = validationRequest.MaxDailySessions,
                RemainingSessions = Math.Max(0, validationRequest.MaxDailySessions - validationRequest.SessionsUsedToday),
                NextAvailableLogin = null
            };

            return Ok(response);
        }
    }
}