namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a data export is requested (GDPR/CCPA).
/// </summary>
public sealed record DataExportRequested : IntegrationEventBase
{
    /// <summary>
    /// Request ID.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Regulation basis (gdpr, ccpa, other).
    /// </summary>
    public required string RegulationType { get; init; }

    /// <summary>
    /// Data categories requested.
    /// </summary>
    public required string[] DataCategories { get; init; }

    /// <summary>
    /// Requested format (json, csv, pdf).
    /// </summary>
    public required string Format { get; init; }

    /// <summary>
    /// Deadline for completion.
    /// </summary>
    public required DateTimeOffset Deadline { get; init; }
}

/// <summary>
/// Published when a data export is completed.
/// </summary>
public sealed record DataExportCompleted : IntegrationEventBase
{
    /// <summary>
    /// Request ID.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Export file location (secure URL).
    /// </summary>
    public required string ExportLocation { get; init; }

    /// <summary>
    /// Export size in bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Expiry of download link.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// Processing time in seconds.
    /// </summary>
    public required int ProcessingTimeSeconds { get; init; }
}

/// <summary>
/// Published when a data deletion is requested (Right to Erasure).
/// </summary>
public sealed record DataDeletionRequested : IntegrationEventBase
{
    /// <summary>
    /// Request ID.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Regulation basis (gdpr, ccpa, user_request).
    /// </summary>
    public required string RegulationType { get; init; }

    /// <summary>
    /// Data categories to delete.
    /// </summary>
    public required string[] DataCategories { get; init; }

    /// <summary>
    /// Whether to anonymize instead of delete.
    /// </summary>
    public bool AnonymizeOnly { get; init; }

    /// <summary>
    /// Deadline for completion.
    /// </summary>
    public required DateTimeOffset Deadline { get; init; }

    /// <summary>
    /// Requester (user, admin, automated).
    /// </summary>
    public required string Requester { get; init; }
}

/// <summary>
/// Published when data deletion is completed.
/// </summary>
public sealed record DataDeletionCompleted : IntegrationEventBase
{
    /// <summary>
    /// Request ID.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Categories deleted.
    /// </summary>
    public required string[] DeletedCategories { get; init; }

    /// <summary>
    /// Records affected.
    /// </summary>
    public required int RecordsDeleted { get; init; }

    /// <summary>
    /// Services processed.
    /// </summary>
    public required string[] ServicesProcessed { get; init; }

    /// <summary>
    /// Processing time in seconds.
    /// </summary>
    public required int ProcessingTimeSeconds { get; init; }
}

/// <summary>
/// Published when content is archived.
/// </summary>
public sealed record ContentArchived : IntegrationEventBase
{
    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Content type (scenario, comment, message).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Owner account ID.
    /// </summary>
    public required string OwnerId { get; init; }

    /// <summary>
    /// Archive reason (age, policy, user_request).
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Archive location.
    /// </summary>
    public required string ArchiveLocation { get; init; }

    /// <summary>
    /// Retention period (days) before permanent deletion.
    /// </summary>
    public required int RetentionDays { get; init; }

    /// <summary>
    /// Whether content is restorable.
    /// </summary>
    public required bool IsRestorable { get; init; }
}

/// <summary>
/// Published when content is restored from archive.
/// </summary>
public sealed record ContentRestored : IntegrationEventBase
{
    /// <summary>
    /// Content ID.
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// Content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Owner account ID.
    /// </summary>
    public required string OwnerId { get; init; }

    /// <summary>
    /// Who restored it.
    /// </summary>
    public required string RestoredBy { get; init; }

    /// <summary>
    /// Days in archive.
    /// </summary>
    public required int DaysArchived { get; init; }
}

/// <summary>
/// Published when a retention policy is applied.
/// </summary>
public sealed record RetentionPolicyApplied : IntegrationEventBase
{
    /// <summary>
    /// Policy ID.
    /// </summary>
    public required string PolicyId { get; init; }

    /// <summary>
    /// Policy name.
    /// </summary>
    public required string PolicyName { get; init; }

    /// <summary>
    /// Data category affected.
    /// </summary>
    public required string DataCategory { get; init; }

    /// <summary>
    /// Records processed.
    /// </summary>
    public required int RecordsProcessed { get; init; }

    /// <summary>
    /// Records archived.
    /// </summary>
    public int RecordsArchived { get; init; }

    /// <summary>
    /// Records deleted.
    /// </summary>
    public int RecordsDeleted { get; init; }

    /// <summary>
    /// Execution time in seconds.
    /// </summary>
    public required int ExecutionTimeSeconds { get; init; }
}

/// <summary>
/// Published when user data is anonymized.
/// </summary>
public sealed record DataAnonymized : IntegrationEventBase
{
    /// <summary>
    /// Original account ID (now invalid).
    /// </summary>
    public required string OriginalAccountId { get; init; }

    /// <summary>
    /// Anonymous ID generated.
    /// </summary>
    public required string AnonymousId { get; init; }

    /// <summary>
    /// Data categories anonymized.
    /// </summary>
    public required string[] Categories { get; init; }

    /// <summary>
    /// Trigger (gdpr, ccpa, account_deletion).
    /// </summary>
    public required string Trigger { get; init; }

    /// <summary>
    /// Whether analytical data was retained.
    /// </summary>
    public bool AnalyticsRetained { get; init; }
}

/// <summary>
/// Published when a compliance audit log entry is created.
/// </summary>
public sealed record ComplianceAuditLog : IntegrationEventBase
{
    /// <summary>
    /// Audit entry ID.
    /// </summary>
    public required string AuditId { get; init; }

    /// <summary>
    /// Action type (access, modify, delete, export).
    /// </summary>
    public required string ActionType { get; init; }

    /// <summary>
    /// Actor (user, system, admin).
    /// </summary>
    public required string Actor { get; init; }

    /// <summary>
    /// Actor ID if applicable.
    /// </summary>
    public string? ActorId { get; init; }

    /// <summary>
    /// Resource accessed.
    /// </summary>
    public required string Resource { get; init; }

    /// <summary>
    /// Resource ID.
    /// </summary>
    public required string ResourceId { get; init; }

    /// <summary>
    /// Outcome (success, denied, error).
    /// </summary>
    public required string Outcome { get; init; }

    /// <summary>
    /// IP address if applicable.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Regulation context.
    /// </summary>
    public string? RegulationContext { get; init; }
}

/// <summary>
/// Published when consent is updated.
/// </summary>
public sealed record ConsentUpdated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Consent category (marketing, analytics, personalization).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Whether consent was granted.
    /// </summary>
    public required bool Granted { get; init; }

    /// <summary>
    /// Previous consent state.
    /// </summary>
    public bool? PreviousState { get; init; }

    /// <summary>
    /// Consent version/policy version.
    /// </summary>
    public required string PolicyVersion { get; init; }

    /// <summary>
    /// Collection method (banner, settings, registration).
    /// </summary>
    public required string CollectionMethod { get; init; }
}
