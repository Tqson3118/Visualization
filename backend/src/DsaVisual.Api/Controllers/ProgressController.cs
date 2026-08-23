using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Tiến độ — API_REFERENCE.md §4.7 (overview, chi tiết bài học, báo cáo giảng viên + CSV).</summary>
[ApiVersion("1.0")]
[Route("api/v1/progress")]
[Authorize]
public class ProgressController(IProgressService service) : ApiControllerBase
{
    private readonly IProgressService _service = service;

    [HttpGet("me")]
    public async Task<ActionResult<ProgressOverviewDto>> GetMyOverview(CancellationToken ct)
    {
        var result = await _service.GetMyOverviewAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("me/lessons/{lessonId:int}")]
    public async Task<ActionResult<LessonProgressDetailDto>> GetMyLesson([FromRoute] int lessonId, CancellationToken ct)
    {
        var result = await _service.GetMyLessonAsync(CurrentUserId(), lessonId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("report")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<TeacherReportDto>> GetReport([FromQuery] int lessonId, CancellationToken ct)
    {
        var result = await _service.GetReportAsync(CurrentUserId(), CurrentRole(), lessonId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("report/export")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<IActionResult> ExportReport([FromQuery] int lessonId, CancellationToken ct)
    {
        var result = await _service.ExportReportCsvAsync(CurrentUserId(), CurrentRole(), lessonId, ct);
        if (!result.IsSuccess)
        {
            return MapResultExtensions.MapResult(this, result);
        }

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }
}
