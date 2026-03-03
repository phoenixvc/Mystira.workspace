namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a subscription is started.
/// </summary>
public sealed record SubscriptionStarted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The subscription ID.
    /// </summary>
    public required string SubscriptionId { get; init; }

    /// <summary>
    /// Subscription plan ID.
    /// </summary>
    public required string PlanId { get; init; }

    /// <summary>
    /// Plan name (e.g., "Premium", "Pro").
    /// </summary>
    public required string PlanName { get; init; }

    /// <summary>
    /// Billing interval (monthly, yearly).
    /// </summary>
    public required string BillingInterval { get; init; }

    /// <summary>
    /// Amount in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency code (USD, EUR, etc.).
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Whether this is a trial.
    /// </summary>
    public bool IsTrial { get; init; }

    /// <summary>
    /// Trial end date if applicable.
    /// </summary>
    public DateTimeOffset? TrialEndsAt { get; init; }
}

/// <summary>
/// Published when a subscription is renewed.
/// </summary>
public sealed record SubscriptionRenewed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The subscription ID.
    /// </summary>
    public required string SubscriptionId { get; init; }

    /// <summary>
    /// The payment ID for this renewal.
    /// </summary>
    public required string PaymentId { get; init; }

    /// <summary>
    /// Amount charged in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Next billing date.
    /// </summary>
    public required DateTimeOffset NextBillingDate { get; init; }
}

/// <summary>
/// Published when a subscription is cancelled.
/// </summary>
public sealed record SubscriptionCancelled : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The subscription ID.
    /// </summary>
    public required string SubscriptionId { get; init; }

    /// <summary>
    /// Cancellation reason if provided.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Whether this was an immediate cancellation or end-of-period.
    /// </summary>
    public bool IsImmediate { get; init; }

    /// <summary>
    /// When access ends.
    /// </summary>
    public required DateTimeOffset AccessEndsAt { get; init; }

    /// <summary>
    /// Whether user requested a refund.
    /// </summary>
    public bool RefundRequested { get; init; }
}

/// <summary>
/// Published when a payment succeeds.
/// </summary>
public sealed record PaymentSucceeded : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The payment ID.
    /// </summary>
    public required string PaymentId { get; init; }

    /// <summary>
    /// Amount in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Payment method type (card, paypal, etc.).
    /// </summary>
    public required string PaymentMethod { get; init; }

    /// <summary>
    /// What was purchased (subscription, content, etc.).
    /// </summary>
    public required string ProductType { get; init; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public required string ProductId { get; init; }

    /// <summary>
    /// External payment provider transaction ID.
    /// </summary>
    public string? ExternalTransactionId { get; init; }
}

/// <summary>
/// Published when a payment fails.
/// </summary>
public sealed record PaymentFailed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The payment ID.
    /// </summary>
    public required string PaymentId { get; init; }

    /// <summary>
    /// Amount attempted in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Failure reason code.
    /// </summary>
    public required string FailureCode { get; init; }

    /// <summary>
    /// Human-readable failure message.
    /// </summary>
    public required string FailureMessage { get; init; }

    /// <summary>
    /// Whether retry is possible.
    /// </summary>
    public bool IsRetryable { get; init; }

    /// <summary>
    /// Retry attempt number.
    /// </summary>
    public int RetryAttempt { get; init; }
}

/// <summary>
/// Published when a refund is processed.
/// </summary>
public sealed record RefundProcessed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The refund ID.
    /// </summary>
    public required string RefundId { get; init; }

    /// <summary>
    /// Original payment ID.
    /// </summary>
    public required string OriginalPaymentId { get; init; }

    /// <summary>
    /// Refund amount in cents.
    /// </summary>
    public required int AmountCents { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Refund reason.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Whether this was a full or partial refund.
    /// </summary>
    public bool IsPartial { get; init; }
}

/// <summary>
/// Published when premium content is unlocked.
/// </summary>
public sealed record PremiumContentUnlocked : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Content type (scenario, chapter, feature).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// How it was unlocked (purchase, subscription, reward).
    /// </summary>
    public required string UnlockMethod { get; init; }

    /// <summary>
    /// Payment ID if purchased.
    /// </summary>
    public string? PaymentId { get; init; }
}
