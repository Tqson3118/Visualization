namespace DsaVisual.Application.Dtos;

/// <summary>Body PUT /auth/2fa — bật/tắt 2FA (GP-T2, API_REFERENCE §4.12).</summary>
public sealed record Toggle2FaRequest
{
    public bool Enabled { get; init; }
}

/// <summary>Body POST /auth/2fa/verify — mã OTP 6 số nhận qua email.</summary>
public sealed record Verify2FaRequest
{
    public string Code { get; init; } = string.Empty;
}

/// <summary>Response POST /auth/2fa/send.</summary>
public sealed record Send2FaResponse(string Message, int ExpiresInSeconds);

/// <summary>Response POST /auth/2fa/verify + PUT /auth/2fa.</summary>
public sealed record Toggle2FaResponse(bool Enabled, string Message);
