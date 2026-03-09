namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a service health status changes.
/// </summary>
public sealed record ServiceHealthStatusChanged : IntegrationEventBase
{
    /// <summary>
    /// Service identifier.
    /// </summary>
    public required string ServiceId { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Previous status (healthy, degraded, unhealthy).
    /// </summary>
    public required string FromStatus { get; init; }

    /// <summary>
    /// New status.
    /// </summary>
    public required string ToStatus { get; init; }

    /// <summary>
    /// Reason for status change.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Environment (dev, staging, prod).
    /// </summary>
    public required string Environment { get; init; }
}

/// <summary>
/// Published when a circuit breaker is triggered.
/// </summary>
public sealed record CircuitBreakerTriggered : IntegrationEventBase
{
    /// <summary>
    /// Circuit breaker name.
    /// </summary>
    public required string CircuitName { get; init; }

    /// <summary>
    /// Target service/endpoint.
    /// </summary>
    public required string TargetService { get; init; }

    /// <summary>
    /// Failure count that triggered it.
    /// </summary>
    public required int FailureCount { get; init; }

    /// <summary>
    /// Failure threshold.
    /// </summary>
    public required int FailureThreshold { get; init; }

    /// <summary>
    /// Circuit state (open, half_open, closed).
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// Retry time if applicable.
    /// </summary>
    public DateTimeOffset? RetryAt { get; init; }
}

/// <summary>
/// Published when a circuit breaker is reset.
/// </summary>
public sealed record CircuitBreakerReset : IntegrationEventBase
{
    /// <summary>
    /// Circuit breaker name.
    /// </summary>
    public required string CircuitName { get; init; }

    /// <summary>
    /// Target service.
    /// </summary>
    public required string TargetService { get; init; }

    /// <summary>
    /// How long it was open (seconds).
    /// </summary>
    public required int OpenDurationSeconds { get; init; }

    /// <summary>
    /// Reset reason (manual, timeout, success).
    /// </summary>
    public required string ResetReason { get; init; }
}

/// <summary>
/// Published when rate limit is exceeded.
/// </summary>
public sealed record RateLimitExceeded : IntegrationEventBase
{
    /// <summary>
    /// Rate limit rule name.
    /// </summary>
    public required string RuleName { get; init; }

    /// <summary>
    /// Endpoint or resource.
    /// </summary>
    public required string Resource { get; init; }

    /// <summary>
    /// Client identifier (user, IP, API key).
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Client type (user, api_key, anonymous).
    /// </summary>
    public required string ClientType { get; init; }

    /// <summary>
    /// Request count in window.
    /// </summary>
    public required int RequestCount { get; init; }

    /// <summary>
    /// Limit that was exceeded.
    /// </summary>
    public required int Limit { get; init; }

    /// <summary>
    /// Window duration (seconds).
    /// </summary>
    public required int WindowSeconds { get; init; }

    /// <summary>
    /// Retry after (seconds).
    /// </summary>
    public required int RetryAfterSeconds { get; init; }
}

/// <summary>
/// Published when database connection pool is exhausted.
/// </summary>
public sealed record DatabaseConnectionPoolExhausted : IntegrationEventBase
{
    /// <summary>
    /// Database identifier.
    /// </summary>
    public required string DatabaseId { get; init; }

    /// <summary>
    /// Database type (postgres, redis, cosmos).
    /// </summary>
    public required string DatabaseType { get; init; }

    /// <summary>
    /// Active connections.
    /// </summary>
    public required int ActiveConnections { get; init; }

    /// <summary>
    /// Pool size.
    /// </summary>
    public required int PoolSize { get; init; }

    /// <summary>
    /// Waiting requests.
    /// </summary>
    public required int WaitingRequests { get; init; }

    /// <summary>
    /// Service that experienced exhaustion.
    /// </summary>
    public required string ServiceId { get; init; }
}

/// <summary>
/// Published when high latency is detected.
/// </summary>
public sealed record HighLatencyDetected : IntegrationEventBase
{
    /// <summary>
    /// Endpoint or operation.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// Observed latency (ms).
    /// </summary>
    public required int LatencyMs { get; init; }

    /// <summary>
    /// Expected SLA threshold (ms).
    /// </summary>
    public required int ThresholdMs { get; init; }

    /// <summary>
    /// P99 latency in window.
    /// </summary>
    public int? P99LatencyMs { get; init; }

    /// <summary>
    /// Service affected.
    /// </summary>
    public required string ServiceId { get; init; }

    /// <summary>
    /// Suspected cause if known.
    /// </summary>
    public string? SuspectedCause { get; init; }
}

/// <summary>
/// Published when storage quota warning is triggered.
/// </summary>
public sealed record StorageQuotaWarning : IntegrationEventBase
{
    /// <summary>
    /// Storage resource.
    /// </summary>
    public required string ResourceId { get; init; }

    /// <summary>
    /// Resource type (blob, database, cache).
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Current usage in bytes.
    /// </summary>
    public required long UsedBytes { get; init; }

    /// <summary>
    /// Total quota in bytes.
    /// </summary>
    public required long QuotaBytes { get; init; }

    /// <summary>
    /// Usage percentage.
    /// </summary>
    public required int UsagePercent { get; init; }

    /// <summary>
    /// Warning threshold percentage.
    /// </summary>
    public required int ThresholdPercent { get; init; }
}

/// <summary>
/// Published when a service dependency fails.
/// </summary>
public sealed record ServiceDependencyFailure : IntegrationEventBase
{
    /// <summary>
    /// Dependent service.
    /// </summary>
    public required string ServiceId { get; init; }

    /// <summary>
    /// Failed dependency.
    /// </summary>
    public required string DependencyId { get; init; }

    /// <summary>
    /// Dependency type (database, api, cache, queue).
    /// </summary>
    public required string DependencyType { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// Whether fallback was used.
    /// </summary>
    public bool FallbackUsed { get; init; }

    /// <summary>
    /// Impact level (none, degraded, critical).
    /// </summary>
    public required string ImpactLevel { get; init; }
}

/// <summary>
/// Published when a backup completes.
/// </summary>
public sealed record BackupCompleted : IntegrationEventBase
{
    /// <summary>
    /// Backup ID.
    /// </summary>
    public required string BackupId { get; init; }

    /// <summary>
    /// Resource backed up.
    /// </summary>
    public required string ResourceId { get; init; }

    /// <summary>
    /// Backup type (full, incremental, differential).
    /// </summary>
    public required string BackupType { get; init; }

    /// <summary>
    /// Backup size in bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// Storage location.
    /// </summary>
    public required string StorageLocation { get; init; }

    /// <summary>
    /// Retention period (days).
    /// </summary>
    public required int RetentionDays { get; init; }
}

/// <summary>
/// Published when a backup fails.
/// </summary>
public sealed record BackupFailed : IntegrationEventBase
{
    /// <summary>
    /// Backup attempt ID.
    /// </summary>
    public required string BackupId { get; init; }

    /// <summary>
    /// Resource being backed up.
    /// </summary>
    public required string ResourceId { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// Whether retry is scheduled.
    /// </summary>
    public bool RetryScheduled { get; init; }

    /// <summary>
    /// Retry time if scheduled.
    /// </summary>
    public DateTimeOffset? RetryAt { get; init; }

    /// <summary>
    /// Attempt number.
    /// </summary>
    public required int AttemptNumber { get; init; }
}

/// <summary>
/// Published when a security vulnerability is detected.
/// </summary>
public sealed record SecurityVulnerabilityDetected : IntegrationEventBase
{
    /// <summary>
    /// Vulnerability ID.
    /// </summary>
    public required string VulnerabilityId { get; init; }

    /// <summary>
    /// Vulnerability type (dependency, configuration, injection).
    /// </summary>
    public required string VulnerabilityType { get; init; }

    /// <summary>
    /// Severity (low, medium, high, critical).
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Affected component.
    /// </summary>
    public required string AffectedComponent { get; init; }

    /// <summary>
    /// CVE if applicable.
    /// </summary>
    public string? CveId { get; init; }

    /// <summary>
    /// Remediation available.
    /// </summary>
    public bool RemediationAvailable { get; init; }

    /// <summary>
    /// Detection source (scan, audit, report).
    /// </summary>
    public required string DetectionSource { get; init; }
}
