using Mystira.App.Application.Helpers;
using Mystira.Application.Ports;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Api.Services;

/// <summary>
/// Background service that processes pending data deletion requests for COPPA compliance.
/// Runs every hour, picks up requests past their scheduled deletion date, and executes deletion.
/// Retries failed deletions with exponential backoff (1h, 2h, 4h, 8h, 16h) up to 5 attempts.
/// Records confirmation for parent dashboard display upon completion.
/// </summary>
public class DataDeletionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataDeletionBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromHours(1);

    public DataDeletionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DataDeletionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data deletion background service started. Polling every {Interval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingDeletionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing pending deletions");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Data deletion background service stopped");
    }

    private async Task ProcessPendingDeletionsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var deletionRepo = scope.ServiceProvider.GetRequiredService<IDataDeletionRepository>();
        var deletionService = scope.ServiceProvider.GetRequiredService<IDataDeletionService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingDeletions = await deletionRepo.GetPendingDeletionsAsync(ct);

        if (pendingDeletions.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} pending data deletion requests", pendingDeletions.Count);

        foreach (var request in pendingDeletions)
        {
            try
            {
                var hashedId = LogAnonymizer.HashId(request.ChildProfileId);
                var isRetry = request.RetryCount > 0;

                if (isRetry)
                {
                    _logger.LogInformation(
                        "Retrying data deletion for {ChildProfileIdHash} (attempt {Attempt}/{MaxRetries})",
                        hashedId, request.RetryCount + 1, DataDeletionRequest.MaxRetries);
                }

                request.Status = DeletionStatus.InProgress;
                request.AddAuditEntry(isRetry ? $"DeletionRetry_Attempt{request.RetryCount + 1}" : "DeletionStarted", "System");
                await deletionRepo.UpdateAsync(request, ct);
                await unitOfWork.SaveChangesAsync(ct);

                var result = await deletionService.ExecuteDeletionAsync(request, ct);

                if (result.Success)
                {
                    request.Complete();
                    request.AddAuditEntry("DeletionCompleted", "System");
                    RecordDeletionConfirmation(request);
                    _logger.LogInformation("Data deletion completed for child profile {ChildProfileIdHash}", hashedId);
                }
                else
                {
                    HandleFailure(request, result.ErrorMessage, hashedId);
                }

                await deletionRepo.UpdateAsync(request, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var hashedId = LogAnonymizer.HashId(request.ChildProfileId);
                _logger.LogError(ex, "Failed to process deletion request {RequestId} for child {ChildProfileIdHash}",
                    request.Id, hashedId);
                var sanitized = LogAnonymizer.SanitizeExceptionMessage(ex.Message);
                HandleFailure(request, sanitized, hashedId);
                request.AddAuditEntry($"DeletionException: {sanitized}", "System");
                await deletionRepo.UpdateAsync(request, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }
    }

    private void HandleFailure(DataDeletionRequest request, string? errorMessage, string hashedId)
    {
        request.RetryCount++;
        request.Status = DeletionStatus.Failed;

        if (request.RetryCount < DataDeletionRequest.MaxRetries)
        {
            // Exponential backoff: 1h, 2h, 4h, 8h, 16h
            var backoffHours = Math.Pow(2, request.RetryCount - 1);
            request.NextRetryAt = DateTime.UtcNow.AddHours(backoffHours);
            request.AddAuditEntry(
                $"DeletionFailed_Retry{request.RetryCount}: {errorMessage}. Next retry at {request.NextRetryAt:u}",
                "System");
            _logger.LogWarning(
                "Data deletion failed for {ChildProfileIdHash} (attempt {Attempt}/{MaxRetries}). " +
                "Next retry scheduled at {NextRetryAt}",
                hashedId, request.RetryCount, DataDeletionRequest.MaxRetries, request.NextRetryAt);
        }
        else
        {
            // Max retries exhausted
            request.NextRetryAt = null;
            request.AddAuditEntry(
                $"DeletionFailed_MaxRetriesExhausted: {errorMessage}",
                "System");
            _logger.LogCritical(
                "Data deletion PERMANENTLY FAILED for {ChildProfileIdHash} after {MaxRetries} attempts. " +
                "Manual intervention required. Request ID: {RequestId}",
                hashedId, DataDeletionRequest.MaxRetries, request.Id);
        }
    }

    private void RecordDeletionConfirmation(DataDeletionRequest request)
    {
        request.ConfirmationEmailSent = true;
        request.AddAuditEntry("DeletionConfirmationRecorded", "System");
        _logger.LogInformation(
            "Data deletion confirmation recorded for child profile {ChildProfileIdHash}. " +
            "Parent will see confirmation on dashboard (email hash only stored).",
            LogAnonymizer.HashId(request.ChildProfileId));
    }
}
