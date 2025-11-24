namespace Api.Services;

/// <summary>
/// Service for sending email messages
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email message
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="fromName">Name of the sender</param>
    /// <param name="fromEmail">Email address of the sender</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string? fromName = null,
        string? fromEmail = null,
        CancellationToken cancellationToken = default);
}
