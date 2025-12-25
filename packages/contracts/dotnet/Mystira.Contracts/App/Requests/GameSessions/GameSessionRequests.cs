using Mystira.Contracts.App.Models;

namespace Mystira.Contracts.App.Requests.GameSessions;

public record StartGameSessionRequest
{
    public string ScenarioId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public List<string>? PlayerNames { get; set; }
    public List<CharacterAssignmentDto>? CharacterAssignments { get; set; }
    public string TargetAgeGroup { get; set; } = string.Empty;
}

public record MakeChoiceRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string SceneId { get; set; } = string.Empty;
    public string ChoiceText { get; set; } = string.Empty;
    public string NextSceneId { get; set; } = string.Empty;
    public string? PlayerId { get; set; }
    public string? CompassAxis { get; set; }
    public string? CompassDirection { get; set; }
    public double? CompassDelta { get; set; }
}

public record ProgressSceneRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string SceneId { get; set; } = string.Empty;
}
