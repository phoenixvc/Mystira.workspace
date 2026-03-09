namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when content is reported by a user.
/// </summary>
public sealed record ContentReported : IntegrationEventBase
{
    /// <summary>
    /// The report ID.
    /// </summary>
    public required string ReportId { get; init; }

    /// <summary>
    /// The reporter's account ID.
    /// </summary>
    public required string ReporterAccountId { get; init; }

    /// <summary>
    /// Content type (scenario, comment, review, profile).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Content owner's account ID.
    /// </summary>
    public required string ContentOwnerId { get; init; }

    /// <summary>
    /// Report category (spam, harassment, inappropriate, copyright, other).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Whether additional details were provided.
    /// </summary>
    public bool HasDetails { get; init; }
}

/// <summary>
/// Published when content is moderated (action taken).
/// </summary>
public sealed record ContentModerated : IntegrationEventBase
{
    /// <summary>
    /// The moderation action ID.
    /// </summary>
    public required string ActionId { get; init; }

    /// <summary>
    /// Content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Content owner's account ID.
    /// </summary>
    public required string ContentOwnerId { get; init; }

    /// <summary>
    /// Moderator's account ID.
    /// </summary>
    public required string ModeratorId { get; init; }

    /// <summary>
    /// Action taken (approved, removed, hidden, edited).
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Reason for moderation.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Related report IDs.
    /// </summary>
    public string[]? RelatedReportIds { get; init; }
}

/// <summary>
/// Published when a user receives a warning.
/// </summary>
public sealed record UserWarned : IntegrationEventBase
{
    /// <summary>
    /// The warning ID.
    /// </summary>
    public required string WarningId { get; init; }

    /// <summary>
    /// The warned user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Moderator's account ID.
    /// </summary>
    public required string ModeratorId { get; init; }

    /// <summary>
    /// Warning category.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Warning severity (minor, moderate, severe).
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Warning count for this user.
    /// </summary>
    public int WarningCount { get; init; }

    /// <summary>
    /// Related content ID if applicable.
    /// </summary>
    public string? RelatedContentId { get; init; }
}

/// <summary>
/// Published when a user is suspended.
/// </summary>
public sealed record UserSuspended : IntegrationEventBase
{
    /// <summary>
    /// The suspension ID.
    /// </summary>
    public required string SuspensionId { get; init; }

    /// <summary>
    /// The suspended user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Moderator's account ID.
    /// </summary>
    public required string ModeratorId { get; init; }

    /// <summary>
    /// Reason for suspension.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Suspension duration in hours (null = permanent until appeal).
    /// </summary>
    public int? DurationHours { get; init; }

    /// <summary>
    /// When suspension ends.
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>
    /// Whether user can appeal.
    /// </summary>
    public bool CanAppeal { get; init; } = true;
}

/// <summary>
/// Published when a user is banned.
/// </summary>
public sealed record UserBanned : IntegrationEventBase
{
    /// <summary>
    /// The ban ID.
    /// </summary>
    public required string BanId { get; init; }

    /// <summary>
    /// The banned user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Moderator's account ID.
    /// </summary>
    public required string ModeratorId { get; init; }

    /// <summary>
    /// Reason for ban.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Whether this is a permanent ban.
    /// </summary>
    public bool IsPermanent { get; init; } = true;

    /// <summary>
    /// Ban end date if not permanent.
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>
    /// Whether user can appeal.
    /// </summary>
    public bool CanAppeal { get; init; }
}

/// <summary>
/// Published when a user submits an appeal.
/// </summary>
public sealed record AppealSubmitted : IntegrationEventBase
{
    /// <summary>
    /// The appeal ID.
    /// </summary>
    public required string AppealId { get; init; }

    /// <summary>
    /// The appellant's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Type of action being appealed (warning, suspension, ban, content_removal).
    /// </summary>
    public required string AppealType { get; init; }

    /// <summary>
    /// Related action ID (warning ID, suspension ID, ban ID, etc.).
    /// </summary>
    public required string RelatedActionId { get; init; }
}

/// <summary>
/// Published when an appeal is resolved.
/// </summary>
public sealed record AppealResolved : IntegrationEventBase
{
    /// <summary>
    /// The appeal ID.
    /// </summary>
    public required string AppealId { get; init; }

    /// <summary>
    /// The appellant's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Moderator who resolved the appeal.
    /// </summary>
    public required string ModeratorId { get; init; }

    /// <summary>
    /// Outcome (approved, denied, partial).
    /// </summary>
    public required string Outcome { get; init; }

    /// <summary>
    /// Resolution notes.
    /// </summary>
    public string? Notes { get; init; }
}
