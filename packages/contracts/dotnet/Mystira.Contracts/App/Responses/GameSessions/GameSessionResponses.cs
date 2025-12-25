namespace Mystira.Contracts.App.Responses.GameSessions;

public record GameSessionResponse
{
    public string Id { get; set; } = string.Empty;
    public string ScenarioId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public List<string> PlayerNames { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string CurrentSceneId { get; set; } = string.Empty;
    public int ChoiceCount { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string TargetAgeGroup { get; set; } = string.Empty;
}

public record SessionStatsResponse
{
    public Dictionary<string, double> CompassValues { get; set; } = new();
    public int TotalChoices { get; set; }
    public TimeSpan SessionDuration { get; set; }
}
