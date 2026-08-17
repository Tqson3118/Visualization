using System.Net;
using System.Net.Mail;
using DsaVisual.Application.Options;

namespace DsaVisual.Application.Services;

/// <summary>
/// Tạo <see cref="SmtpClient"/> theo <see cref="EmailOptions"/> — tập trung logic AUTH/TLS để mọi nơi gửi email dùng chung.
/// </summary>
/// <remarks>
/// - MailHog (UseMailHog=true): không AUTH, không TLS (dev — localhost:1025).
/// - SMTP thật Gmail (UseMailHog=false): <see cref="SmtpClient.EnableSsl"/>=true (TLS/STARTTLS qua cổng 587) +
///   <see cref="SmtpClient.Credentials"/> khi có đủ username + password (lấy từ env lúc runtime — không bao giờ từ file tracked).
/// - Không log password; Timeout ngắn (10s) để không giữ request (GP-T2).
/// </remarks>
public static class SmtpClientFactory
{
    public static SmtpClient Create(EmailOptions email)
    {
        var smtp = new SmtpClient(email.SmtpHost ?? string.Empty, email.SmtpPort)
        {
            Timeout = 10_000,
            // SMTP thật → TLS bắt buộc (Gmail: cổng 587 = STARTTLS Explicit TLS)
            EnableSsl = !email.UseMailHog,
        };

        if (!email.UseMailHog)
        {
            if (!string.IsNullOrWhiteSpace(email.SmtpUsername) &&
                !string.IsNullOrWhiteSpace(email.SmtpPassword))
            {
                smtp.Credentials = new NetworkCredential(email.SmtpUsername, email.SmtpPassword);
            }
        }

        return smtp;
    }
}
