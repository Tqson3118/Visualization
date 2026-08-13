using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Cấu hình hệ thống — GET/PUT /settings (API_REFERENCE.md §4.10, Admin).
/// Tách riêng khỏi AdminController để khớp route tài liệu (/settings, không phải /admin/settings).
/// GET trả SystemSettingsDto (object — shape FE AdminSettingsView); PUT nhận cùng shape và upsert key-value.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/settings")]
[Authorize(Roles = "ADMIN")]
public class SettingsController(ISettingService service, IValidator<SystemSettingsDto> validator) : ApiControllerBase
{
    private readonly ISettingService _service = service;

    [HttpGet]
    public async Task<ActionResult<SystemSettingsDto>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return MapResultExtensions.MapResult(this, result);
    }

    [HttpPut]
    public async Task<ActionResult<SystemSettingsDto>> Update([FromBody] SystemSettingsDto request, CancellationToken ct)
    {
        // Finding security#12: validate range trước khi upsert (minLength/upload/sandbox…).
        var invalid = await ValidateRequestAsync(validator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.UpdateAsync(CurrentUserId(), request, ct);
        if (!result.IsSuccess)
        {
            return MapResultExtensions.MapResult(this, result);
        }

        var updated = await _service.GetAllAsync(ct);
        return MapResultExtensions.MapResult(this, updated);
    }
}
