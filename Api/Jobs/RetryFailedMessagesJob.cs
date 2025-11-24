using Api.Features.Messages;
using Api.Infrastructure.Database.MongoDb.Entities;
using Hangfire;
using Shared.Enums;

namespace Api.Jobs;

/// <summary>
/// Recurring job to retry failed messages that haven't exceeded max retries
/// </summary>
public class RetryFailedMessagesJob
{
    private readonly IMessageRepository _messageRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<RetryFailedMessagesJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryFailedMessagesJob"/> class
    /// </summary>
    public RetryFailedMessagesJob(
        IMessageRepository messageRepository,
        IBackgroundJobClient backgroundJobClient,
        ILogger<RetryFailedMessagesJob> logger)
    {
        _messageRepository = messageRepository;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    /// <summary>
    /// Execute the retry job
    /// </summary>
    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("Starting RetryFailedMessagesJob");

            var allMessages = await _messageRepository.GetAllMessagesAsync();
            var failedMessages = allMessages
                .Where(m => m.Status == MessageStatus.Failed && m.AttemptCount < m.MaxRetries)
                .ToList();

            _logger.LogInformation("Found {Count} failed messages to retry", failedMessages.Count);

            foreach (var message in failedMessages)
            {
                try
                {
                    // Enqueue a new job to retry sending
                    var jobId = _backgroundJobClient.Enqueue<SendMessageJob>(job => job.ProcessMessageAsync(message.ID));
                    
                    // Update the job ID
                    message.HangfireJobId = jobId;
                    await _messageRepository.SaveMessageAsync(message);

                    _logger.LogInformation("Enqueued retry job for message {MessageId}", message.ID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enqueue retry job for message {MessageId}", message.ID);
                }
            }

            _logger.LogInformation("RetryFailedMessagesJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RetryFailedMessagesJob failed: {Error}", ex.Message);
            throw;
        }
    }
}
