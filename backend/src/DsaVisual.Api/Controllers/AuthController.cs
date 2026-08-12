using Asp.Versioning;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Xác thực — API_REFERENCE.md §4.1 (9 endpoint). Refresh token qua cookie HttpOnly
/// (SameSite=Strict; Secure; Path=/api/v1/auth) — SDD §1.2/§5.6.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/auth")]
public class AuthController(IAuthService service) : ApiControllerBase
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
        var result = await _service.UpdateMeAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Đổi mật khẩu (thu hồi refresh phiên khác).</summary>
    [HttpPut("me/password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _service.ChangePasswordAsync(CurrentUserId(), request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Quên mật khẩu — Công khai (trả thông báo chung, không lộ email).</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await _service.ForgotPasswordAsync(request, ct);
        return result.IsSuccess ? Ok(new { message = "Nếu email tồn tại, bạn sẽ nhận được link đặt lại mật khẩu" }) : MapResultExtensions.MapResult(this, result);
    }

    /// <summary>Đặt lại mật khẩu bằng token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _service.ResetPasswordAsync(request, ct);
        return MapResultExtensions.MapResult(this, result);
    }

    /// <summary>2FA — TODO: chưa triển khai (cần luồng email mã OTP + xác nhận), trả 501 theo API_REFERENCE §4.12.</summary>
    [HttpPut("2fa")]
    [Authorize]
    public ActionResult Toggle2Fa() =>
        StatusCode(501, new { message = "2FA chưa được triển khai (TODO — cần SMTP + OTP store)" });

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
