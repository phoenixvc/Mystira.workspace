namespace Mystira.Contracts.App.Ports.Health;

/// <summary>
/// Health check port interface and related types for service health monitoring.
/// </summary>

/// <summary>
/// Port interface for health check operations.
/// Implementations provide health status for various system components.
/// </summary>
public interface IHealthCheckPort
{
    /// <summary>
    /// Performs a health check and returns the report.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health report for the system.</returns>
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the name of this health check.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the tags associated with this health check for filtering.
    /// </summary>
    IEnumerable<string> Tags { get; }
}

/// <summary>
/// Represents the overall health status of a system or component.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// The component is healthy and operating normally.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// The component is degraded but still operational.
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// The component is unhealthy and not operational.
    /// </summary>
    Unhealthy = 2
}

/// <summary>
/// Represents a complete health check report for the system.
/// </summary>
public record HealthReport
{
    /// <summary>
    /// Overall status of the system.
    /// </summary>
    public required HealthStatus Status { get; init; }

    /// <summary>
    /// Total duration of all health checks.
    /// </summary>
    public required TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// Individual health check entries.
    /// </summary>
    public required IReadOnlyDictionary<string, HealthCheckEntry> Entries { get; init; }

    /// <summary>
    /// When this report was generated.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a healthy report with no entries.
    /// </summary>
    public static HealthReport Healthy() => new()
    {
        Status = HealthStatus.Healthy,
        TotalDuration = TimeSpan.Zero,
        Entries = new Dictionary<string, HealthCheckEntry>()
    };

    /// <summary>
    /// Creates a report from a collection of entries.
    /// </summary>
    public static HealthReport FromEntries(IReadOnlyDictionary<string, HealthCheckEntry> entries, TimeSpan duration)
    {
        var status = HealthStatus.Healthy;

        foreach (var entry in entries.Values)
        {
            if (entry.Status == HealthStatus.Unhealthy)
            {
                status = HealthStatus.Unhealthy;
                break;
            }

            if (entry.Status == HealthStatus.Degraded && status != HealthStatus.Unhealthy)
            {
                status = HealthStatus.Degraded;
            }
        }

        return new HealthReport
        {
            Status = status,
            TotalDuration = duration,
            Entries = entries
        };
    }
}

/// <summary>
/// Represents a single health check entry in a health report.
/// </summary>
public record HealthCheckEntry
{
    /// <summary>
    /// Status of this health check.
    /// </summary>
    public required HealthStatus Status { get; init; }

    /// <summary>
    /// Description or additional information about the health check result.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Duration of this health check.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Exception that occurred during the health check, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Additional data about the health check.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Data { get; init; }

    /// <summary>
    /// Tags associated with this health check for filtering.
    /// </summary>
    public IEnumerable<string> Tags { get; init; } = [];

    /// <summary>
    /// Creates a healthy entry.
    /// </summary>
    public static HealthCheckEntry Healthy(TimeSpan duration, string? description = null) => new()
    {
        Status = HealthStatus.Healthy,
        Duration = duration,
        Description = description
    };

    /// <summary>
    /// Creates a degraded entry.
    /// </summary>
    public static HealthCheckEntry Degraded(TimeSpan duration, string? description = null, Exception? exception = null) => new()
    {
        Status = HealthStatus.Degraded,
        Duration = duration,
        Description = description,
        Exception = exception
    };

    /// <summary>
    /// Creates an unhealthy entry.
    /// </summary>
    public static HealthCheckEntry Unhealthy(TimeSpan duration, string? description = null, Exception? exception = null) => new()
    {
        Status = HealthStatus.Unhealthy,
        Duration = duration,
        Description = description,
        Exception = exception
    };
}

/// <summary>
/// Options for configuring health checks.
/// </summary>
public record HealthCheckOptions
{
    /// <summary>
    /// Timeout for individual health checks.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Tags to filter which health checks to run.
    /// </summary>
    public IEnumerable<string>? Tags { get; init; }

    /// <summary>
    /// Whether to include detailed data in the response.
    /// </summary>
    public bool IncludeDetails { get; init; } = true;

    /// <summary>
    /// Whether to include exception information in the response.
    /// </summary>
    public bool IncludeExceptions { get; init; } = false;
}
