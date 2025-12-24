namespace Mystira.Shared.Resilience;

/// <summary>
/// Configuration options for resilience policies.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Resilience";

    /// <summary>
    /// Number of retry attempts before giving up. Default: 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay in seconds for exponential backoff. Default: 2.
    /// </summary>
    public int BaseDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Number of consecutive failures before opening the circuit breaker. Default: 5.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// Duration in seconds the circuit breaker stays open. Default: 30.
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Request timeout in seconds. Default: 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Request timeout for long-running operations (e.g., LLM calls). Default: 300.
    /// </summary>
    public int LongRunningTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Whether to enable detailed logging of retry/circuit breaker events.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
}
