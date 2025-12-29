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

/// <summary>
/// Request for calculating badge score thresholds per tier based on scenario data.
/// </summary>
public record CalculateBadgeScoresRequest
{
    /// <summary>
    /// The ID of the content bundle containing scenarios to analyze.
    /// </summary>
    public string ContentBundleId { get; set; } = string.Empty;

    /// <summary>
    /// Array of percentile values (0-100) to calculate score thresholds for.
    /// Example: [50, 75, 90, 95] for bronze, silver, gold, platinum tiers.
    /// </summary>
    public List<double> Percentiles { get; set; } = new();
}
