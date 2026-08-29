using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DsaVisual.Application.Services;

/// <summary>
/// Phát hành JWT + refresh token (SDD §5.3.7 — Singleton, không state).
/// JWT HS256: claims sub=userId, role, iat, exp, jti; expiry từ config <c>DSA:Jwt:AccessTokenMinutes</c>.
/// Refresh token: 64 byte ngẫu nhiên base64url; lưu SHA256 hash (SDD §7.3.5).
/// </summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(int userId, string role);
    (string Token, DateTime ExpiresAt) CreateTwoFactorToken(int userId);
    int? ValidateTwoFactorToken(string token);
    string CreateRefreshToken();
    string HashToken(string token);
}

public sealed class TokenService(IConfiguration config) : ITokenService
{
    private readonly SymmetricSecurityKey _key =
        new(Encoding.UTF8.GetBytes(config["DSA:Jwt:Secret"] ?? string.Empty));

    public (string Token, DateTime ExpiresAt) CreateAccessToken(int userId, string role)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(config.GetValue("DSA:Jwt:AccessTokenMinutes", 60));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: config["DSA:Jwt:Issuer"],
            audience: config["DSA:Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string Token, DateTime ExpiresAt) CreateTwoFactorToken(int userId)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(5);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("purpose", "2fa_login"),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: config["DSA:Jwt:Issuer"],
            audience: config["DSA:Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public int? ValidateTwoFactorToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = !string.IsNullOrEmpty(config["DSA:Jwt:Issuer"]),
                ValidIssuer = config["DSA:Jwt:Issuer"],
                ValidateAudience = !string.IsNullOrEmpty(config["DSA:Jwt:Audience"]),
                ValidAudience = config["DSA:Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var purpose = principal.FindFirst("purpose")?.Value;
            if (purpose != "2fa_login") return null;

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(sub, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    public string CreateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
