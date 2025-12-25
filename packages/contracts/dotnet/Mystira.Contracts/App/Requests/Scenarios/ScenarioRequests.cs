namespace Mystira.Contracts.App.Requests.Scenarios;

public record CreateScenarioRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string SessionLength { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
    public List<string>? Archetypes { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public int MinimumAge { get; set; }
    public List<string>? CoreAxes { get; set; }
}

public record ScenarioQueryRequest
{
    public string? Difficulty { get; set; }
    public string? SessionLength { get; set; }
    public int? MinimumAge { get; set; }
    public string? AgeGroup { get; set; }
    public List<string>? Tags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
