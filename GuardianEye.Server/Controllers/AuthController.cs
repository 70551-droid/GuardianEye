using GuardianEye.Server.Data;
using GuardianEye.Server.Models;
using GuardianEye.Server.Services;
using GuardianEye.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GuardianEye.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GuardianEyeDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(GuardianEyeDbContext db, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _db = db;
        _jwtService = jwtService;
        _logger = logger;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(loginRequest.Username) ||
                string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            // Find user by username
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == loginRequest.Username.Trim().ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", loginRequest.Username);
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Account is disabled. Please contact an administrator."
                });
            }

            // Multi-PC Prevention: Deactivate any existing session for this user
            var existingSession = await _db.ActiveSessions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.IsActive);

            if (existingSession != null)
            {
                existingSession.IsActive = false;
                _logger.LogInformation("Deactivated previous session for user {UserId} (multi-PC prevention)", user.Id);
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Create new active session record
            var activeSession = new ActiveSession
            {
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow,
                IsActive = true
            };
            _db.ActiveSessions.Add(activeSession);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully. Session ID: {SessionId}",
                user.Username, activeSession.Id);

            return Ok(new LoginResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", loginRequest.Username);
            return StatusCode(500, new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login. Please try again."
            });
        }
    }

    // POST: api/auth/admin-login
    [HttpPost("admin-login")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequestDto adminLoginRequest)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(adminLoginRequest.Username) ||
                string.IsNullOrWhiteSpace(adminLoginRequest.Password))
            {
                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == adminLoginRequest.Username.Trim().ToLower()
                    && u.Role == "Admin");

            if (user == null || !BCrypt.Net.BCrypt.Verify(adminLoginRequest.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed admin login attempt for user: {Username}", adminLoginRequest.Username);
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid admin credentials"
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Admin account is disabled. Please contact the system owner."
                });
            }

            // Deactivate any existing admin session
            var existingSession = await _db.ActiveSessions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.IsActive);

            if (existingSession != null)
            {
                existingSession.IsActive = false;
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Create new active session
            var activeSession = new ActiveSession
            {
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow,
                IsActive = true
            };
            _db.ActiveSessions.Add(activeSession);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin {Username} logged in successfully", user.Username);

            return Ok(new LoginResponseDto
            {
                Success = true,
                Message = "Admin login successful",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin login for user {Username}", adminLoginRequest.Username);
            return StatusCode(500, new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during admin login."
            });
        }
    }

    // POST: api/auth/validate-session
    [HttpPost("validate-session")]
    public IActionResult ValidateSession([FromBody] SessionValidationDto validationRequest)
    {
        try
        {
            _logger.LogInformation("Session validation requested");

            return Ok(new SessionValidationDto
            {
                CanLogin = true,
                Reason = "Session validation passed",
                IsValid = true,
                Status = "Valid",
                SessionsUsedToday = validationRequest.SessionsUsedToday,
                MaxDailySessions = validationRequest.MaxDailySessions,
                RemainingSessions = Math.Max(0, validationRequest.MaxDailySessions - validationRequest.SessionsUsedToday)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session validation");
            return StatusCode(500, new SessionValidationDto
            {
                CanLogin = false,
                IsValid = false,
                Status = "Error",
                Reason = "Session validation failed due to server error"
            });
        }
    }

    // POST: api/auth/heartbeat
    // JWT-protected endpoint for session heartbeat validation
    [Authorize]
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Heartbeat received with invalid user claims");
                return Ok(new SessionValidationDto
                {
                    IsValid = false,
                    Status = "InvalidToken",
                    CanLogin = false,
                    Reason = "Invalid authentication token"
                });
            }

            var activeSession = await _db.ActiveSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (activeSession == null)
            {
                _logger.LogInformation("Heartbeat rejected: no active session for user {UserId}", userId);
                return Ok(new SessionValidationDto
                {
                    IsValid = false,
                    Status = "SessionNotFound",
                    CanLogin = false,
                    Reason = "Your session ended because another login was detected."
                });
            }

            // Multi-PC Prevention: Check if session was externally invalidated
            if (!activeSession.IsActive)
            {
                _logger.LogInformation("Heartbeat rejected: session was invalidated for user {UserId}", userId);
                return Ok(new SessionValidationDto
                {
                    IsValid = false,
                    Status = "SessionInvalidated",
                    CanLogin = false,
                    Reason = "Your session ended because another login was detected."
                });
            }

            // Update heartbeat timestamp
            activeSession.LastHeartbeat = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new SessionValidationDto
            {
                IsValid = true,
                Status = "Active",
                CanLogin = true,
                SessionsUsedToday = 0,
                MaxDailySessions = 0,
                RemainingSessions = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during heartbeat");
            return Ok(new SessionValidationDto
            {
                IsValid = false,
                Status = "Error",
                CanLogin = false,
                Reason = "Session validation error"
            });
        }
    }

    // POST: api/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { Success = false, Message = "Invalid user token" });
            }

            var activeSessions = await _db.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in activeSessions)
            {
                session.IsActive = false;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out. Deactivated {Count} session(s)", userId, activeSessions.Count);

            return Ok(new { Success = true, Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { Success = false, Message = "Logout failed" });
        }
    }
}
