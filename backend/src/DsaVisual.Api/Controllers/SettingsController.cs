using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Cấu hình hệ thống — GET/PUT /settings (API_REFERENCE.md §4.10, Admin).
/// Tách riêng khỏi AdminController để khớp route tài liệu (/settings, không phải /admin/settings).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/settings")]
[Authorize(Roles = "ADMIN")]
public class SettingsController(ISettingService service) : ApiControllerBase
{
    private readonly ISettingService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<SettingDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut]
    public async Task<ActionResult> Update([FromBody] SettingsUpdateRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(CurrentUserId(), request, ct);
        return result.IsSuccess ? Ok(await _service.GetAllAsync(ct)) : MapResultExtensions.MapResult(this, result);
    }
}
