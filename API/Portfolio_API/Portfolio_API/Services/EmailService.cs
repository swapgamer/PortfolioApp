using Portfolio_API.Models;

namespace Portfolio_API.Services
{
    // PLACEHOLDER implementation -- logs instead of actually sending an email.
    // Swap this out once a real provider is chosen (SendGrid, SMTP, Azure Communication
    // Services, etc.) per Docs/02-High-Level-Design.md section 2.4. Keeps ContactController
    // fully functional end-to-end in the meantime, without needing provider credentials yet.
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task NotifyOwnerAsync(ContactMessage message, CancellationToken ct)
        {
            _logger.LogInformation(
                "New contact message from {Name} <{Email}>: {Message}",
                message.Name, message.Email, message.Message);

            return Task.CompletedTask;
        }
    }
}
