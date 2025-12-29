namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a page/view is loaded.
/// </summary>
public sealed record PageViewed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID (null for anonymous).
    /// </summary>
    public string? AccountId { get; init; }

    /// <summary>
    /// Page/route identifier.
    /// </summary>
    public required string PageId { get; init; }

    /// <summary>
    /// Page name for display.
    /// </summary>
    public required string PageName { get; init; }

    /// <summary>
    /// Referrer page if any.
    /// </summary>
    public string? ReferrerPageId { get; init; }

    /// <summary>
    /// Session ID for tracking.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Device type (mobile, tablet, desktop).
    /// </summary>
    public string? DeviceType { get; init; }
}

/// <summary>
/// Published when a feature is used.
/// </summary>
public sealed record FeatureUsed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Feature identifier.
    /// </summary>
    public required string FeatureId { get; init; }

    /// <summary>
    /// Feature name.
    /// </summary>
    public required string FeatureName { get; init; }

    /// <summary>
    /// Feature category (navigation, gameplay, social, settings).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Context/location where used.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// Whether first time using this feature.
    /// </summary>
    public bool IsFirstUse { get; init; }
}

/// <summary>
/// Published when a user is assigned to an A/B test variant.
/// </summary>
public sealed record ABTestAssigned : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Experiment identifier.
    /// </summary>
    public required string ExperimentId { get; init; }

    /// <summary>
    /// Experiment name.
    /// </summary>
    public required string ExperimentName { get; init; }

    /// <summary>
    /// Assigned variant (control, variant_a, variant_b, etc.).
    /// </summary>
    public required string Variant { get; init; }

    /// <summary>
    /// Assignment method (random, targeted, deterministic).
    /// </summary>
    public required string AssignmentMethod { get; init; }
}

/// <summary>
/// Published when a user sees an A/B test variant.
/// </summary>
public sealed record ABTestExposure : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Experiment identifier.
    /// </summary>
    public required string ExperimentId { get; init; }

    /// <summary>
    /// Variant shown.
    /// </summary>
    public required string Variant { get; init; }

    /// <summary>
    /// Page/context where exposed.
    /// </summary>
    public required string Context { get; init; }
}

/// <summary>
/// Published when a conversion event occurs in an A/B test.
/// </summary>
public sealed record ABTestConversion : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Experiment identifier.
    /// </summary>
    public required string ExperimentId { get; init; }

    /// <summary>
    /// Variant the user was in.
    /// </summary>
    public required string Variant { get; init; }

    /// <summary>
    /// Conversion goal achieved.
    /// </summary>
    public required string ConversionGoal { get; init; }

    /// <summary>
    /// Conversion value if applicable.
    /// </summary>
    public decimal? ConversionValue { get; init; }
}

/// <summary>
/// Published when a performance metric is recorded.
/// </summary>
public sealed record PerformanceMetricRecorded : IntegrationEventBase
{
    /// <summary>
    /// Metric name (page_load, api_latency, render_time).
    /// </summary>
    public required string MetricName { get; init; }

    /// <summary>
    /// Metric value in milliseconds.
    /// </summary>
    public required double ValueMs { get; init; }

    /// <summary>
    /// Page/endpoint context.
    /// </summary>
    public required string Context { get; init; }

    /// <summary>
    /// The user's account ID if authenticated.
    /// </summary>
    public string? AccountId { get; init; }

    /// <summary>
    /// Device type.
    /// </summary>
    public string? DeviceType { get; init; }

    /// <summary>
    /// Network type (wifi, 4g, 5g).
    /// </summary>
    public string? NetworkType { get; init; }
}

/// <summary>
/// Published when an application error occurs.
/// </summary>
public sealed record ErrorOccurred : IntegrationEventBase
{
    /// <summary>
    /// Error ID for correlation.
    /// </summary>
    public required string ErrorId { get; init; }

    /// <summary>
    /// Error type/code.
    /// </summary>
    public required string ErrorType { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Where it occurred (frontend, backend, database).
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Stack trace hash for grouping.
    /// </summary>
    public string? StackTraceHash { get; init; }

    /// <summary>
    /// Affected user if any.
    /// </summary>
    public string? AccountId { get; init; }

    /// <summary>
    /// Severity (error, warning, critical).
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Whether it was handled/recovered.
    /// </summary>
    public bool WasHandled { get; init; }
}

/// <summary>
/// Published when a retention milestone is reached.
/// </summary>
public sealed record RetentionMilestone : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Milestone type (D1, D7, D14, D30, D60, D90).
    /// </summary>
    public required string MilestoneType { get; init; }

    /// <summary>
    /// Days since registration.
    /// </summary>
    public required int DaysSinceRegistration { get; init; }

    /// <summary>
    /// Sessions in this period.
    /// </summary>
    public required int SessionsInPeriod { get; init; }

    /// <summary>
    /// Total playtime in period (seconds).
    /// </summary>
    public long PlaytimeSeconds { get; init; }
}

/// <summary>
/// Published when user engagement score is calculated.
/// </summary>
public sealed record EngagementScoreCalculated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Engagement score (0-100).
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Previous score.
    /// </summary>
    public int PreviousScore { get; init; }

    /// <summary>
    /// Score components.
    /// </summary>
    public required Dictionary<string, int> Components { get; init; }

    /// <summary>
    /// Calculation period (daily, weekly).
    /// </summary>
    public required string Period { get; init; }

    /// <summary>
    /// User segment (active, at_risk, churned).
    /// </summary>
    public required string Segment { get; init; }
}

/// <summary>
/// Published when churn risk is detected.
/// </summary>
public sealed record ChurnRiskDetected : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Risk level (low, medium, high, critical).
    /// </summary>
    public required string RiskLevel { get; init; }

    /// <summary>
    /// Risk score (0-100).
    /// </summary>
    public required int RiskScore { get; init; }

    /// <summary>
    /// Risk factors identified.
    /// </summary>
    public required string[] RiskFactors { get; init; }

    /// <summary>
    /// Days since last session.
    /// </summary>
    public required int DaysSinceLastSession { get; init; }

    /// <summary>
    /// Recommended interventions.
    /// </summary>
    public string[]? RecommendedActions { get; init; }
}

/// <summary>
/// Published when a funnel step is completed.
/// </summary>
public sealed record FunnelStepCompleted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Funnel identifier.
    /// </summary>
    public required string FunnelId { get; init; }

    /// <summary>
    /// Funnel name.
    /// </summary>
    public required string FunnelName { get; init; }

    /// <summary>
    /// Step number.
    /// </summary>
    public required int StepNumber { get; init; }

    /// <summary>
    /// Step name.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// Time in previous step (seconds).
    /// </summary>
    public int TimeInPreviousStepSeconds { get; init; }

    /// <summary>
    /// Whether funnel was completed.
    /// </summary>
    public bool IsFinalStep { get; init; }
}
