namespace Mystira.Ai.RateLimiting;

/// <summary>
/// Simple asynchronous rate limiter that enforces an average maximum number of
/// requests per minute by spacing permits at a fixed interval (60s / maxPerMinute).
/// This is a lightweight, lock-based leaky-bucket approximation suitable for
/// gating outbound LLM calls. It is FIFO-ish due to the single critical section.
/// </summary>
public sealed class PerMinuteRateLimiter
{
    private readonly TimeSpan _intervalPerRequest;
    private DateTimeOffset _nextPermitUtc;
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new rate limiter with the specified maximum requests per minute.
    /// </summary>
    /// <param name="maxRequestsPerMinute">Maximum requests allowed per minute.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if maxRequestsPerMinute is not positive.</exception>
    public PerMinuteRateLimiter(int maxRequestsPerMinute)
    {
        if (maxRequestsPerMinute <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRequestsPerMinute), "Must be > 0");

        _intervalPerRequest = TimeSpan.FromMinutes(1) / maxRequestsPerMinute;
        _nextPermitUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Waits until a permit is available according to the configured per-minute rate.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset scheduled;

        lock (_lock)
        {
            // Schedule at the later of now or previously scheduled next slot
            scheduled = _nextPermitUtc < now ? now : _nextPermitUtc;
            _nextPermitUtc = scheduled + _intervalPerRequest;
        }

        var delay = scheduled - now;
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }
}
