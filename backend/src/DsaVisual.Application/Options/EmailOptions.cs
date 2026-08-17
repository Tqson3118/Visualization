using Microsoft.Extensions.Configuration;

namespace DsaVisual.Application.Options;

/// <summary>
/// Cấu hình SMTP — map từ section <c>"DSA:Email"</c> (appsettings hoặc env var <c>DSA__Email__*</c>).
/// </summary>
/// <remarks>
/// Quy ước (đồng bộ toàn hệ thống — SETUP_TODO §10.1):
/// - <see cref="UseMailHog"/>=true   → chế độ dev MailHog: KHÔNG AUTH, KHÔNG TLS (MailHog nhận mọi email).
/// - <see cref="UseMailHog"/>=false  → SMTP thật (Gmail): AUTH bằng <see cref="SmtpUsername"/>/<see cref="SmtpPassword"/> + TLS bắt buộc.
/// - <see cref="SmtpPassword"/> CHỈ đọc từ config/env lúc runtime — KHÔNG bao giờ ghi password vào file tracked, git diff, log hay report.
/// </remarks>
public sealed class EmailOptions
{
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 1025;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? From { get; set; }

    /// <summary>
    /// true = MailHog dev (mặc định — an toàn, không cần credential); false = SMTP thật cần AUTH+TLS.
    /// </summary>
    public bool UseMailHog { get; set; } = true;

    public static EmailOptions FromConfiguration(IConfiguration config)
    {
        var section = config.GetSection("DSA:Email");
        return new EmailOptions
        {
            SmtpHost = section["SmtpHost"],
            SmtpPort = section.GetValue("SmtpPort", 1025),
            SmtpUsername = section["SmtpUsername"],
            SmtpPassword = section["SmtpPassword"],
            From = section["From"],
            UseMailHog = ParseBool(section["UseMailHog"], true),
        };
    }

    private static bool ParseBool(string? value, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }
        return bool.TryParse(value, out var result) ? result : fallback;
    }
}
