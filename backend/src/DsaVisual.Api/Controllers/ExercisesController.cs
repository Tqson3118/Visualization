using Asp.Versioning;
using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Bài tập — API_REFERENCE.md §4.6/§4.13 (CRUD, submit, practice, import CSV, code-submit).</summary>
[ApiVersion("1.0")]
[Route("api/v1/exercises")]
[Authorize]
public class ExercisesController(
    IExerciseService service,
    IValidator<CodeSubmitRequest> codeSubmitValidator) : ApiControllerBase
{
    private readonly IExerciseService _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ExerciseSummaryDto>>> GetList(
        [FromQuery] int? lessonId, [FromQuery] int? nodeId, [FromQuery] int? stage,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetListAsync(lessonId, nodeId, stage, page, pageSize, ct);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ExerciseDto>> Create([FromBody] ExerciseUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(CurrentUserId(), request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetExercise), new { id = result.Value!.Id }, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ExerciseDto>> Update([FromRoute] int id, [FromBody] ExerciseUpsertRequest request, CancellationToken ct)
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

    [HttpPost("{id:int}/submit")]
    [Authorize(Roles = "STUDENT,TEACHER")]
    public async Task<ActionResult<SubmitResultDto>> Submit([FromRoute] int id, [FromBody] SubmitRequest request, CancellationToken ct)
    {
        var result = await _service.SubmitAsync(CurrentUserId(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/practice")]
    [Authorize(Roles = "STUDENT,TEACHER")]
    public async Task<ActionResult<ExerciseDto>> Practice([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.PracticeAsync(CurrentUserId(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("import-csv")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ImportCsvResultDto>> ImportCsv(
        [FromForm] IFormFile file, [FromForm] int lessonId, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ErrorResponseDto.Create(ErrorCodes.UPLOAD_INVALID_TYPE, "Thiếu file CSV", "file"));
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var csvText = await reader.ReadToEndAsync(ct);
        var result = await _service.ImportCsvAsync(CurrentUserId(), lessonId, csvText, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/submissions/me")]
    public async Task<ActionResult<PagedResponse<SubmissionSummaryDto>>> GetMySubmissions(
        [FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default,
        // perf#6: cursor keyset — lastSubmittedAt + lastId = phần tử cuối trang trước (tùy chọn; không truyền → offset cũ)
        [FromQuery] DateTime? lastSubmittedAt = null, [FromQuery] int? lastId = null)
    {
        var result = await _service.GetMySubmissionsAsync(CurrentUserId(), id, page, pageSize, ct, lastSubmittedAt, lastId);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/submissions")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<PagedResponse<SubmissionSummaryDto>>> GetSubmissions(
        [FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default,
        [FromQuery] DateTime? lastSubmittedAt = null, [FromQuery] int? lastId = null)
    {
        var result = await _service.GetSubmissionsAsync(CurrentUserId(), CurrentRole(), id, page, pageSize, ct, lastSubmittedAt, lastId);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/code-submit")]
    [Authorize(Roles = "STUDENT,TEACHER")]
    public async Task<ActionResult<CodeSubmitResultDto>> SubmitCode([FromRoute] int id, [FromBody] CodeSubmitRequest request, CancellationToken ct)
    {
        // Finding security#12: giới hạn độ dài Code (chống DB DoS) + số liệu không âm.
        var invalid = await ValidateRequestAsync(codeSubmitValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.SubmitCodeAsync(CurrentUserId(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/code-submissions")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<PagedResponse<CodeSubmissionSummaryDto>>> GetCodeSubmissions(
        [FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default,
        // perf#7: cursor keyset — tùy chọn, fallback offset khi không truyền
        [FromQuery] DateTime? lastSubmittedAt = null, [FromQuery] int? lastId = null)
    {
        var result = await _service.GetCodeSubmissionsAsync(CurrentUserId(), CurrentRole(), id, page, pageSize, ct, lastSubmittedAt, lastId);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/code-submissions/me")]
    public async Task<ActionResult<PagedResponse<CodeSubmissionSummaryDto>>> GetMyCodeSubmissions(
        [FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default,
        [FromQuery] DateTime? lastSubmittedAt = null, [FromQuery] int? lastId = null)
    {
        var result = await _service.GetMyCodeSubmissionsAsync(CurrentUserId(), id, page, pageSize, ct, lastSubmittedAt, lastId);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }
}
