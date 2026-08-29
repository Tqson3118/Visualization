namespace DsaVisual.Application.Dtos;

/// <summary>Body POST /auth/register/otp — yêu cầu gửi mã OTP xác thực email về hộp thư (B0, bước 1/3).</summary>
public sealed record SendRegisterOtpRequest
{
    public string Email { get; init; } = string.Empty;
}

/// <summary>Body POST /auth/register/otp/verify — xác nhận mã OTP 6 số (B0, bước 2/3).</summary>
public sealed record VerifyRegisterOtpRequest
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

/// <summary>Response POST /auth/register/otp.</summary>
public sealed record SendRegisterOtpResponse(string Message, int ExpiresInSeconds);

/// <summary>
/// Response POST /auth/register/otp/verify — otpToken đính kèm RegisterRequest.OtpToken
/// khi gọi POST /auth/register (B0, bước 3/3). Token dùng 1 lần, hiệu lực 10 phút.
/// </summary>
public sealed record VerifyRegisterOtpResponse(string OtpToken, int ExpiresInSeconds, string Message);
