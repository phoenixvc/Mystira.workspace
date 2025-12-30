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
    /// The target age group for this scenario.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// The difficulty level of the scenario.
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The expected duration of a session.
    /// </summary>
    public string? SessionLength { get; set; }

    /// <summary>
    /// Optional list of character archetypes available in this scenario.
    /// </summary>
    public List<string>? Archetypes { get; set; }

    /// <summary>
    /// The minimum recommended age for players.
    /// </summary>
    public int? MinimumAge { get; set; }

    /// <summary>
    /// Optional list of core moral compass axes explored in this scenario.
    /// </summary>
    public List<string>? CoreAxes { get; set; }

    /// <summary>
    /// The date and time when the scenario was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional music palette identifier for the scenario.
    /// </summary>
    public string? MusicPalette { get; set; }
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
    /// Whether the scenario is valid (no missing references).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of missing media references.
    /// </summary>
    public List<MissingReference> MissingMedia { get; set; } = new();

    /// <summary>
    /// List of missing character references.
    /// </summary>
    public List<MissingReference> MissingCharacters { get; set; } = new();

    /// <summary>
    /// List of broken scene connections.
    /// </summary>
    public List<MissingReference> BrokenSceneConnections { get; set; } = new();

    /// <summary>
    /// Total count of missing references.
    /// </summary>
    public int TotalMissingCount => MissingMedia.Count + MissingCharacters.Count + BrokenSceneConnections.Count;

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
    /// The unique identifier of the media.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of media (e.g., "image", "audio", "video").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the media.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The scene ID where this media is used.
    /// </summary>
    public string? SceneId { get; set; }

    /// <summary>
    /// Whether the media reference is valid.
    /// </summary>
    public bool IsValid { get; set; }
}

/// <summary>
/// Represents a character reference within a scenario.
/// </summary>
public record CharacterReference
{
    /// <summary>
    /// The unique identifier of the character.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The scene IDs where this character appears.
    /// </summary>
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// Whether the character reference is valid.
    /// </summary>
    public bool IsValid { get; set; }
}

/// <summary>
/// Represents a missing reference in a scenario.
/// </summary>
public record MissingReference
{
    /// <summary>
    /// The type of reference (e.g., "media", "character", "scene").
    /// </summary>
    public string ReferenceType { get; set; } = string.Empty;

    /// <summary>
    /// The ID that was referenced but not found.
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// The location where the reference was made (e.g., scene ID).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Additional context about the missing reference.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Suggested fix for the missing reference.
    /// </summary>
    public string? SuggestedFix { get; set; }
}
