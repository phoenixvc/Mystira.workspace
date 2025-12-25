namespace Mystira.Contracts.App.Responses.Scenarios;

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
}
