using Asp.Versioning;
using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Controller mẫu theo SDD §5.7.1: nhận DTO → gọi Service → map Result qua MapResult.
/// KHÔNG logic nghiệp vụ > 5 dòng, KHÔNG truy cập DbContext (SDD §5.3.1).
/// v2.15: endpoint kiểm duyệt (admin) + báo cáo vi phạm (student).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/lessons")]
[Authorize]
public class LessonsController(ILessonService service) : ApiControllerBase
{
    private readonly ILessonService _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<LessonSummaryDto>>> GetLessons(
        [FromQuery] int? topicId, [FromQuery] string? status, [FromQuery] string? q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetListAsync(
            CurrentUserId(), CurrentRole(), topicId, status, q, page, pageSize, ct);

        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LessonDto>> GetLesson(
        [FromRoute] int id, [FromQuery] bool includeContent = false, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(CurrentUserId(), CurrentRole(), id, includeContent, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<LessonDto>> Create([FromBody] LessonUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(CurrentUserId(), CurrentRole(), request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetLesson), new { id = result.Value!.Id }, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<LessonDto>> Update(
        [FromRoute] int id, [FromBody] LessonUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(CurrentUserId(), CurrentRole(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(CurrentUserId(), CurrentRole(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/mark-viewed")]
    public async Task<ActionResult> MarkViewed([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.MarkViewedAsync(CurrentUserId(), CurrentRole(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/feedback")]
    public async Task<ActionResult<FeedbackSavedDto>> SubmitFeedback(
        [FromRoute] int id, [FromBody] LessonFeedbackRequest request, CancellationToken ct)
    {
        var result = await _service.AddFeedbackAsync(CurrentUserId(), CurrentRole(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Báo cáo vi phạm (v2.15) ──

    [HttpPost("{id:int}/report")]
    public async Task<ActionResult> ReportLesson(
        [FromRoute] int id, [FromBody] LessonReportRequest request, CancellationToken ct)
    {
        var result = await _service.ReportAsync(CurrentUserId(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    // ── Kiểm duyệt Admin (v2.15) ──

    [HttpGet("pending")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<List<LessonSummaryDto>>> GetPendingLessons(CancellationToken ct)
    {
        var result = await _service.GetPendingListAsync(ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<LessonDto>> ReviewLesson(
        [FromRoute] int id, [FromBody] LessonReviewRequest request, CancellationToken ct)
    {
        if (!request.Approve && string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(ErrorResponseDto.Create(
                ErrorCodes.VALIDATION_FAILED,
                "Phải nhập lý do khi từ chối duyệt bài học",
                "reason",
                [new ErrorDetailDto("reason", "Phải nhập lý do khi từ chối duyệt bài học")]));
        }

        var result = await _service.ReviewAsync(CurrentUserId(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }
}
