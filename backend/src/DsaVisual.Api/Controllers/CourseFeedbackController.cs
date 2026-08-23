using System.Text.Json;
using Asp.Versioning;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Ý kiến học viên → giảng viên theo khóa học (lộ trình) — tương tác 2 chiều:
/// HV gửi ý kiến, GV đọc/trả lời/cập nhật trạng thái (pattern BugReport §4.15).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/courses")]
[Authorize]
public class CourseFeedbackController(AppDbContext db) : ApiControllerBase
{
    private readonly AppDbContext _db = db;

    public sealed class CourseFeedbackRequest
    {
        public int CourseId { get; set; }
        public string Type { get; set; } = "Suggestion";   // Suggestion | Bug | Request
        public string Content { get; set; } = string.Empty;
    }

    public sealed class CourseFeedbackDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReplyText { get; set; }
        public string? RepliedByName { get; set; }
        public DateTime? RepliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>HV gửi ý kiến về khóa học (không cần đã đăng ký — lộ trình mở).</summary>
    [HttpPost("feedback")]
    public async Task<ActionResult<CourseFeedbackDto>> Submit([FromBody] CourseFeedbackRequest request, CancellationToken ct)
    {
        var course = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.CourseId && p.IsActive, ct);
        if (course is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var type = ParseType(request.Type);
        var content = request.Content?.Trim() ?? string.Empty;
        if (content.Length is < 1 or > 1000)
        {
            return BadRequest(new { message = "Nội dung ý kiến phải từ 1 đến 1000 ký tự." });
        }

        var now = DateTime.UtcNow;
        var feedback = new CourseFeedback
        {
            CourseId = course.Id,
            UserId = CurrentUserId(),
            Type = type,
            Content = content,
            Status = CourseFeedbackStatus.New,
            CreatedAt = now
        };
        _db.CourseFeedback.Add(feedback);
        await _db.SaveChangesAsync(ct);

        return StatusCode(201, await ToDtoAsync(feedback, ct));
    }

    /// <summary>HV xem danh sách ý kiến của chính mình (kèm câu trả lời của GV).</summary>
    [HttpGet("feedback/mine")]
    public async Task<ActionResult<List<CourseFeedbackDto>>> GetMine([FromQuery] int? courseId, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var query = _db.CourseFeedback.AsNoTracking()
            .Where(f => f.UserId == userId);
        if (courseId is { } id)
        {
            query = query.Where(f => f.CourseId == id);
        }

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        var result = new List<CourseFeedbackDto>();
        foreach (var item in items)
        {
            result.Add(await ToDtoAsync(item, ct));
        }
        return Ok(result);
    }

    /// <summary>GV xem tất cả ý kiến của một khóa (có thể lọc theo trạng thái).</summary>
    [HttpGet("feedback/all")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<List<CourseFeedbackDto>>> GetAll([FromQuery] int courseId, [FromQuery] string? status, CancellationToken ct)
    {
        var course = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == courseId && p.IsActive, ct);
        if (course is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var query = _db.CourseFeedback.AsNoTracking().Where(f => f.CourseId == courseId);
        if (!string.IsNullOrWhiteSpace(status) && TryParseStatus(status, out var statusFilter))
        {
            query = query.Where(f => f.Status == statusFilter);
        }

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        var result = new List<CourseFeedbackDto>();
        foreach (var item in items)
        {
            result.Add(await ToDtoAsync(item, ct));
        }
        return Ok(result);
    }

    /// <summary>GV xem tất cả ý kiến của các khóa mình quản lý (hoặc admin xem tất cả).</summary>
    [HttpGet("feedback/for-teacher")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<List<CourseFeedbackDto>>> GetTeacherFeedback(
        [FromQuery] int? courseId,
        [FromQuery] string? status,
        [FromQuery] string? type,
        CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();

        var query = _db.CourseFeedback.AsNoTracking();

        if (role == "TEACHER")
        {
            var myCourseIds = await _db.LearningPaths.AsNoTracking()
                .Where(p => p.CreatedBy == userId || p.AuthorId == userId)
                .Select(p => p.Id)
                .ToListAsync(ct);
            query = query.Where(f => myCourseIds.Contains(f.CourseId));
        }

        if (courseId is { } cId)
        {
            query = query.Where(f => f.CourseId == cId);
        }

        if (!string.IsNullOrWhiteSpace(status) && TryParseStatus(status, out var statusFilter))
        {
            query = query.Where(f => f.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var parsedType = ParseType(type);
            query = query.Where(f => f.Type == parsedType);
        }

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        var result = new List<CourseFeedbackDto>();
        foreach (var item in items)
        {
            result.Add(await ToDtoAsync(item, ct));
        }
        return Ok(result);
    }

    public sealed class CourseFeedbackReplyRequest
    {
        public string? Status { get; set; }
        public string? ReplyText { get; set; }
    }

    /// <summary>GV trả lời ý kiến + cập nhật trạng thái (New → Read/Resolved).</summary>
    [HttpPut("feedback/{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<CourseFeedbackDto>> Reply(int id, [FromBody] CourseFeedbackReplyRequest request, CancellationToken ct)
    {
        var feedback = await _db.CourseFeedback
            .FirstOrDefaultAsync(f => f.Id == id, ct);
        if (feedback is null)
        {
            return NotFound(new { message = "Ý kiến không tồn tại." });
        }

        var replyText = request.ReplyText?.Trim();
        if (replyText is { Length: > 2000 })
        {
            return BadRequest(new { message = "Câu trả lời tối đa 2000 ký tự." });
        }

        var now = DateTime.UtcNow;
        if (replyText is not null)
        {
            feedback.ReplyText = replyText.Length == 0 ? null : replyText;
            feedback.RepliedById = CurrentUserId();
            feedback.RepliedAt = now;
            if (request.Status is null)
            {
                feedback.Status = CourseFeedbackStatus.Resolved;
            }
        }

        if (request.Status is not null)
        {
            if (!TryParseStatus(request.Status, out var status))
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ (New | Read | Resolved)." });
            }
            feedback.Status = status;
        }

        feedback.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        return Ok(await ToDtoAsync(feedback, ct));
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task<CourseFeedbackDto> ToDtoAsync(CourseFeedback feedback, CancellationToken ct)
    {
        var courseTitle = string.Empty;
        var course = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == feedback.CourseId, ct);
        if (course is not null)
        {
            courseTitle = course.Title;
        }

        var userName = string.Empty;
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == feedback.UserId, ct);
        if (user is not null)
        {
            userName = user.DisplayName;
        }

        var repliedByName = (string?)null;
        if (feedback.RepliedById is { } repliedById)
        {
            var replier = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == repliedById, ct);
            if (replier is not null)
            {
                repliedByName = replier.DisplayName;
            }
        }

        return new CourseFeedbackDto
        {
            Id = feedback.Id,
            CourseId = feedback.CourseId,
            CourseTitle = courseTitle,
            UserId = feedback.UserId,
            UserName = userName,
            Type = feedback.Type.ToString(),
            Content = feedback.Content,
            Status = feedback.Status.ToString(),
            ReplyText = feedback.ReplyText,
            RepliedByName = repliedByName,
            RepliedAt = feedback.RepliedAt,
            CreatedAt = feedback.CreatedAt
        };
    }

    private static CourseFeedbackType ParseType(string type) =>
        Enum.TryParse<CourseFeedbackType>(type, true, out var parsed)
            ? parsed
            : CourseFeedbackType.Suggestion;

    private static bool TryParseStatus(string status, out CourseFeedbackStatus parsed) =>
        Enum.TryParse(status, true, out parsed);
}
