namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a notification is sent.
/// </summary>
public sealed record NotificationSent : IntegrationEventBase
{
    /// <summary>
    /// The notification ID.
    /// </summary>
    public required string NotificationId { get; init; }

    /// <summary>
    /// The recipient's account ID.
    /// </summary>
    public required string RecipientId { get; init; }

    /// <summary>
    /// Notification type (email, push, in-app).
    /// </summary>
    public required string NotificationType { get; init; }

    /// <summary>
    /// Template or category.
    /// </summary>
    public required string Template { get; init; }
}

/// <summary>
/// Published when an email is sent.
/// </summary>
public sealed record EmailSent : IntegrationEventBase
{
    /// <summary>
    /// The email ID for tracking.
    /// </summary>
    public required string EmailId { get; init; }

    /// <summary>
    /// The recipient's email (masked).
    /// </summary>
    public required string RecipientEmail { get; init; }

    /// <summary>
    /// Email template used.
    /// </summary>
    public required string Template { get; init; }

    /// <summary>
    /// Subject line.
    /// </summary>
    public required string Subject { get; init; }
}

/// <summary>
/// Published when a notification delivery fails.
/// </summary>
public sealed record NotificationFailed : IntegrationEventBase
{
    /// <summary>
    /// The notification ID.
    /// </summary>
    public required string NotificationId { get; init; }

    /// <summary>
    /// The recipient's account ID.
    /// </summary>
    public required string RecipientId { get; init; }

    /// <summary>
    /// Error reason.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Whether retry is possible.
    /// </summary>
    public bool IsRetryable { get; init; }
}
