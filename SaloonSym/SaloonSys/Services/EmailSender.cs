using Microsoft.AspNetCore.Identity.UI.Services;

namespace SaloonSys.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IEmailService _emailService;

        public EmailSender(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                await _emailService.SendEmailAsync(email, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd wysyłania emaila: {ex.Message}");
            }
        }
    }
}
