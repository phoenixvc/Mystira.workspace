using Mystira.Contracts.App.Enums;

namespace Mystira.Contracts.App.Responses.Payments;

/// <summary>
/// Result of a checkout session creation
/// </summary>
public record CheckoutResult
{
    /// <summary>
    /// Unique checkout session ID
    /// </summary>
    public required string CheckoutId { get; init; }

    /// <summary>
    /// Status of the checkout session
    /// </summary>
    public required CheckoutResultStatus Status { get; init; }

    /// <summary>
    /// URL to redirect user for payment
    /// </summary>
    public string? RedirectUrl { get; init; }

    /// <summary>
    /// Form data for embedded checkout (if applicable)
    /// </summary>
    public string? FormData { get; init; }

    /// <summary>
    /// Error message if checkout failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// When the checkout session expires
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Result of a payment transaction
/// </summary>
public record PaymentResult
{
    /// <summary>
    /// Unique transaction ID
    /// </summary>
    public required string TransactionId { get; init; }

    /// <summary>
    /// Status of the payment
    /// </summary>
    public required PaymentResultStatus Status { get; init; }

    /// <summary>
    /// Amount charged
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if payment failed
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// When the payment was processed
    /// </summary>
    public DateTime ProcessedAt { get; init; }

    /// <summary>
    /// Metadata associated with the payment
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Payment status details
/// </summary>
public record PaymentStatus
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    public required string TransactionId { get; init; }

    /// <summary>
    /// Current status
    /// </summary>
    public required PaymentResultStatus Status { get; init; }

    /// <summary>
    /// Amount
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// When the payment was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the payment was completed (if applicable)
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Failure reason if payment failed
    /// </summary>
    public string? FailureReason { get; init; }
}

/// <summary>
/// Result of a refund operation
/// </summary>
public record RefundResult
{
    /// <summary>
    /// Unique refund ID
    /// </summary>
    public required string RefundId { get; init; }

    /// <summary>
    /// Original transaction ID
    /// </summary>
    public required string TransactionId { get; init; }

    /// <summary>
    /// Status of the refund
    /// </summary>
    public required RefundStatus Status { get; init; }

    /// <summary>
    /// Amount refunded
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Error message if refund failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// When the refund was processed
    /// </summary>
    public DateTime ProcessedAt { get; init; }
}

/// <summary>
/// Result of subscription creation
/// </summary>
public record SubscriptionResult
{
    /// <summary>
    /// Unique subscription ID
    /// </summary>
    public required string SubscriptionId { get; init; }

    /// <summary>
    /// Status of the subscription
    /// </summary>
    public required SubscriptionResultStatus Status { get; init; }

    /// <summary>
    /// Plan ID
    /// </summary>
    public required string PlanId { get; init; }

    /// <summary>
    /// Start of current billing period
    /// </summary>
    public DateTime? CurrentPeriodStart { get; init; }

    /// <summary>
    /// End of current billing period
    /// </summary>
    public DateTime? CurrentPeriodEnd { get; init; }

    /// <summary>
    /// When trial ends (if applicable)
    /// </summary>
    public DateTime? TrialEnd { get; init; }

    /// <summary>
    /// Error message if subscription creation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Subscription status details
/// </summary>
public record SubscriptionStatus
{
    /// <summary>
    /// Subscription ID
    /// </summary>
    public required string SubscriptionId { get; init; }

    /// <summary>
    /// Current status
    /// </summary>
    public required SubscriptionResultStatus Status { get; init; }

    /// <summary>
    /// Plan ID
    /// </summary>
    public required string PlanId { get; init; }

    /// <summary>
    /// Start of current billing period
    /// </summary>
    public DateTime CurrentPeriodStart { get; init; }

    /// <summary>
    /// End of current billing period
    /// </summary>
    public DateTime CurrentPeriodEnd { get; init; }

    /// <summary>
    /// When subscription was cancelled (if applicable)
    /// </summary>
    public DateTime? CancelledAt { get; init; }

    /// <summary>
    /// When subscription will be cancelled (if scheduled)
    /// </summary>
    public DateTime? CancelAt { get; init; }

    /// <summary>
    /// Whether cancellation is scheduled for end of period
    /// </summary>
    public bool CancelAtPeriodEnd { get; init; }
}

/// <summary>
/// Result of subscription cancellation
/// </summary>
public record SubscriptionCancellationResult
{
    /// <summary>
    /// Subscription ID
    /// </summary>
    public required string SubscriptionId { get; init; }

    /// <summary>
    /// Whether cancellation was successful
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// When cancellation takes effect
    /// </summary>
    public DateTime? EffectiveDate { get; init; }

    /// <summary>
    /// Error message if cancellation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Webhook event from payment provider
/// </summary>
public record WebhookEvent
{
    /// <summary>
    /// Unique event ID
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Type of event
    /// </summary>
    public required WebhookEventType Type { get; init; }

    /// <summary>
    /// Transaction ID associated with the event
    /// </summary>
    public required string TransactionId { get; init; }

    /// <summary>
    /// Subscription ID (if subscription-related event)
    /// </summary>
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// Account ID of the user
    /// </summary>
    public string? AccountId { get; init; }

    /// <summary>
    /// Amount (if payment-related event)
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Payment status (if payment-related event)
    /// </summary>
    public PaymentResultStatus? PaymentStatus { get; init; }

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Raw data from the payment provider
    /// </summary>
    public Dictionary<string, object>? RawData { get; init; }
}
