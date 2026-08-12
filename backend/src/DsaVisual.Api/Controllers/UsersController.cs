using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>Quản lý người dùng — API_REFERENCE.md §4.8 (Admin).</summary>
[ApiVersion("1.0")]
[Route("api/v1/users")]
[Authorize(Roles = "ADMIN")]
public class UsersController(IUserService service, AppDbContext db) : ApiControllerBase
{
    private readonly IUserService _service = service;
    private readonly AppDbContext _db = db;

    private bool ActorIsPrimaryAdmin() =>
        _db.Users.AsNoTracking()
            .Where(u => u.Id == CurrentUserId())
            .Select(u => u.IsPrimaryAdmin)
            .FirstOrDefault();

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminUserDto>>> GetList(
        [FromQuery] string? role, [FromQuery] string? status, [FromQuery] string? q,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetListAsync(role, status, q, page, pageSize, ct);
        if (result.IsSuccess)
        {
            Response.Headers["X-Total-Count"] = result.Value!.Total.ToString();
        }

        return MapResultExtensions.MapResult(this, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> GetUser([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult> SetStatus(
        [FromRoute] int id, [FromBody] SetStatusRequest request, CancellationToken ct)
    {
        var result = await _service.SetStatusAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, request.IsActive, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut("{id:int}/role")]
    public async Task<ActionResult> SetRole([FromRoute] int id, [FromBody] SetRoleRequest request, CancellationToken ct)
    {
        var result = await _service.SetRoleAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, request.Role, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/approve-teacher")]
    public async Task<ActionResult> ApproveTeacher(
        [FromRoute] int id, [FromBody] ApproveTeacherRequest request, CancellationToken ct)
    {
        var result = await _service.ApproveTeacherAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<ActionResult> ResetPassword(
        [FromRoute] int id, [FromBody] AdminResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _service.ResetPasswordAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    public sealed class SetStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public sealed class SetRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
