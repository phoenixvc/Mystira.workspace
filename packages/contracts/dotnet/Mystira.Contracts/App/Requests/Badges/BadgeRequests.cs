namespace Mystira.Contracts.App.Requests.Badges;

public record AwardBadgeRequest
{
    public string UserProfileId { get; set; } = string.Empty;
    public string BadgeConfigurationId { get; set; } = string.Empty;
    public float TriggerValue { get; set; }
    public string? GameSessionId { get; set; }
    public string? ScenarioId { get; set; }
}
