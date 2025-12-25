namespace Mystira.Contracts.App.Responses.Scenarios;

public record ScenarioListResponse
{
    public List<ScenarioSummary> Scenarios { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}

public record ScenarioSummary
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

public record ScenarioGameStateResponse
{
    public List<ScenarioWithGameState> Scenarios { get; set; } = new();
    public int TotalCount { get; set; }
}

public record ScenarioWithGameState
{
    public string ScenarioId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GameState { get; set; } = string.Empty;  // NotStarted, InProgress, Completed
    public DateTime? LastPlayedAt { get; set; }
}
