using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Controller mẫu theo SDD §5.7.1: nhận DTO → gọi Service → map Result qua MapResult.
/// KHÔNG logic nghiệp vụ > 5 dòng, KHÔNG truy cập DbContext (SDD §5.3.1).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/lessons")]
[Authorize]
public class LessonsController(ILessonService service) : ControllerBase
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
        var result = await _service.CreateAsync(CurrentUserId(), request, ct);
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

    private int CurrentUserId() => int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    private string CurrentRole() => User.FindFirst(ClaimTypes.Role)!.Value;
}
