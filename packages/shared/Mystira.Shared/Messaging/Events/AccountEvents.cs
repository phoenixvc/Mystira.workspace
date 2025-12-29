namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a new user account is created.
/// </summary>
public sealed record AccountCreated : IntegrationEventBase
{
    /// <summary>
    /// The unique account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The authentication provider (e.g., "entra", "google").
    /// </summary>
    public string? Provider { get; init; }
}

/// <summary>
/// Published when a user account is updated.
/// </summary>
public sealed record AccountUpdated : IntegrationEventBase
{
    /// <summary>
    /// The unique account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// List of fields that were updated.
    /// </summary>
    public required IReadOnlyList<string> UpdatedFields { get; init; }
}

/// <summary>
/// Published when a user account is deleted (soft or hard).
/// </summary>
public sealed record AccountDeleted : IntegrationEventBase
{
    /// <summary>
    /// The unique account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Whether this was a soft delete (can be restored).
    /// </summary>
    public bool IsSoftDelete { get; init; } = true;
}
