using Portfolio_API.Models;

namespace Portfolio_API.Services
{
    public interface IEmailService
    {
        Task NotifyOwnerAsync(ContactMessage message, CancellationToken ct);
    }
}
