using Mystira.Contracts.App.Enums;

namespace Mystira.Contracts.App.Requests.Payments;

/// <summary>
/// Request to create a checkout session
/// </summary>
public record CheckoutRequest
{
    /// <summary>
    /// The user's account ID
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// User's email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Product/plan ID being purchased
    /// </summary>
    public required string ProductId { get; init; }

    /// <summary>
    /// Amount to charge
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code (e.g., ZAR, USD)
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Description of the purchase
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// URL to redirect to on success
    /// </summary>
    public string? SuccessUrl { get; init; }

    /// <summary>
    /// URL to redirect to on cancellation
    /// </summary>
    public string? CancelUrl { get; init; }

    /// <summary>
    /// URL for payment webhook notifications
    /// </summary>
    public string? WebhookUrl { get; init; }

    /// <summary>
    /// Additional metadata to store with the payment
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Type of checkout (one-time or subscription)
    /// </summary>
    public CheckoutType Type { get; init; } = CheckoutType.OneTime;
}

/// <summary>
/// Request to process a direct payment
/// </summary>
public record PaymentRequest
{
    /// <summary>
    /// The user's account ID
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Amount to charge
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code (e.g., ZAR, USD)
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Token for the stored payment method
    /// </summary>
    public required string PaymentMethodToken { get; init; }

    /// <summary>
    /// Description of the payment
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Additional metadata to store with the payment
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Request to refund a payment
/// </summary>
public record RefundRequest
{
    /// <summary>
    /// ID of the transaction to refund
    /// </summary>
    public required string TransactionId { get; init; }

    /// <summary>
    /// Amount to refund (null = full refund)
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Reason for the refund
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Request to create a subscription
/// </summary>
public record SubscriptionRequest
{
    /// <summary>
    /// The user's account ID
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// User's email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Subscription plan ID
    /// </summary>
    public required string PlanId { get; init; }

    /// <summary>
    /// Token for the payment method to use
    /// </summary>
    public string? PaymentMethodToken { get; init; }

    /// <summary>
    /// End date for trial period (if applicable)
    /// </summary>
    public DateTime? TrialEndDate { get; init; }

    /// <summary>
    /// Additional metadata to store with the subscription
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}
