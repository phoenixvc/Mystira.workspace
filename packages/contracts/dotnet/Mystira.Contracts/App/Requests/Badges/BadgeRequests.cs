namespace Mystira.Contracts.App.Requests.Badges;

/// <summary>
/// Request to award a badge to a user profile.
/// </summary>
public record AwardBadgeRequest
{
    /// <summary>
    /// The unique identifier of the user profile to award the badge to.
    /// </summary>
    public string UserProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the badge configuration to award.
    /// </summary>
    public string BadgeConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// The trigger value that caused the badge to be awarded.
    /// </summary>
    public float TriggerValue { get; set; }

    /// <summary>
    /// Optional game session identifier where the badge was earned.
    /// </summary>
    public string? GameSessionId { get; set; }

    /// <summary>
    /// Optional scenario identifier where the badge was earned.
    /// </summary>
    public string? ScenarioId { get; set; }
}
