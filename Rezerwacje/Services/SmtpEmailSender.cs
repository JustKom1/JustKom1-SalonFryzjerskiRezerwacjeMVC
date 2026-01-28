using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Rezerwacje.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                _logger.LogInformation("Wysyłam email do: {Email}, host: {Host}, port: {Port}",
                    email, _settings.Host, _settings.Port);

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                    EnableSsl = true,
                    Timeout = 10000,
                    UseDefaultCredentials = false
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.From),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                message.To.Add(email);

                await client.SendMailAsync(message);

                _logger.LogInformation("Email wysłany do: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BŁĄD SMTP przy wysyłaniu emaila do: {Email}", email);
                throw;
            }
        }
    }
}
