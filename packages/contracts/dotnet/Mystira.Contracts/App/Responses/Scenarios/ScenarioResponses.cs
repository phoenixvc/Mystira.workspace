namespace Mystira.Contracts.App.Responses.Scenarios;

/// <summary>
/// Represents the current state of a scenario game.
/// </summary>
public enum ScenarioGameState
{
    /// <summary>
    /// The game has not been started yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The game is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The game has been completed.
    /// </summary>
    Completed
}

/// <summary>
/// Response containing a paginated list of scenarios.
/// </summary>
public record ScenarioListResponse
{
    /// <summary>
    /// The list of scenario summaries.
    /// </summary>
    public List<ScenarioSummary> Scenarios { get; set; } = new();

    /// <summary>
    /// The total number of scenarios matching the query.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Indicates if there are more pages available.
    /// </summary>
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Summary information for a scenario.
/// </summary>
public record ScenarioSummary
{
    /// <summary>
    /// The unique identifier of the scenario.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The title of the scenario.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scenario.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// The difficulty level of the scenario.
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// The expected session length in minutes.
    /// </summary>
    public int SessionLength { get; set; }

    /// <summary>
    /// List of character archetypes available in this scenario.
    /// </summary>
    public List<string> Archetypes { get; set; } = new();

    /// <summary>
    /// The minimum recommended age for players.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// The target age group for this scenario.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// List of core moral compass axes explored in this scenario.
    /// </summary>
    public List<string> CoreAxes { get; set; } = new();

    /// <summary>
    /// The date and time when the scenario was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional URL or identifier for the scenario's cover image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional music palette identifier for the scenario.
    /// </summary>
    public string? MusicPalette { get; set; }

    /// <summary>
    /// Whether this scenario is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Optional URL for the scenario's thumbnail image.
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// Response containing scenarios with their game state information.
/// </summary>
public record ScenarioGameStateResponse
{
    /// <summary>
    /// The list of scenarios with game state information.
    /// </summary>
    public List<ScenarioWithGameState> Scenarios { get; set; } = new();

    /// <summary>
    /// The total number of scenarios.
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Scenario information including current game state.
/// </summary>
public record ScenarioWithGameState
{
    /// <summary>
    /// The unique identifier of the scenario.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the scenario.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scenario.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The current game state (NotStarted, InProgress, Completed).
    /// </summary>
    public string GameState { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the scenario was last played.
    /// </summary>
    public DateTime? LastPlayedAt { get; set; }

    /// <summary>
    /// The target age group for this scenario.
    /// </summary>
    public string? AgeGroup { get; set; }

    /// <summary>
    /// The difficulty level of the scenario.
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// The expected duration of a session.
    /// </summary>
    public string? SessionLength { get; set; }

    /// <summary>
    /// Optional list of core moral compass axes explored in this scenario.
    /// </summary>
    public List<string>? CoreAxes { get; set; }

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Optional list of character archetypes available in this scenario.
    /// </summary>
    public List<string>? Archetypes { get; set; }

    /// <summary>
    /// The number of times this scenario has been played.
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// Optional URL or identifier for the scenario's cover image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Whether this scenario is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Optional URL for the scenario's thumbnail image.
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// Response containing scenario reference validation results.
/// </summary>
public record ScenarioReferenceValidation
{
    /// <summary>
    /// The unique identifier of the validated scenario.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the validated scenario.
    /// </summary>
    public string ScenarioTitle { get; set; } = string.Empty;

    /// <summary>
    /// Whether the scenario is valid (no missing references).
    /// </summary>
    public bool IsValid => MissingReferences.Count == 0;

    /// <summary>
    /// List of media references in the scenario.
    /// </summary>
    public List<MediaReference> MediaReferences { get; set; } = new();

    /// <summary>
    /// List of character references in the scenario.
    /// </summary>
    public List<CharacterReference> CharacterReferences { get; set; } = new();

    /// <summary>
    /// List of missing references found during validation.
    /// </summary>
    public List<MissingReference> MissingReferences { get; set; } = new();

    /// <summary>
    /// Validation timestamp.
    /// </summary>
    public DateTime ValidatedAt { get; set; }
}

/// <summary>
/// Represents a media reference within a scenario.
/// </summary>
public record MediaReference
{
    /// <summary>
    /// The ID of the scene containing this reference.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the scene containing this reference.
    /// </summary>
    public string SceneTitle { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the media.
    /// </summary>
    public string MediaId { get; set; } = string.Empty;

    /// <summary>
    /// The type of media (e.g., "image", "audio", "video").
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Whether the referenced media exists.
    /// </summary>
    public bool MediaExists { get; set; }

    /// <summary>
    /// Whether the media has associated metadata.
    /// </summary>
    public bool HasMetadata { get; set; }
}

/// <summary>
/// Represents a character reference within a scenario.
/// </summary>
public record CharacterReference
{
    /// <summary>
    /// The ID of the scene containing this reference.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the scene containing this reference.
    /// </summary>
    public string SceneTitle { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the character.
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the character.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the referenced character exists.
    /// </summary>
    public bool CharacterExists { get; set; }

    /// <summary>
    /// Whether the character has associated metadata.
    /// </summary>
    public bool HasMetadata { get; set; }
}

/// <summary>
/// Represents a missing reference in a scenario.
/// </summary>
public record MissingReference
{
    /// <summary>
    /// The ID that was referenced but not found.
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// The type of reference (e.g., "media", "character", "scene").
    /// </summary>
    public string ReferenceType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the scene where the reference was made.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// The title of the scene where the reference was made.
    /// </summary>
    public string SceneTitle { get; set; } = string.Empty;

    /// <summary>
    /// The type of issue (e.g., "missing", "invalid", "orphaned").
    /// </summary>
    public string IssueType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional context about the missing reference.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Suggested fix for the missing reference.
    /// </summary>
    public string? SuggestedFix { get; set; }
}
