using GuardianEye.Server.Data;
using GuardianEye.Server.Models;
using GuardianEye.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GuardianEye.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StudentsController : ControllerBase
{
    private readonly GuardianEyeDbContext _db;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(GuardianEyeDbContext db, ILogger<StudentsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/students
    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> GetStudents(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? className = null)
    {
        try
        {
            var query = _db.Users
                .Where(u => u.Role == "Student")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(searchLower) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                    (u.StudentId != null && u.StudentId.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "active")
                    query = query.Where(u => u.IsActive);
                else if (status == "inactive")
                    query = query.Where(u => !u.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(className))
            {
                query = query.Where(u => u.Class == className);
            }

            var students = await query
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    Username = u.Username,
                    StudentId = u.StudentId,
                    Class = u.Class,
                    DeviceName = u.DeviceId,
                    SessionsUsedToday = u.SessionsUsedToday,
                    MaxDailySessions = u.MaxDailySessions,
                    SessionDurationMinutes = u.SessionDurationMinutes,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastActivityTime = u.LastActivityTime,
                    IsLoggedIn = u.IsLoggedIn
                })
                .OrderBy(u => u.Username)
                .ToListAsync();

            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students");
            return StatusCode(500, new { Success = false, Message = "Failed to retrieve students" });
        }
    }

    // GET: api/students/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null || user.Role != "Student")
                return NotFound(new { Success = false, Message = "Student not found" });

            var student = new StudentDto
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Username = user.Username,
                StudentId = user.StudentId,
                Class = user.Class,
                DeviceName = user.DeviceId,
                SessionsUsedToday = user.SessionsUsedToday,
                MaxDailySessions = user.MaxDailySessions,
                SessionDurationMinutes = user.SessionDurationMinutes,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastActivityTime = user.LastActivityTime,
                IsLoggedIn = user.IsLoggedIn
            };

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student {Id}", id);
            return StatusCode(500, new { Success = false, Message = "Failed to retrieve student" });
        }
    }

    // POST: api/students
    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.Username) || string.IsNullOrWhiteSpace(createDto.Password))
            {
                return BadRequest(new { Success = false, Message = "Username and password are required" });
            }

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == createDto.Username.Trim().ToLower());
            if (existingUser != null)
            {
                return BadRequest(new { Success = false, Message = "Username already exists" });
            }

            var user = new User
            {
                Username = createDto.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
                FullName = createDto.FullName,
                StudentId = createDto.StudentId,
                Class = createDto.Class,
                Role = "Student",
                MaxDailySessions = createDto.MaxDailySessions,
                SessionDurationMinutes = createDto.SessionDurationMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Created student: {Username}", user.Username);

            var studentDto = new StudentDto
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Username = user.Username,
                StudentId = user.StudentId,
                Class = user.Class,
                SessionsUsedToday = 0,
                MaxDailySessions = user.MaxDailySessions,
                SessionDurationMinutes = user.SessionDurationMinutes,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(new { Success = true, Data = studentDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return StatusCode(500, new { Success = false, Message = "Failed to create student" });
        }
    }

    // PUT: api/students/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto updateDto)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null || user.Role != "Student")
                return NotFound(new { Success = false, Message = "Student not found" });

            user.FullName = updateDto.FullName;
            user.StudentId = updateDto.StudentId;
            user.Class = updateDto.Class;
            user.MaxDailySessions = updateDto.MaxDailySessions;
            user.SessionDurationMinutes = updateDto.SessionDurationMinutes;
            user.IsActive = updateDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Updated student: {Id}", id);
            return Ok(new { Success = true, Message = "Student updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {Id}", id);
            return StatusCode(500, new { Success = false, Message = "Failed to update student" });
        }
    }

    // DELETE: api/students/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStudent(int id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null || user.Role != "Student")
                return NotFound(new { Success = false, Message = "Student not found" });

            // Delete associated data
            var sessions = await _db.ActiveSessions.Where(s => s.UserId == id).ToListAsync();
            _db.ActiveSessions.RemoveRange(sessions);

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted student: {Id}", id);
            return Ok(new { Success = true, Message = "Student deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {Id}", id);
            return StatusCode(500, new { Success = false, Message = "Failed to delete student" });
        }
    }

    // POST: api/students/{id}/reset-sessions
    [HttpPost("{id}/reset-sessions")]
    public async Task<ActionResult> ResetStudentSessions(int id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null || user.Role != "Student")
                return NotFound(new { Success = false, Message = "Student not found" });

            user.SessionsUsedToday = 0;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Reset sessions for student: {Id}", id);
            return Ok(new { Success = true, Message = "Sessions reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting sessions for student {Id}", id);
            return StatusCode(500, new { Success = false, Message = "Failed to reset sessions" });
        }
    }

    // POST: api/students/{id}/force-logout
    [HttpPost("{id}/force-logout")]
    public async Task<ActionResult> ForceLogoutStudent(int id)
    {
        try
        {
            var sessions = await _db.ActiveSessions
                .Where(s => s.UserId == id && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.IsLoggedIn = false;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Force logout for student: {Id}", id);
            return Ok(new { Success = true, Message = "Student logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force logout for student {Id}", id);
            return StatusCode(500, new { Success = false, Message = "Failed to logout student" });
        }
    }
}