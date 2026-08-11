using System.Threading.Tasks;

namespace VisualizationDSA.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
        bool IsConfigured { get; }
    }
}
