namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user logs in.
/// </summary>
public sealed record UserLoggedIn : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Authentication provider used.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Client IP address (masked for privacy).
    /// </summary>
    public string? ClientIp { get; init; }

    /// <summary>
    /// User agent string.
    /// </summary>
    public string? UserAgent { get; init; }
}

/// <summary>
/// Published when a user logs out.
/// </summary>
public sealed record UserLoggedOut : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Whether this was an explicit logout or session expiry.
    /// </summary>
    public bool IsExplicit { get; init; } = true;
}

/// <summary>
/// Published when a password reset is requested.
/// </summary>
public sealed record PasswordResetRequested : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Email address (for notification).
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Expiry time for the reset token.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// Published when a password is successfully changed.
/// </summary>
public sealed record PasswordChanged : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Whether this was via reset flow or user-initiated.
    /// </summary>
    public bool ViaReset { get; init; }
}
