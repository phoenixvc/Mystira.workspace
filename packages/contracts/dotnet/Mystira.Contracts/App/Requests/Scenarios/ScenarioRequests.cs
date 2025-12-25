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
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>
    /// The expected duration of a session.
    /// </summary>
    public string SessionLength { get; set; } = string.Empty;

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
}

/// <summary>
/// Request to query scenarios with filtering and pagination.
/// </summary>
public record ScenarioQueryRequest
{
    /// <summary>
    /// Optional filter by difficulty level.
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// Optional filter by session length.
    /// </summary>
    public string? SessionLength { get; set; }

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
}
