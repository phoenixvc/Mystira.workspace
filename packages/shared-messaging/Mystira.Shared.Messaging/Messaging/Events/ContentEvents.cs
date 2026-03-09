namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a new scenario is created.
/// </summary>
public sealed record ScenarioCreated : IntegrationEventBase
{
    /// <summary>
    /// The unique scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The scenario title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The author's account ID.
    /// </summary>
    public required string AuthorId { get; init; }

    /// <summary>
    /// Whether the scenario is published.
    /// </summary>
    public bool IsPublished { get; init; }
}

/// <summary>
/// Published when a scenario is updated.
/// </summary>
public sealed record ScenarioUpdated : IntegrationEventBase
{
    /// <summary>
    /// The unique scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// List of fields that were updated.
    /// </summary>
    public required IReadOnlyList<string> UpdatedFields { get; init; }

    /// <summary>
    /// New version number if applicable.
    /// </summary>
    public int? Version { get; init; }
}

/// <summary>
/// Published when a scenario is published.
/// </summary>
public sealed record ScenarioPublished : IntegrationEventBase
{
    /// <summary>
    /// The unique scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The scenario title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The author's account ID.
    /// </summary>
    public required string AuthorId { get; init; }
}

/// <summary>
/// Published when a scenario is unpublished.
/// </summary>
public sealed record ScenarioUnpublished : IntegrationEventBase
{
    /// <summary>
    /// The unique scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Reason for unpublishing.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Published when media is uploaded.
/// </summary>
public sealed record MediaUploaded : IntegrationEventBase
{
    /// <summary>
    /// The unique media ID.
    /// </summary>
    public required string MediaId { get; init; }

    /// <summary>
    /// The uploader's account ID.
    /// </summary>
    public required string UploaderId { get; init; }

    /// <summary>
    /// The MIME type of the media.
    /// </summary>
    public required string MimeType { get; init; }

    /// <summary>
    /// The file size in bytes.
    /// </summary>
    public required long SizeBytes { get; init; }
}
