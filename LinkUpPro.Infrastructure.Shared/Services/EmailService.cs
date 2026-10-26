using LinkUpPro.Core.Application.DTOs.Email;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace LinkUpPro.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequestDTO emailRequestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || string.IsNullOrEmpty(_emailSettings.SmtpUser) || string.IsNullOrEmpty(_emailSettings.SmtpPass) || string.IsNullOrEmpty(_emailSettings.EmailFrom))
                {
                    _logger.LogWarning("Email server is not fully configured. Email was not sent.");
                    return;
                }

                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(_emailSettings.EmailFrom!),
                    Subject = emailRequestDto.Subject
                };

                email.From.Add(new MailboxAddress(
                    _emailSettings.DisplayName ?? "LinkUp Pro",
                    _emailSettings.EmailFrom!
                ));

                if (!string.IsNullOrWhiteSpace(emailRequestDto.To))
                {
                    email.To.Add(MailboxAddress.Parse(emailRequestDto.To));
                }

                if (emailRequestDto.ToRange != null)
                {
                    foreach (var toItem in emailRequestDto.ToRange)
                    {
                        email.To.Add(MailboxAddress.Parse(toItem));
                    }
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                var socketOption = _emailSettings.SmtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, socketOption);
                await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo electronico");
            }
        }
    }
}
