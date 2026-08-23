using Asp.Versioning;
using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
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

/// <summary>Cá nhân mở rộng — API_REFERENCE.md §4.12: ghi chú bài học + huy hiệu.</summary>
[ApiVersion("1.0")]
[Route("api/v1/me")]
[Authorize]
public class MeController(
    AppDbContext db,
    IHtmlSanitizer htmlSanitizer,
    IValidator<NoteUpsertRequest> noteValidator) : ApiControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet("notes")]
    public async Task<ActionResult<List<NoteDto>>> GetNotes([FromQuery] int? lessonId, CancellationToken ct)
    {
        var query = _db.LessonNotes.AsNoTracking().Where(n => n.UserId == CurrentUserId());
        if (lessonId is > 0)
        {
            query = query.Where(n => n.LessonId == lessonId);
        }

        var notes = await query
            .OrderByDescending(n => n.UpdatedAt)
            .Select(n => new NoteDto
            {
                LessonId = n.LessonId,
                ContentHtml = n.ContentHtml,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync(ct);
        return Ok(notes);
    }

    [HttpPut("notes/{lessonId:int}")]
    public async Task<ActionResult<NoteDto>> UpsertNote([FromRoute] int lessonId, [FromBody] NoteUpsertRequest request, CancellationToken ct)
    {
        // Finding security#12: giới hạn độ dài ContentHtml (50 KB — chống DB DoS).
        var invalid = await ValidateRequestAsync(noteValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var lessonExists = await _db.Lessons.AsNoTracking()
            .AnyAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (!lessonExists)
        {
            return NotFound(ErrorResponseDto.Create(ErrorCodes.NOT_FOUND, "Bài học không tồn tại"));
        }

        // Finding security#7: sanitize ContentHtml TRƯỚC khi lưu (cùng IHtmlSanitizer/whitelist
        // với LessonService) — chống stored XSS (Persistence/Entities/LessonNote.cs ghi "// sanitize"
        // nhưng trước đây lưu RAW).
        var sanitized = htmlSanitizer.Sanitize(request.ContentHtml);

        var now = DateTime.UtcNow;
        var note = await _db.LessonNotes.FirstOrDefaultAsync(n => n.UserId == CurrentUserId() && n.LessonId == lessonId, ct);
        if (note is null)
        {
            note = new LessonNote { UserId = CurrentUserId(), LessonId = lessonId, ContentHtml = sanitized, UpdatedAt = now };
            _db.LessonNotes.Add(note);
        }
        else
        {
            note.ContentHtml = sanitized;
            note.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new NoteDto { LessonId = lessonId, ContentHtml = note.ContentHtml, UpdatedAt = note.UpdatedAt });
    }

    [HttpDelete("notes/{lessonId:int}")]
    public async Task<ActionResult> DeleteNote([FromRoute] int lessonId, CancellationToken ct)
    {
        var note = await _db.LessonNotes.FirstOrDefaultAsync(n => n.UserId == CurrentUserId() && n.LessonId == lessonId, ct);
        if (note is null)
        {
            return NoContent();
        }

        _db.LessonNotes.Remove(note);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

/// <summary>Huy hiệu của tôi — API_REFERENCE.md §4.12 (route /achievements riêng).</summary>
[ApiVersion("1.0")]
[Route("api/v1/achievements")]
[Authorize]
public class AchievementsController(AppDbContext db) : ApiControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<ActionResult<List<AchievementDto>>> GetMyAchievements(CancellationToken ct)
    {
        var userId = CurrentUserId();
        var items = await _db.Achievements.AsNoTracking()
            .OrderBy(a => a.SortOrder)
            .Select(a => new AchievementDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                IconUrl = a.IconUrl,
                EarnedAt = _db.UserAchievements.AsNoTracking()
                    .Where(ua => ua.UserId == userId && ua.AchievementId == a.Id)
                    .Select(ua => (DateTime?)ua.EarnedAt)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);
        return Ok(items);
    }
}
