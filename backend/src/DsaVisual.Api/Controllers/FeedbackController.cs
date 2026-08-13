using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Phản hồi & báo lỗi — API_REFERENCE.md §4.15 (FR-7.4).
/// POST /feedback: 403 nếu chưa "Đánh dấu đã học" bài đó (v2.9).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/feedback")]
[Authorize]
public class FeedbackController(
    AppDbContext db,
    IValidator<FeedbackRequest> feedbackValidator,
    IHtmlSanitizer htmlSanitizer) : ApiControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<ActionResult<FeedbackSummaryDto>> GetSummary([FromQuery] int lessonId, CancellationToken ct)
    {
        var lessonExists = await _db.Lessons.AsNoTracking()
            .AnyAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (!lessonExists)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Bài học không tồn tại", field = (string?)null, details = Array.Empty<string>() } });
        }

        // perf#21: đẩy aggregate xuống SQL (COUNT + AVG trong GROUP BY) — trước đây tải TOÀN BỘ
        // ContentFeedback của lesson về memory rồi Average/Count (lesson đông feedback = tốn RAM/network).
        // Math.Round tính trong memory (1 dòng kết quả — không cần dịch xuống SQL).
        var aggregate = await _db.ContentFeedback.AsNoTracking()
            .Where(f => f.LessonId == lessonId)
            .GroupBy(f => f.LessonId)
            .Select(g => new { Count = g.Count(), Avg = g.Average(f => f.Rating) })
            .FirstOrDefaultAsync(ct);

        var count = aggregate?.Count ?? 0;
        return Ok(new FeedbackSummaryDto
        {
            LessonId = lessonId,
            AvgRating = count > 0 ? Math.Round(aggregate!.Avg, 1) : 0,
            Count = count
        });
    }

    [HttpPost]
    public async Task<ActionResult> Submit([FromBody] FeedbackRequest request, CancellationToken ct)
    {
        // Finding security#12: FeedbackRequestValidator (đăng ký từ trước nhưng controller không dùng —
        // validator "chết"); wire vào đây để thay check tay rải rác.
        var invalid = await ValidateRequestAsync(feedbackValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var lessonExists = await _db.Lessons.AsNoTracking()
            .AnyAsync(l => l.Id == request.LessonId && l.DeletedAt == null, ct);
        if (!lessonExists)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Bài học không tồn tại", field = (string?)null, details = Array.Empty<string>() } });
        }

        // 403 nếu chưa "Đánh dấu đã học" bài đó (v2.9 — API_REFERENCE §4.15)
        var viewed = await _db.UserProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == CurrentUserId() && p.LessonId == request.LessonId && p.Viewed, ct);
        if (!viewed)
        {
            return StatusCode(403, new { error = new { code = "FORBIDDEN", message = "Bạn cần học bài này trước khi đánh giá", field = (string?)null, details = Array.Empty<string>() } });
        }

        // Finding security#8: sanitize Comment trước khi lưu (chống stored XSS).
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : htmlSanitizer.Sanitize(request.Comment);

        var now = DateTime.UtcNow;
        var feedback = await _db.ContentFeedback
            .FirstOrDefaultAsync(f => f.UserId == CurrentUserId() && f.LessonId == request.LessonId, ct);
        if (feedback is null)
        {
            feedback = new ContentFeedback
            {
                UserId = CurrentUserId(),
                LessonId = request.LessonId,
                Rating = request.Rating,
                Comment = comment,
                CreatedAt = now
            };
            _db.ContentFeedback.Add(feedback);
        }
        else
        {
            feedback.Rating = request.Rating;
            feedback.Comment = comment;
            feedback.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { rating = feedback.Rating, lessonId = request.LessonId });
    }
}

/// <summary>Báo lỗi — API_REFERENCE.md §4.15.</summary>
[ApiVersion("1.0")]
[Route("api/v1/bug-reports")]
[Authorize]
public class BugReportsController(
    AppDbContext db,
    IValidator<BugReportRequest> validator,
    IHtmlSanitizer htmlSanitizer) : ApiControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpPost]
    public async Task<ActionResult<BugReportDto>> Create([FromBody] BugReportRequest request, CancellationToken ct)
    {
        // Finding security#12: validator thay check tay (Description bắt buộc, ≤ 2000).
        var invalid = await ValidateRequestAsync(validator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        // Finding security#8: sanitize Description + Context trước khi lưu — bug report hiển thị
        // cho Admin (GetList/UpdateStatus trả về) → chống stored XSS vào màn hình admin.
        var report = new BugReport
        {
            UserId = CurrentUserId(),
            Description = htmlSanitizer.Sanitize(request.Description.Trim()),
            ContextJson = string.IsNullOrWhiteSpace(request.Context) ? null : htmlSanitizer.Sanitize(request.Context),
            Status = BugReportStatus.New,
            CreatedAt = DateTime.UtcNow
        };
        _db.BugReports.Add(report);
        await _db.SaveChangesAsync(ct);

        return StatusCode(201, ToDto(report));
    }

    private static BugReportDto ToDto(BugReport report) => new()
    {
        Id = report.Id,
        UserId = report.UserId,
        Description = report.Description,
        Context = report.ContextJson,
        Status = report.Status.ToString(),
        CreatedAt = report.CreatedAt,
        ResolvedAt = report.ResolvedAt
    };
}

/// <summary>Quản trị báo lỗi — API_REFERENCE.md §4.15 (Admin).</summary>
[ApiVersion("1.0")]
[Route("api/v1/admin/bug-reports")]
[Authorize(Roles = "ADMIN")]
public class AdminBugReportsController(AppDbContext db) : ApiControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<ActionResult<List<BugReportDto>>> GetList([FromQuery] string? status, CancellationToken ct)
    {
        var query = _db.BugReports.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BugReportStatus>(status, true, out var statusFilter))
        {
            query = query.Where(b => b.Status == statusFilter);
        }

        var reports = await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BugReportDto
            {
                Id = b.Id,
                UserId = b.UserId,
                Description = b.Description,
                Context = b.ContextJson,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt,
                ResolvedAt = b.ResolvedAt
            })
            .ToListAsync(ct);
        return Ok(reports);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BugReportDto>> UpdateStatus([FromRoute] int id, [FromBody] BugReportUpdateRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<BugReportStatus>(request.Status, true, out var status))
        {
            return BadRequest(new { error = new { code = "VALIDATION_FAILED", message = "Trạng thái không hợp lệ (NEW/PROCESSING/RESOLVED/CLOSED)", field = "status", details = Array.Empty<string>() } });
        }

        var report = await _db.BugReports.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (report is null)
        {
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Báo lỗi không tồn tại", field = (string?)null, details = Array.Empty<string>() } });
        }

        report.Status = status;
        report.AssigneeId = CurrentUserId();
        report.ResolvedAt = status is BugReportStatus.Resolved or BugReportStatus.Closed ? DateTime.UtcNow : null;
        await _db.SaveChangesAsync(ct);

        return Ok(new BugReportDto
        {
            Id = report.Id,
            UserId = report.UserId,
            Description = report.Description,
            Context = report.ContextJson,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            ResolvedAt = report.ResolvedAt
        });
    }
}
