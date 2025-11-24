using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Api.Services;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string? fromName = null,
        string? fromEmail = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Email:Smtp:Username"];
            var smtpPassword = _configuration["Email:Smtp:Password"];
            var smtpEnableSsl = bool.Parse(_configuration["Email:Smtp:EnableSsl"] ?? "true");
            var defaultFromEmail = _configuration["Email:FromEmail"];
            var defaultFromName = _configuration["Email:FromName"];

            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogError("SMTP host is not configured");
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName ?? defaultFromName ?? "BluChilli Works", fromEmail ?? defaultFromEmail ?? "noreply@bluchilliworks.com"));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body>
                        <h2>New Contact Message</h2>
                        <p><strong>From:</strong> {fromName ?? "Unknown"} ({fromEmail ?? "Unknown"})</p>
                        <p><strong>Subject:</strong> {subject}</p>
                        <hr/>
                        <div>{body}</div>
                    </body>
                    </html>",
                TextBody = $"From: {fromName ?? "Unknown"} ({fromEmail ?? "Unknown"})\nSubject: {subject}\n\n{body}"
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(smtpHost, smtpPort, smtpEnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Error}", to, ex.Message);
            return false;
        }
    }
}
