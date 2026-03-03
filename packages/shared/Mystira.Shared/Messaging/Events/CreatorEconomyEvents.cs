namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user becomes a creator.
/// </summary>
public sealed record CreatorProfileCreated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Creator ID (may differ from account).
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Creator display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Initial tier (new, verified, partner).
    /// </summary>
    public required string Tier { get; init; }
}

/// <summary>
/// Published when a creator is verified.
/// </summary>
public sealed record CreatorVerified : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Verification type (identity, email, social).
    /// </summary>
    public required string VerificationType { get; init; }

    /// <summary>
    /// Verified by (system, admin).
    /// </summary>
    public required string VerifiedBy { get; init; }

    /// <summary>
    /// Badge/status granted.
    /// </summary>
    public string? BadgeGranted { get; init; }
}

/// <summary>
/// Published when creator enables monetization on content.
/// </summary>
public sealed record ContentMonetizationEnabled : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Content type (scenario, bundle, item).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Pricing model (free, premium, freemium, subscription).
    /// </summary>
    public required string PricingModel { get; init; }

    /// <summary>
    /// Price in cents (if applicable).
    /// </summary>
    public int? PriceCents { get; init; }

    /// <summary>
    /// Currency.
    /// </summary>
    public string? Currency { get; init; }
}

/// <summary>
/// Published when creator earnings are calculated.
/// </summary>
public sealed record CreatorEarningsCalculated : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Calculation period (daily, weekly, monthly).
    /// </summary>
    public required string Period { get; init; }

    /// <summary>
    /// Period start date.
    /// </summary>
    public required DateTimeOffset PeriodStart { get; init; }

    /// <summary>
    /// Period end date.
    /// </summary>
    public required DateTimeOffset PeriodEnd { get; init; }

    /// <summary>
    /// Gross earnings in cents.
    /// </summary>
    public required int GrossEarningsCents { get; init; }

    /// <summary>
    /// Platform fee in cents.
    /// </summary>
    public required int PlatformFeeCents { get; init; }

    /// <summary>
    /// Net earnings in cents.
    /// </summary>
    public required int NetEarningsCents { get; init; }

    /// <summary>
    /// Currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Transaction count.
    /// </summary>
    public required int TransactionCount { get; init; }
}

/// <summary>
/// Published when creator requests payout.
/// </summary>
public sealed record CreatorPayoutRequested : IntegrationEventBase
{
    /// <summary>
    /// Payout request ID.
    /// </summary>
    public required string PayoutId { get; init; }

    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Amount in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Payout method (bank, paypal, stripe).
    /// </summary>
    public required string PayoutMethod { get; init; }

    /// <summary>
    /// Estimated processing time (days).
    /// </summary>
    public required int EstimatedProcessingDays { get; init; }
}

/// <summary>
/// Published when creator payout is processed.
/// </summary>
public sealed record CreatorPayoutProcessed : IntegrationEventBase
{
    /// <summary>
    /// Payout ID.
    /// </summary>
    public required string PayoutId { get; init; }

    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Amount in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// External transaction ID.
    /// </summary>
    public required string ExternalTransactionId { get; init; }

    /// <summary>
    /// Processing fee in cents.
    /// </summary>
    public int ProcessingFeeCents { get; init; }

    /// <summary>
    /// Net amount received.
    /// </summary>
    public required int NetAmountCents { get; init; }
}

/// <summary>
/// Published when creator payout fails.
/// </summary>
public sealed record CreatorPayoutFailed : IntegrationEventBase
{
    /// <summary>
    /// Payout ID.
    /// </summary>
    public required string PayoutId { get; init; }

    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Amount in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Failure reason.
    /// </summary>
    public required string FailureReason { get; init; }

    /// <summary>
    /// Whether retry is scheduled.
    /// </summary>
    public bool RetryScheduled { get; init; }

    /// <summary>
    /// Retry time if scheduled.
    /// </summary>
    public DateTimeOffset? RetryAt { get; init; }
}

/// <summary>
/// Published when revenue is distributed to multiple creators.
/// </summary>
public sealed record RevenueSharingDistributed : IntegrationEventBase
{
    /// <summary>
    /// Distribution ID.
    /// </summary>
    public required string DistributionId { get; init; }

    /// <summary>
    /// Content ID that generated revenue.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Total revenue in cents.
    /// </summary>
    public required int TotalRevenueCents { get; init; }

    /// <summary>
    /// Currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Number of creators receiving share.
    /// </summary>
    public required int CreatorCount { get; init; }

    /// <summary>
    /// Distribution breakdown (creator_id -> amount_cents).
    /// </summary>
    public required Dictionary<string, int> Distribution { get; init; }
}

/// <summary>
/// Published when creator reaches a follower milestone.
/// </summary>
public sealed record CreatorFollowerMilestone : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Milestone reached (100, 1000, 10000, etc.).
    /// </summary>
    public required int Milestone { get; init; }

    /// <summary>
    /// Current follower count.
    /// </summary>
    public required int CurrentFollowers { get; init; }

    /// <summary>
    /// Previous milestone.
    /// </summary>
    public int PreviousMilestone { get; init; }

    /// <summary>
    /// Reward granted if any.
    /// </summary>
    public string? RewardGranted { get; init; }
}

/// <summary>
/// Published when creator reaches an earnings milestone.
/// </summary>
public sealed record CreatorEarningsMilestone : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Milestone amount in cents (10000 = $100).
    /// </summary>
    public required int MilestoneAmountCents { get; init; }

    /// <summary>
    /// Currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Total lifetime earnings.
    /// </summary>
    public required int TotalEarningsCents { get; init; }

    /// <summary>
    /// Badge/status unlocked if any.
    /// </summary>
    public string? BadgeUnlocked { get; init; }
}

/// <summary>
/// Published when creator tier changes.
/// </summary>
public sealed record CreatorTierChanged : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Previous tier.
    /// </summary>
    public required string FromTier { get; init; }

    /// <summary>
    /// New tier.
    /// </summary>
    public required string ToTier { get; init; }

    /// <summary>
    /// Reason for change.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// New revenue share percentage.
    /// </summary>
    public required decimal NewRevenueSharePercent { get; init; }

    /// <summary>
    /// New benefits unlocked.
    /// </summary>
    public string[]? NewBenefits { get; init; }
}

/// <summary>
/// Published when creator submits tax form.
/// </summary>
public sealed record CreatorTaxFormSubmitted : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Form type (W9, W8BEN, etc.).
    /// </summary>
    public required string FormType { get; init; }

    /// <summary>
    /// Tax year.
    /// </summary>
    public required int TaxYear { get; init; }

    /// <summary>
    /// Status (submitted, verified, rejected).
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Submission ID.
    /// </summary>
    public required string SubmissionId { get; init; }
}

/// <summary>
/// Published when creator views their analytics dashboard.
/// </summary>
public sealed record CreatorAnalyticsViewed : IntegrationEventBase
{
    /// <summary>
    /// The creator ID.
    /// </summary>
    public required string CreatorId { get; init; }

    /// <summary>
    /// Dashboard section viewed.
    /// </summary>
    public required string Section { get; init; }

    /// <summary>
    /// Time period selected.
    /// </summary>
    public required string TimePeriod { get; init; }

    /// <summary>
    /// Session duration in seconds.
    /// </summary>
    public int SessionDurationSeconds { get; init; }
}
