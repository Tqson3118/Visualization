using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Lớp học (Module H) — API_REFERENCE.md §4.11.</summary>
[ApiVersion("1.0")]
[Route("api/v1/classes")]
[Authorize]
public class ClassesController(
    IClassService service,
    IValidator<JoinClassRequest> joinValidator,
    IValidator<AddMemberRequest> addMemberValidator) : ApiControllerBase
{
    private readonly IClassService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ClassDto>>> GetMyClasses(CancellationToken ct)
    {
        var result = await _service.GetMyClassesAsync(CurrentUserId(), CurrentRole(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassDto>> Create([FromBody] ClassUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(CurrentUserId(), request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetClass), new { id = result.Value!.Id }, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassDetailDto>> GetClass([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(CurrentUserId(), CurrentRole(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassDto>> Update([FromRoute] int id, [FromBody] ClassUpsertRequest request, CancellationToken ct)
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

    [HttpPost("{id:int}/join")]
    public async Task<ActionResult<ClassDetailDto>> Join([FromRoute] int id, [FromBody] JoinClassRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(joinValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.JoinAsync(CurrentUserId(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>v2.15: tham gia lớp bằng mã mời (sinh viên nhập mã 6 ký tự, không cần classId).</summary>
    [HttpPost("join-by-code")]
    public async Task<ActionResult<ClassDetailDto>> JoinByCode([FromBody] JoinClassByCodeRequest request, CancellationToken ct)
    {
        var result = await _service.JoinByCodeAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/members")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassDetailDto>> AddMember([FromRoute] int id, [FromBody] AddMemberRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(addMemberValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.AddMemberAsync(CurrentUserId(), CurrentRole(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpDelete("{id:int}/members/{userId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> RemoveMember([FromRoute] int id, [FromRoute] int userId, CancellationToken ct)
    {
        var result = await _service.RemoveMemberAsync(CurrentUserId(), CurrentRole(), id, userId, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/assignments")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassDetailDto>> AddAssignment(
        [FromRoute] int id, [FromBody] ClassAssignmentUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.AddAssignmentAsync(CurrentUserId(), CurrentRole(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}/assignments/{assignId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> UpdateAssignment(
        [FromRoute] int id, [FromRoute] int assignId, [FromBody] ClassAssignmentUpdateRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAssignmentAsync(CurrentUserId(), CurrentRole(), id, assignId, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpDelete("{id:int}/assignments/{assignId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> RemoveAssignment([FromRoute] int id, [FromRoute] int assignId, CancellationToken ct)
    {
        var result = await _service.RemoveAssignmentAsync(CurrentUserId(), CurrentRole(), id, assignId, ct);
        return MapResultExtensions.MapResult(this, result);
    }


    // ── Lộ trình học (Curriculum) ─────────────────────────────

    /// <summary>Xem lộ trình học của lớp (học viên: status from real progress; manager: same).</summary>
    [HttpGet("{id:int}/curriculum")]
    public async Task<ActionResult<ClassCurriculumDto>> GetCurriculum([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetCurriculumAsync(CurrentUserId(), CurrentRole(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Teacher/Admin: tạo/đổi meta lộ trình + publish/unpublish (draft ẩn với học viên).</summary>
    [HttpPut("{id:int}/curriculum")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassDetailDto>> UpdateCurriculum(
        [FromRoute] int id, [FromBody] ClassCurriculumUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateCurriculumAsync(CurrentUserId(), CurrentRole(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Teacher/Admin: sắp xếp lại thứ tự items trong lộ trình.</summary>
    [HttpPut("{id:int}/curriculum/reorder")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> ReorderCurriculum(
        [FromRoute] int id, [FromBody] ClassCurriculumReorderRequest request, CancellationToken ct)
    {
        var result = await _service.ReorderCurriculumAsync(CurrentUserId(), CurrentRole(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/report")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassReportDto>> GetReport([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetReportAsync(CurrentUserId(), CurrentRole(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}/report/export")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<IActionResult> ExportReport([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.ExportReportCsvAsync(CurrentUserId(), CurrentRole(), id, ct);
        if (!result.IsSuccess)
        {
            return MapResultExtensions.MapResult(this, result);
        }

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }
}
