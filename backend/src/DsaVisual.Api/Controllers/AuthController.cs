using Asp.Versioning;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Xác thực — API_REFERENCE.md §4.1 (9 endpoint) + §4.12 (2FA email — GP-T2: PUT /auth/2fa,
/// POST /auth/2fa/send, POST /auth/2fa/verify). Refresh token qua cookie HttpOnly
/// (SameSite=Strict; Secure; Path=/api/v1/auth) — SDD §1.2/§5.6.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/auth")]
public class AuthController(
    IAuthService service,
    IValidator<ChangePasswordRequest> changePasswordValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator,
    IValidator<UpdateProfileRequest> updateProfileValidator,
    IValidator<Verify2FaRequest> verify2FaValidator) : ApiControllerBase
{
    private readonly IAuthService _service = service;

    private const string RefreshCookieName = "refresh_token";

    /// <summary>
    /// Options cookie refresh — Secure CHỈ khi HTTPS (F5-Minor: dev chạy HTTP không bị chặn cookie).
    /// </summary>
    private CookieOptions BuildRefreshCookieOptions(DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = Request.IsHttps,
        Path = "/api/v1/auth",
        Expires = expires
    };

    /// <summary>Đăng ký — Công khai ([AllowAnonymous], API_REFERENCE §4.1).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshResponse>> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return ApplyCookieAndMap(result, statusCode: 201);
    }

    /// <summary>Đăng nhập — Công khai.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshResponse>> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return ApplyCookieAndMap(result);
    }

    /// <summary>Làm mới token — chỉ cần cookie (rotate-invalidate).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshResponse>> Refresh(CancellationToken ct)
    {
        var cookie = Request.Cookies[RefreshCookieName];
        var result = await _service.RefreshAsync(cookie, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return ApplyCookieAndMap(result);
    }

    /// <summary>Đăng xuất — thu hồi refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout(CancellationToken ct)
    {
        var result = await _service.LogoutAsync(CurrentUserId(), ct);
        if (result.IsSuccess)
        {
            Response.Cookies.Delete(RefreshCookieName, BuildRefreshCookieOptions());
        }

        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Thông tin bản thân.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserSummary>> GetMe(CancellationToken ct)
    {
        var result = await _service.GetMeAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Cập nhật hồ sơ (tên/avatar).</summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UserSummary>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(updateProfileValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.UpdateMeAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Đổi mật khẩu (thu hồi refresh phiên khác).</summary>
    [HttpPut("me/password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        // Finding security#12: validate TRƯỚC khi chạy logic — thiếu field → VALIDATION_FAILED,
        // không rơi vào OLD_PASSWORD_WRONG (tránh lộ thông tin + đúng contract API_REFERENCE §2.2).
        var invalid = await ValidateRequestAsync(changePasswordValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.ChangePasswordAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Quên mật khẩu — Công khai (trả thông báo chung, không lộ email).</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(forgotPasswordValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.ForgotPasswordAsync(request, ct);
        return result.IsSuccess ? Ok(new { message = "Nếu email tồn tại, bạn sẽ nhận được link đặt lại mật khẩu" }) : MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Đặt lại mật khẩu bằng token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(resetPasswordValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.ResetPasswordAsync(request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>
    /// 2FA email (GP-T2 — FR-1.11, API_REFERENCE §4.12). Bật: bắt buộc qua mã OTP
    /// (POST /auth/2fa/send → nhận mã qua email → POST /auth/2fa/verify); tắt: trực tiếp.
    /// </summary>
    [HttpPut("2fa")]
    [Authorize]
    public async Task<ActionResult<Toggle2FaResponse>> Toggle2Fa(
        [FromBody] Toggle2FaRequest request, CancellationToken ct)
    {
        var result = await _service.Toggle2FaAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>2FA email — gửi mã OTP 6 số (hiệu lực 5 phút, dùng 1 lần) qua SMTP (dev: MailHog).</summary>
    [HttpPost("2fa/send")]
    [Authorize]
    public async Task<ActionResult<Send2FaResponse>> Send2Fa(CancellationToken ct)
    {
        var result = await _service.Send2FaCodeAsync(CurrentUserId(), ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>2FA email — xác nhận mã OTP → bật 2FA cho tài khoản.</summary>
    [HttpPost("2fa/verify")]
    [Authorize]
    public async Task<ActionResult<Toggle2FaResponse>> Verify2Fa(
        [FromBody] Verify2FaRequest request, CancellationToken ct)
    {
        var invalid = await ValidateRequestAsync(verify2FaValidator, request, ct);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await _service.Verify2FaCodeAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    private ActionResult ApplyCookieAndMap(Result<RefreshResponse> result, int statusCode = 200)
    {
        if (result.IsSuccess && result.Value!.RefreshToken is { } refreshToken)
        {
            var expires = DateTimeOffset.UtcNow.AddDays(7);
            Response.Cookies.Append(RefreshCookieName, refreshToken, BuildRefreshCookieOptions(expires));
        }

        return result.IsSuccess
            ? StatusCode(statusCode, result.Value)
            : MapResultExtensions.MapResult(this, result);
    }
}
