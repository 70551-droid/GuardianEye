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
public class TimeRequestsController : ControllerBase
{
    private readonly GuardianEyeDbContext _db;
    private readonly ILogger<TimeRequestsController> _logger;

    public TimeRequestsController(GuardianEyeDbContext db, ILogger<TimeRequestsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/timetrequests (Admin only - pending requests)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TimeRequestDto>>> GetTimeRequests(
        [FromQuery] string? status = null,
        [FromQuery] int? studentId = null)
    {
        try
        {
            var query = _db.TimeRequests
                .Include(tr => tr.Student)
                .Include(tr => tr.ApprovedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(tr => tr.Status.ToLower() == status.ToLower());
            }

            if (studentId.HasValue)
            {
                query = query.Where(tr => tr.StudentId == studentId.Value);
            }

            var requests = await query
                .Select(tr => new TimeRequestDto
                {
                    Id = tr.Id,
                    StudentId = tr.StudentId,
                    StudentName = tr.Student != null ? tr.Student.FullName ?? tr.Student.Username : string.Empty,
                    StudentUsername = tr.Student != null ? tr.Student.Username : string.Empty,
                    RequestedMinutes = tr.RequestedMinutes,
                    ApprovedMinutes = tr.ApprovedMinutes,
                    Reason = tr.Reason,
                    Status = tr.Status,
                    RequestedAt = tr.RequestedAt,
                    ApprovedAt = tr.ApprovedAt,
                    ApprovedByName = tr.ApprovedBy != null ? tr.ApprovedBy.FullName ?? tr.ApprovedBy.Username : null
                })
                .OrderByDescending(tr => tr.RequestedAt)
                .ToListAsync();

            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time requests");
            return StatusCode(500, new { Success = false, Message = "Failed to retrieve time requests" });
        }
    }

    // POST: api/timetrequests (Student creates request)
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TimeRequestDto>> CreateTimeRequest([FromBody] CreateTimeRequestDto createDto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid user token" });
            }

            var timeRequest = new TimeRequest
            {
                StudentId = createDto.StudentId,
                RequestedMinutes = createDto.RequestedMinutes,
                Reason = createDto.Reason,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _db.TimeRequests.Add(timeRequest);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Time request created for student {StudentId}", createDto.StudentId);

            var requestDto = new TimeRequestDto
            {
                Id = timeRequest.Id,
                StudentId = timeRequest.StudentId,
                RequestedMinutes = timeRequest.RequestedMinutes,
                Reason = timeRequest.Reason,
                Status = timeRequest.Status,
                RequestedAt = timeRequest.RequestedAt
            };

            return Ok(new { Success = true, Data = requestDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating time request");
            return StatusCode(500, new { Success = false, Message = "Failed to create time request" });
        }
    }

    // POST: api/timetrequests/approve (Admin approves/denies)
    [HttpPost("approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ApproveTimeRequest([FromBody] TimeRequestApprovalDto approvalDto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var adminId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid user token" });
            }

            var request = await _db.TimeRequests.FindAsync(approvalDto.RequestId);
            if (request == null)
                return NotFound(new { Success = false, Message = "Request not found" });

            if (approvalDto.IsApproved)
            {
                request.Status = "Approved";
                request.ApprovedMinutes = approvalDto.ApprovedMinutes ?? request.RequestedMinutes;
                request.ApprovedAt = DateTime.UtcNow;
                request.ApprovedById = adminId;
            }
            else
            {
                request.Status = "Denied";
                request.ApprovedAt = DateTime.UtcNow;
                request.ApprovedById = adminId;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Time request {Id} {Status}", request.Id, request.Status);
            return Ok(new { Success = true, Message = $"Request {request.Status.ToLower()}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving time request");
            return StatusCode(500, new { Success = false, Message = "Failed to process approval" });
        }
    }
}