using Mystira.Contracts.App.Enums;

namespace Mystira.Contracts.App.Requests.Scenarios;

/// <summary>
/// Request to create a new scenario.
/// </summary>
public record CreateScenarioRequest
{
    /// <summary>
    /// The title of the scenario.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scenario content and objectives.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The difficulty level of the scenario.
    /// </summary>
    public DifficultyLevel Difficulty { get; set; }

    /// <summary>
    /// The expected duration of a session.
    /// </summary>
    public SessionLength SessionLength { get; set; }

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Optional list of character archetypes available in this scenario.
    /// </summary>
    public List<string>? Archetypes { get; set; }

    /// <summary>
    /// The target age group for this scenario.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// The minimum recommended age for players.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Optional list of core moral compass axes explored in this scenario.
    /// </summary>
    public List<string>? CoreAxes { get; set; }

    /// <summary>
    /// Optional list of characters in this scenario.
    /// </summary>
    public List<CharacterRequest>? Characters { get; set; }

    /// <summary>
    /// Optional list of scenes in this scenario.
    /// </summary>
    public List<SceneRequest>? Scenes { get; set; }

    /// <summary>
    /// Optional URL or identifier for the scenario's cover image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional list of compass axes used in this scenario.
    /// </summary>
    public List<string>? CompassAxes { get; set; }
}

/// <summary>
/// Request to update an existing scenario.
/// Extends CreateScenarioRequest with additional admin-controllable fields.
/// </summary>
public record UpdateScenarioRequest
{
    /// <summary>
    /// The title of the scenario.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scenario content and objectives.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The difficulty level of the scenario.
    /// </summary>
    public DifficultyLevel Difficulty { get; set; }

    /// <summary>
    /// The expected duration of a session.
    /// </summary>
    public SessionLength SessionLength { get; set; }

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Optional list of character archetypes available in this scenario.
    /// </summary>
    public List<string>? Archetypes { get; set; }

    /// <summary>
    /// The target age group for this scenario.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// The minimum recommended age for players.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Optional list of core moral compass axes explored in this scenario.
    /// </summary>
    public List<string>? CoreAxes { get; set; }

    /// <summary>
    /// Optional list of characters in this scenario.
    /// </summary>
    public List<CharacterRequest>? Characters { get; set; }

    /// <summary>
    /// Optional list of scenes in this scenario.
    /// </summary>
    public List<SceneRequest>? Scenes { get; set; }

    /// <summary>
    /// Optional URL or identifier for the scenario's cover image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional list of compass axes used in this scenario.
    /// </summary>
    public List<string>? CompassAxes { get; set; }

    /// <summary>
    /// Whether this scenario is featured (admin-controlled).
    /// </summary>
    public bool? IsFeatured { get; set; }

    /// <summary>
    /// Optional URL for the scenario's thumbnail image.
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// Request to query scenarios with filtering and pagination.
/// </summary>
public record ScenarioQueryRequest
{
    /// <summary>
    /// Optional filter by difficulty level (enum).
    /// </summary>
    public DifficultyLevel? Difficulty { get; set; }

    /// <summary>
    /// Optional filter by session length (enum).
    /// </summary>
    public SessionLength? SessionLength { get; set; }

    /// <summary>
    /// Optional filter by minimum age requirement.
    /// </summary>
    public int? MinimumAge { get; set; }

    /// <summary>
    /// Optional filter by age group.
    /// </summary>
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Optional list of tags to filter scenarios.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Optional search term to filter scenarios by title or description.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional search query (alias for SearchTerm).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Optional filter by genre.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Optional list of archetypes to filter scenarios.
    /// </summary>
    public List<string>? Archetypes { get; set; }

    /// <summary>
    /// Optional list of core compass axes to filter scenarios.
    /// </summary>
    public List<string>? CoreAxes { get; set; }
}

/// <summary>
/// Request to validate scenario references (media, characters, etc.).
/// </summary>
public record ValidateScenarioReferencesRequest
{
    /// <summary>
    /// The unique identifier of the scenario to validate.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to validate media references.
    /// </summary>
    public bool ValidateMedia { get; set; } = true;

    /// <summary>
    /// Whether to validate character references.
    /// </summary>
    public bool ValidateCharacters { get; set; } = true;

    /// <summary>
    /// Whether to validate scene connections.
    /// </summary>
    public bool ValidateSceneConnections { get; set; } = true;

    /// <summary>
    /// Whether to include detailed error information.
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
}
