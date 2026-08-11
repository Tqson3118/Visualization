using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using VisualizationDSA.Application.Common.Interfaces;

namespace VisualizationDSA.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_config["SMTP:Host"]) &&
            !_config["SMTP:Host"].StartsWith("CHANGE_ME") &&
            !string.IsNullOrWhiteSpace(_config["SMTP:User"]);

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            if (!IsConfigured)
            {
                // Dev placeholder: không có SMTP thật → log thay vì fail (PM sẽ điền key sau)
                _logger.LogInformation("[EmailService] SMTP chưa cấu hình (placeholder). Skip gửi tới {To}", toEmail);
                return;
            }

            var host = _config["SMTP:Host"];
            var port = int.TryParse(_config["SMTP:Port"], out var p) ? p : 587;
            var user = _config["SMTP:User"];
            var password = _config["SMTP:Password"];
            var from = _config["SMTP:FromEmail"] ?? "noreply@visualizationdsa.dev";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, password),
                Timeout = 15000
            };

            var message = new MailMessage(from, toEmail, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("[EmailService] Đã gửi email tới {To}", toEmail);
        }
    }
}
