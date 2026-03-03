namespace Mystira.Identity.Api.Services;

public class InMemoryProvisioningQueue : IProvisioningQueue
{
    private readonly Dictionary<string, ProvisioningJob> _jobs = new();
    private readonly Queue<string> _jobQueue = new();
    private readonly object _lock = new();

    public Task EnqueueProvisioningJobAsync(ProvisioningJob job, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _jobs[job.JobId] = job;
            _jobQueue.Enqueue(job.JobId);
        }

        return Task.CompletedTask;
    }

    public Task<ProvisioningJob?> DequeueJobAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_jobQueue.Count == 0)
            {
                return Task.FromResult<ProvisioningJob?>(null);
            }

            var jobId = _jobQueue.Dequeue();
            var job = _jobs[jobId];

            // Check if it's time to attempt this job
            if (job.NextAttemptAt.HasValue && job.NextAttemptAt > DateTime.UtcNow)
            {
                // Re-queue for later
                _jobQueue.Enqueue(jobId);
                return Task.FromResult<ProvisioningJob?>(null);
            }

            return Task.FromResult<ProvisioningJob?>(job);
        }
    }

    public Task MarkJobCompletedAsync(string jobId, ProvisioningResult result, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                _jobs.Remove(jobId);
            }
        }

        return Task.CompletedTask;
    }

    public Task MarkJobFailedAsync(string jobId, string error, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                var updatedJob = job with
                {
                    AttemptCount = job.AttemptCount + 1,
                    LastError = error,
                    NextAttemptAt = CalculateNextAttempt(job.AttemptCount + 1)
                };

                // If max attempts reached, remove the job
                if (updatedJob.AttemptCount >= 5)
                {
                    _jobs.Remove(jobId);
                }
                else
                {
                    _jobs[jobId] = updatedJob;
                    _jobQueue.Enqueue(jobId);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<ProvisioningJob>> GetPendingJobsAsync(int maxCount = 10, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var pendingJobs = _jobs.Values
                .Where(j => j.NextAttemptAt == null || j.NextAttemptAt <= DateTime.UtcNow)
                .Take(maxCount)
                .ToList();

            return Task.FromResult<IEnumerable<ProvisioningJob>>(pendingJobs);
        }
    }

    private static DateTime CalculateNextAttempt(int attemptCount)
    {
        // Exponential backoff: 1min, 5min, 25min, 2h, 10h
        var delays = new[] { 1, 5, 25, 120, 600 };
        var delayMinutes = attemptCount <= delays.Length ? delays[attemptCount - 1] : delays.Last();
        
        return DateTime.UtcNow.AddMinutes(delayMinutes);
    }
}
