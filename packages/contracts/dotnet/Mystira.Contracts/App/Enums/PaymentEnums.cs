namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Type of checkout session
/// </summary>
public enum CheckoutType
{
    /// <summary>
    /// One-time payment
    /// </summary>
    OneTime,

    /// <summary>
    /// Subscription (recurring) payment
    /// </summary>
    Subscription
}

/// <summary>
/// Status of a checkout session
/// </summary>
public enum CheckoutResultStatus
{
    /// <summary>
    /// Checkout session created and awaiting payment
    /// </summary>
    Created,

    /// <summary>
    /// Payment is pending (processing)
    /// </summary>
    Pending,

    /// <summary>
    /// Checkout failed
    /// </summary>
    Failed
}

/// <summary>
/// Status of a payment transaction
/// </summary>
public enum PaymentResultStatus
{
    /// <summary>
    /// Payment is pending
    /// </summary>
    Pending,

    /// <summary>
    /// Payment is being processed
    /// </summary>
    Processing,

    /// <summary>
    /// Payment succeeded
    /// </summary>
    Succeeded,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed,

    /// <summary>
    /// Payment was cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Payment was fully refunded
    /// </summary>
    Refunded,

    /// <summary>
    /// Payment was partially refunded
    /// </summary>
    PartiallyRefunded
}

/// <summary>
/// Status of a refund operation
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// Refund is pending
    /// </summary>
    Pending,

    /// <summary>
    /// Refund succeeded
    /// </summary>
    Succeeded,

    /// <summary>
    /// Refund failed
    /// </summary>
    Failed
}

/// <summary>
/// Status of a subscription
/// </summary>
public enum SubscriptionResultStatus
{
    /// <summary>
    /// Subscription is active
    /// </summary>
    Active,

    /// <summary>
    /// Subscription is in trial period
    /// </summary>
    Trialing,

    /// <summary>
    /// Payment is past due
    /// </summary>
    PastDue,

    /// <summary>
    /// Subscription has been cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Subscription is unpaid
    /// </summary>
    Unpaid,

    /// <summary>
    /// Subscription setup is incomplete
    /// </summary>
    Incomplete
}

/// <summary>
/// Types of webhook events from payment providers
/// </summary>
public enum WebhookEventType
{
    /// <summary>
    /// Payment was successful
    /// </summary>
    PaymentSucceeded,

    /// <summary>
    /// Payment failed
    /// </summary>
    PaymentFailed,

    /// <summary>
    /// Payment was refunded
    /// </summary>
    PaymentRefunded,

    /// <summary>
    /// New subscription created
    /// </summary>
    SubscriptionCreated,

    /// <summary>
    /// Subscription was updated
    /// </summary>
    SubscriptionUpdated,

    /// <summary>
    /// Subscription was cancelled
    /// </summary>
    SubscriptionCancelled,

    /// <summary>
    /// Subscription was renewed
    /// </summary>
    SubscriptionRenewed,

    /// <summary>
    /// Checkout was completed successfully
    /// </summary>
    CheckoutCompleted,

    /// <summary>
    /// Checkout session expired
    /// </summary>
    CheckoutExpired
}
