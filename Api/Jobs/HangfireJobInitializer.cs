using Hangfire;

namespace Api.Jobs;

/// <summary>
/// Hosted service that initializes Hangfire recurring jobs after the application starts
/// and JobStorage is properly initialized.
/// </summary>
public class HangfireJobInitializer : IHostedService
{
    private readonly ILogger<HangfireJobInitializer> _logger;

    public HangfireJobInitializer(ILogger<HangfireJobInitializer> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Wait a moment to ensure Hangfire server is fully started
            Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ContinueWith(_ =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    RegisterRecurringJobs();
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Hangfire recurring jobs");
            // Don't throw - allow the application to start even if job registration fails
            return Task.CompletedTask;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void RegisterRecurringJobs()
    {
        try
        {
            // Register retry failed messages job - runs every 5 minutes
            RecurringJob.AddOrUpdate<RetryFailedMessagesJob>(
                "retry-failed-messages",
                job => job.ExecuteAsync(),
                "*/5 * * * *"); // Cron expression: every 5 minutes

            _logger.LogInformation("✅ Hangfire recurring jobs registered successfully");
            Console.WriteLine("✅ Hangfire recurring jobs registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "⚠️ Failed to register Hangfire recurring jobs");
            Console.WriteLine($"⚠️ Failed to register Hangfire recurring jobs: {ex.Message}");
        }
    }
}
