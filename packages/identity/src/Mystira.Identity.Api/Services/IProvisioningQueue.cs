namespace Mystira.Identity.Api.Services;

/// <summary>
/// Provides queue management for Entra ID provisioning operations.
/// Handles job queuing, dequeueing, and status tracking for asynchronous provisioning.
/// </summary>
public interface IProvisioningQueue
{
    /// <summary>
    /// Enqueues a new provisioning job for processing by the background worker.
    /// </summary>
    /// <param name="job">The provisioning job to enqueue.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the enqueue operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the job is invalid.</exception>
    Task EnqueueProvisioningJobAsync(ProvisioningJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the next available provisioning job for processing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The next provisioning job if available; otherwise null.</returns>
    Task<ProvisioningJob?> DequeueJobAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a provisioning job as completed and removes it from the queue.
    /// </summary>
    /// <param name="jobId">The identifier of the job to mark as completed.</param>
    /// <param name="result">The result of the provisioning operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the mark operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the job ID is invalid.</exception>
    Task MarkJobCompletedAsync(string jobId, ProvisioningResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a provisioning job as failed and updates error information.
    /// </summary>
    /// <param name="jobId">The identifier of the job to mark as failed.</param>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the mark operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the job ID is invalid.</exception>
    Task MarkJobFailedAsync(string jobId, string error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a collection of pending provisioning jobs.
    /// </summary>
    /// <param name="maxCount">The maximum number of jobs to retrieve; defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of pending provisioning jobs.</returns>
    Task<IEnumerable<ProvisioningJob>> GetPendingJobsAsync(int maxCount = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a provisioning job for Entra ID user creation and linking.
/// Contains job metadata, user information, and retry tracking details.
/// </summary>
/// <param name="JobId">The unique identifier for the provisioning job.</param>
/// <param name="Email">The email address of the user to provision.</param>
/// <param name="DisplayName">The display name of the user to provision.</param>
/// <param name="AccountId">The local account identifier associated with the job.</param>
/// <param name="AttemptCount">The number of attempts made; defaults to 0.</param>
/// <param name="NextAttemptAt">The timestamp for the next retry attempt; optional.</param>
/// <param name="CreatedAt">The creation timestamp of the job; defaults to current time.</param>
/// <param name="LastError">The last error message from a failed attempt; optional.</param>
public record ProvisioningJob(
    string JobId,
    string Email,
    string DisplayName,
    string AccountId,
    int AttemptCount = 0,
    DateTime? NextAttemptAt = null,
    DateTime CreatedAt = default,
    string? LastError = null);

/// <summary>
/// Represents the result of a provisioning job operation.
/// Contains success status and relevant outcome information.
/// </summary>
/// <param name="IsSuccess">True if the operation succeeded; false otherwise.</param>
/// <param name="EntraObjectId">The Entra ID object ID if successful; otherwise null.</param>
/// <param name="ErrorMessage">Error message describing the failure, if any.</param>
public record ProvisioningResult(
    bool IsSuccess,
    string? EntraObjectId = null,
    string? ErrorMessage = null);
