using GuardianEye.Server.Data;
using GuardianEye.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuardianEye.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SessionHistoriesController : ControllerBase
{
    private readonly GuardianEyeDbContext _db;
    private readonly ILogger<SessionHistoriesController> _logger;

    public SessionHistoriesController(GuardianEyeDbContext db, ILogger<SessionHistoriesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/sessionhistories
    [HttpGet]
    public async Task<ActionResult<List<SessionHistoryDto>>> GetSessionHistories(
        [FromQuery] int? studentId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _db.SessionHistories
                .Include(sh => sh.User)
                .AsQueryable();

            if (studentId.HasValue)
            {
                query = query.Where(sh => sh.UserId == studentId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(sh => sh.LoginTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(sh => sh.LoginTime <= toDate.Value);
            }

            var histories = await query
                .Select(sh => new SessionHistoryDto
                {
                    Id = sh.Id,
                    UserId = sh.UserId,
                    UserName = sh.User != null ? sh.User.FullName ?? sh.User.Username : string.Empty,
                    DeviceName = sh.DeviceName,
                    LoginTime = sh.LoginTime,
                    LogoutTime = sh.LogoutTime,
                    SessionDurationMinutes = sh.SessionDurationMinutes,
                    EndReason = sh.EndReason,
                    WasExtended = sh.WasExtended
                })
                .OrderByDescending(sh => sh.LoginTime)
                .ToListAsync();

            return Ok(histories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session histories");
            return StatusCode(500, new { Success = false, Message = "Failed to retrieve session histories" });
        }
    }
}