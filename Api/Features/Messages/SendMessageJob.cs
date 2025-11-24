using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Features.Messages;
using Api.Services;
using Shared.Enums;

namespace Api.Features.Messages;

/// <summary>
/// Background job for sending email messages
/// </summary>
public class SendMessageJob
{
    private readonly IMessageRepository _messageRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendMessageJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendMessageJob"/> class
    /// </summary>
    public SendMessageJob(
        IMessageRepository messageRepository,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<SendMessageJob> logger)
    {
        _messageRepository = messageRepository;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Process and send a message by ID
    /// </summary>
    /// <param name="messageId">The ID of the message to send</param>
    public async Task ProcessMessageAsync(string messageId)
    {
        try
        {
            var message = await _messageRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning("Message {MessageId} not found", messageId);
                return;
            }

            // Check if already sent or exceeded max retries
            if (message.Status == MessageStatus.Sent)
            {
                _logger.LogInformation("Message {MessageId} already sent", messageId);
                return;
            }

            if (message.AttemptCount >= message.MaxRetries)
            {
                _logger.LogWarning("Message {MessageId} exceeded max retries ({MaxRetries})", messageId, message.MaxRetries);
                message.Status = MessageStatus.Failed;
                message.LastErrorMessage = "Exceeded maximum retry attempts";
                message.LastAttemptedOn = DateTime.UtcNow.ToString("o");
                await _messageRepository.SaveMessageAsync(message);
                return;
            }

            // Update status to processing
            message.Status = MessageStatus.Processing;
            message.AttemptCount++;
            message.LastAttemptedOn = DateTime.UtcNow.ToString("o");
            await _messageRepository.SaveMessageAsync(message);

            // Get recipients from configuration
            var recipients = _configuration["Email:Recipients"] ?? "test@example.com";
            var recipientList = recipients.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Send email to all recipients
            var allSent = true;
            foreach (var recipient in recipientList)
            {
                var sent = await _emailService.SendEmailAsync(
                    to: recipient,
                    subject: message.Subject ?? "No Subject",
                    body: message.Body ?? "No message content",
                    fromName: message.Name,
                    fromEmail: message.EmailAddress
                );

                if (!sent)
                {
                    allSent = false;
                    _logger.LogError("Failed to send message {MessageId} to {Recipient}", messageId, recipient);
                }
            }

            if (allSent)
            {
                // Mark as sent
                message.Status = MessageStatus.Sent;
                message.SentOn = DateTime.UtcNow.ToString("o");
                message.LastErrorMessage = null;
                _logger.LogInformation("Message {MessageId} sent successfully to all recipients", messageId);
            }
            else
            {
                // Mark as failed, will retry based on Hangfire configuration
                message.Status = MessageStatus.Failed;
                message.LastErrorMessage = "Failed to send to one or more recipients";
                _logger.LogWarning("Message {MessageId} failed to send to all recipients", messageId);
            }

            await _messageRepository.SaveMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}: {Error}", messageId, ex.Message);

            try
            {
                var message = await _messageRepository.GetMessageByIdAsync(messageId);
                if (message != null)
                {
                    message.Status = MessageStatus.Failed;
                    message.LastErrorMessage = ex.Message;
                    message.LastAttemptedOn = DateTime.UtcNow.ToString("o");
                    await _messageRepository.SaveMessageAsync(message);
                }
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Failed to update message {MessageId} status after error", messageId);
            }

            throw; // Re-throw to let Hangfire handle retry
        }
    }
}
