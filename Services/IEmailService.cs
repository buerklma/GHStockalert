using System.Threading.Tasks;

namespace StockAlerter.Services
{
    public interface IEmailService
    {
        Task SendAlertAsync(string recipient, string subject, string body);
    }
}