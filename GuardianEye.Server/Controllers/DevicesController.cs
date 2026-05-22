using GuardianEye.Server.Data;
using GuardianEye.Server.Models;
using GuardianEye.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuardianEye.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly GuardianEyeDbContext _db;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(GuardianEyeDbContext db, ILogger<DevicesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/devices/status
    [HttpGet("status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<DeviceStatusDto>>> GetDeviceStatus()
    {
        try
        {
            var sessions = await _db.ActiveSessions
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .Select(s => new DeviceStatusDto
                {
                    DeviceName = s.User != null ? s.User.DeviceId ?? "Unknown" : "Unknown",
                    UserName = s.User != null ? s.User.FullName ?? s.User.Username : string.Empty,
                    StudentId = s.User != null ? s.User.StudentId ?? string.Empty : string.Empty,
                    State = "In Session",
                    SessionRemaining = CalculateRemainingTime(s),
                    LastHeartbeat = s.LastHeartbeat.ToString("HH:mm:ss")
                })
                .ToListAsync();

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving device status");
            return StatusCode(500, new { Success = false, Message = "Failed to retrieve device status" });
        }
    }

    private string CalculateRemainingTime(ActiveSession session)
    {
        if (session.User == null) return "--:--";
        
        var duration = session.User.SessionDurationMinutes;
        var elapsed = DateTime.UtcNow - session.CreatedAt;
        var remaining = TimeSpan.FromMinutes(duration) - elapsed;
        
        if (remaining.TotalSeconds <= 0) return "00:00";
        
        return remaining.ToString(@"mm\:ss");
    }
}