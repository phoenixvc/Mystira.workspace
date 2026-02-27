namespace Mystira.Identity.Api.Services;

public class ProvisioningBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProvisioningBackgroundWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public ProvisioningBackgroundWorker(
        IServiceProvider serviceProvider,
        ILogger<ProvisioningBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "[no-email]";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return "[invalid-email]";

        var localPart = email.Substring(0, atIndex);
        var domain = email.Substring(atIndex + 1);

        // Show first 2 characters and last character of local part, mask the rest
        var maskedLocal = localPart.Length > 3
            ? $"{localPart.Substring(0, 2)}***{localPart[^1]}"
            : $"***{localPart[^1]}";

        return $"{maskedLocal}@{domain}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Provisioning background worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Provisioning background worker stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing provisioning jobs");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Provisioning background worker stopped");
    }

    private async Task ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var provisioningQueue = scope.ServiceProvider.GetRequiredService<IProvisioningQueue>();
        var provisioningService = scope.ServiceProvider.GetRequiredService<IEntraProvisioningService>();

        var pendingJobs = await provisioningQueue.GetPendingJobsAsync(maxCount: 10, cancellationToken);

        foreach (var job in pendingJobs)
        {
            try
            {
                _logger.LogInformation("Processing provisioning job {JobId} for email {Email} (attempt {AttemptCount})",
                    job.JobId, MaskEmail(job.Email), job.AttemptCount + 1);

                var result = await provisioningService.ProvisionUserAsync(job.Email, job.DisplayName, cancellationToken);

                if (result.IsSuccess)
                {
                    await provisioningQueue.MarkJobCompletedAsync(job.JobId,
                        new ProvisioningResult(true, result.EntraObjectId), cancellationToken);

                    _logger.LogInformation("Successfully completed provisioning job {JobId} for email {Email}",
                        job.JobId, MaskEmail(job.Email));
                }
                else
                {
                    await provisioningQueue.MarkJobFailedAsync(job.JobId,
                        result.ErrorMessage ?? "Unknown error", cancellationToken);

                    _logger.LogWarning("Failed provisioning job {JobId} for email {Email}: {Error}",
                        job.JobId, MaskEmail(job.Email), result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                await provisioningQueue.MarkJobFailedAsync(job.JobId, ex.Message, cancellationToken);
                _logger.LogError(ex, "Exception processing provisioning job {JobId} for email {Email}",
                    job.JobId, MaskEmail(job.Email));
            }
        }
    }
}
