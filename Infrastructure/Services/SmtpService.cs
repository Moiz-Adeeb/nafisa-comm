using Application.Interfaces;
using Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public class SmtpService : ISmtpService
{
    private readonly IOptions<SmtpOption> _options;
    private readonly ILogger<SmtpService> _logger;

    public SmtpService(IOptions<SmtpOption> options, ILogger<SmtpService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string email, string subject, string body)
    {
        var smtpOptions = _options.Value;

        try
        {
            _logger.LogDebug("Sending email... to " + email + " with subject " + subject);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", smtpOptions.Email));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (body.Contains("<") && body.Contains(">")) // Simple HTML detection
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Connect to the SMTP server
                await client.ConnectAsync(
                    smtpOptions.Smtp,
                    smtpOptions.Port,
                    SecureSocketOptions.StartTls
                );

                // Authenticate
                await client.AuthenticateAsync(smtpOptions.Email, smtpOptions.Password);

                // Send the message
                await client.SendAsync(message);

                // Disconnect
                await client.DisconnectAsync(true);
                _logger.LogDebug("Sending email... success " + email + " with subject " + subject);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Message}", email, ex.Message);
            return false;
        }
    }
}
