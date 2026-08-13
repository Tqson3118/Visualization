using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Base controller: helper CurrentUserId()/CurrentRole() (SDD §5.7.1) — đọc từ JWT claims.
/// exc#5: claim thiếu/malformed → UnauthorizedAccessException (ErrorHandlingMiddleware map 401 UNAUTHORIZED)
/// thay vì NRE/FormatException → 500. Defense-in-depth dù [Authorize] + MapInboundClaims=false đã giảm rủi ro.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (sub is null || !int.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Token thiếu hoặc sai claim sub");
        }

        return userId;
    }

    protected string CurrentRole()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role))
        {
            throw new UnauthorizedAccessException("Token thiếu claim role");
        }

        return role;
    }

    /// <summary>
    /// Validate FluentValidation ở Controller (finding security#12 — validator cho DTO body thiếu).
    /// Hợp lệ → null (tiếp tục); không hợp lệ → 400 VALIDATION_FAILED envelope §2.1 (khớp định dạng
    /// InvalidModelStateResponseFactory + MapResultExtensions).
    /// </summary>
    protected async Task<ActionResult?> ValidateRequestAsync<T>(
        IValidator<T> validator, T request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (validation.IsValid)
        {
            return null;
        }

        var details = validation.Errors
            .Select(e => new ErrorDetailDto(e.PropertyName, e.ErrorMessage))
            .ToList();
        return BadRequest(ErrorResponseDto.Create(
            ErrorCodes.VALIDATION_FAILED,
            "Dữ liệu không hợp lệ",
            validation.Errors.FirstOrDefault()?.PropertyName,
            details));
    }
}
