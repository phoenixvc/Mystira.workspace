namespace Mystira.Domain.Enums;

/// <summary>
/// Represents the status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment was refunded.
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Payment was cancelled.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Payment is disputed.
    /// </summary>
    Disputed = 6
}

/// <summary>
/// Represents the type of payment.
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Subscription payment.
    /// </summary>
    Subscription = 0,

    /// <summary>
    /// One-time purchase.
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Royalty payout to contributor.
    /// </summary>
    RoyaltyPayout = 2,

    /// <summary>
    /// Refund.
    /// </summary>
    Refund = 3
}

/// <summary>
/// Represents the payment method used.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Credit or debit card.
    /// </summary>
    Card = 0,

    /// <summary>
    /// Bank transfer.
    /// </summary>
    BankTransfer = 1,

    /// <summary>
    /// PayPal.
    /// </summary>
    PayPal = 2,

    /// <summary>
    /// Apple Pay.
    /// </summary>
    ApplePay = 3,

    /// <summary>
    /// Google Pay.
    /// </summary>
    GooglePay = 4,

    /// <summary>
    /// Cryptocurrency.
    /// </summary>
    Crypto = 5
}
