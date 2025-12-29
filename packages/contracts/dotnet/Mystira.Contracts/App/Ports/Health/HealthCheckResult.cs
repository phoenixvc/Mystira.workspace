namespace Mystira.Contracts.App.Ports.Health;

/// <summary>
/// Represents a health check result for API responses with string-based status.
/// This type is used for serialization in API responses where enum values
/// should be represented as strings.
/// </summary>
public record HealthCheckResult
{
    /// <summary>
    /// The status of the health check as a string (e.g., "Healthy", "Degraded", "Unhealthy").
    /// </summary>
    public string Status { get; init; } = "Healthy";

    /// <summary>
    /// Description or additional information about the health check result.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Duration of this health check.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// When this result was generated.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Additional data about the health check.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// Individual component health check results.
    /// </summary>
    public Dictionary<string, ComponentHealthResult>? Components { get; init; }

    /// <summary>
    /// Creates a healthy result.
    /// </summary>
    public static HealthCheckResult Healthy(string? description = null) => new()
    {
        Status = "Healthy",
        Description = description
    };

    /// <summary>
    /// Creates a degraded result.
    /// </summary>
    public static HealthCheckResult Degraded(string? description = null) => new()
    {
        Status = "Degraded",
        Description = description
    };

    /// <summary>
    /// Creates an unhealthy result.
    /// </summary>
    public static HealthCheckResult Unhealthy(string? description = null) => new()
    {
        Status = "Unhealthy",
        Description = description
    };
}

/// <summary>
/// Represents the health status of an individual component.
/// </summary>
public record ComponentHealthResult
{
    /// <summary>
    /// The status of the component as a string.
    /// </summary>
    public string Status { get; init; } = "Healthy";

    /// <summary>
    /// Description of the component health status.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Duration of this component's health check.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Additional data about the component health.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}
