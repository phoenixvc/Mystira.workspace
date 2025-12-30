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

/// <summary>
/// Request to create a new badge configuration.
/// </summary>
public record CreateBadgeConfigurationRequest
{
    /// <summary>
    /// Display name of the badge.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of how to earn the badge.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category or type of the badge (e.g., "story", "engagement", "milestone").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// The tier of this badge (e.g., "bronze", "silver", "gold", "platinum").
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Display order of this tier within the badge hierarchy.
    /// </summary>
    public int TierOrder { get; set; }

    /// <summary>
    /// Title of the badge.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Age group identifier for age-appropriate badge tracking.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Required score threshold to earn this badge.
    /// </summary>
    public float RequiredScore { get; set; }

    /// <summary>
    /// Media identifier for the badge image.
    /// </summary>
    public string? ImageId { get; set; }

    /// <summary>
    /// Points awarded when earning this badge.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// The compass axis this badge is associated with, if any.
    /// </summary>
    public string? CompassAxisId { get; set; }
}

/// <summary>
/// Request to update an existing badge configuration.
/// </summary>
public record UpdateBadgeConfigurationRequest
{
    /// <summary>
    /// Updated display name of the badge.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Updated description of how to earn the badge.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated category or type of the badge.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Updated tier of this badge.
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Updated display order of this tier.
    /// </summary>
    public int? TierOrder { get; set; }

    /// <summary>
    /// Updated title of the badge.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Updated age group identifier.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Updated required score threshold.
    /// </summary>
    public float? RequiredScore { get; set; }

    /// <summary>
    /// Updated media identifier for the badge image.
    /// </summary>
    public string? ImageId { get; set; }

    /// <summary>
    /// Updated points awarded when earning this badge.
    /// </summary>
    public int? Points { get; set; }

    /// <summary>
    /// Updated compass axis this badge is associated with.
    /// </summary>
    public string? CompassAxisId { get; set; }
}
