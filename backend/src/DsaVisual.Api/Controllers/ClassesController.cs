using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>Lớp học (Module H) — API_REFERENCE.md §4.11.</summary>
[ApiVersion("1.0")]
[Route("api/v1/classes")]
[Authorize]
public class ClassesController(IClassService service) : ApiControllerBase
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
        var result = await _service.JoinAsync(CurrentUserId(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/members")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ClassDetailDto>> AddMember([FromRoute] int id, [FromBody] AddMemberRequest request, CancellationToken ct)
    {
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
