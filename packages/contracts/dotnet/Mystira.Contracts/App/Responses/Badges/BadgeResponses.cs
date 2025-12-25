namespace Mystira.Contracts.App.Responses.Badges;

/// <summary>
/// Badge and achievement response types for the Mystira gamification system.
/// </summary>

/// <summary>
/// Represents a badge that can be earned by users.
/// </summary>
public record BadgeResponse
{
    /// <summary>
    /// Unique identifier for the badge.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the badge.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of how to earn the badge.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// URL to the badge icon/image.
    /// </summary>
    public string? IconUrl { get; init; }

    /// <summary>
    /// Category or type of the badge (e.g., "story", "engagement", "milestone").
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// The tier of this badge (e.g., "bronze", "silver", "gold", "platinum").
    /// </summary>
    public string? Tier { get; init; }

    /// <summary>
    /// Display order of this tier within the badge hierarchy.
    /// </summary>
    public int TierOrder { get; init; }

    /// <summary>
    /// Title of the badge.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Age group identifier for age-appropriate badge tracking.
    /// </summary>
    public string? AgeGroupId { get; init; }

    /// <summary>
    /// Required score threshold to earn this badge.
    /// </summary>
    public float RequiredScore { get; init; }

    /// <summary>
    /// Media identifier for the badge image.
    /// </summary>
    public string? ImageId { get; init; }

    /// <summary>
    /// Points awarded when earning this badge.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Whether the user has earned this badge.
    /// </summary>
    public bool IsEarned { get; init; }

    /// <summary>
    /// When the badge was earned, if applicable.
    /// </summary>
    public DateTimeOffset? EarnedAt { get; init; }

    /// <summary>
    /// The compass axis this badge is associated with, if any.
    /// </summary>
    public string? CompassAxisId { get; init; }
}

/// <summary>
/// Represents progress toward earning a badge.
/// </summary>
public record BadgeProgressResponse
{
    /// <summary>
    /// The badge being tracked.
    /// </summary>
    public required BadgeResponse Badge { get; init; }

    /// <summary>
    /// Current progress value.
    /// </summary>
    public required int CurrentValue { get; init; }

    /// <summary>
    /// Target value to earn the badge.
    /// </summary>
    public required int TargetValue { get; init; }

    /// <summary>
    /// Progress as a percentage (0-100).
    /// </summary>
    public double ProgressPercentage => TargetValue > 0
        ? Math.Min(100, (double)CurrentValue / TargetValue * 100)
        : 0;

    /// <summary>
    /// Whether the badge requirement is complete.
    /// </summary>
    public bool IsComplete => CurrentValue >= TargetValue;

    /// <summary>
    /// Description of the current milestone.
    /// </summary>
    public string? MilestoneDescription { get; init; }

    /// <summary>
    /// The age group identifier for age-appropriate badge tracking.
    /// </summary>
    public string? AgeGroupId { get; init; }

    /// <summary>
    /// Progress on each compass axis related to this badge.
    /// </summary>
    public IReadOnlyList<AxisProgressResponse> AxisProgresses { get; init; } = [];
}

/// <summary>
/// Represents an achievement earned on a specific compass axis.
/// </summary>
public record AxisAchievementResponse
{
    /// <summary>
    /// Unique identifier for the achievement.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Age group identifier for age-appropriate achievement tracking.
    /// </summary>
    public string? AgeGroupId { get; init; }

    /// <summary>
    /// The compass axis this achievement belongs to.
    /// </summary>
    public required string CompassAxisId { get; init; }

    /// <summary>
    /// Name of the compass axis.
    /// </summary>
    public string? CompassAxisName { get; init; }

    /// <summary>
    /// Name of the axis (e.g., "Creativity", "Logic", "Empathy").
    /// </summary>
    public required string AxisName { get; init; }

    /// <summary>
    /// Direction on the compass axis (e.g., "positive", "negative").
    /// </summary>
    public string? AxesDirection { get; init; }

    /// <summary>
    /// Display name of the achievement.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of the achievement.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Level or tier of this achievement within the axis.
    /// </summary>
    public required int Level { get; init; }

    /// <summary>
    /// Score threshold required to unlock this achievement.
    /// </summary>
    public required int ScoreThreshold { get; init; }

    /// <summary>
    /// URL to the achievement icon.
    /// </summary>
    public string? IconUrl { get; init; }

    /// <summary>
    /// Whether the user has unlocked this achievement.
    /// </summary>
    public bool IsUnlocked { get; init; }

    /// <summary>
    /// When the achievement was unlocked, if applicable.
    /// </summary>
    public DateTimeOffset? UnlockedAt { get; init; }
}

/// <summary>
/// Represents progress on a specific compass axis.
/// </summary>
public record AxisProgressResponse
{
    /// <summary>
    /// The compass axis identifier.
    /// </summary>
    public required string CompassAxisId { get; init; }

    /// <summary>
    /// Name of the compass axis.
    /// </summary>
    public string? CompassAxisName { get; init; }

    /// <summary>
    /// Display name of the axis.
    /// </summary>
    public required string AxisName { get; init; }

    /// <summary>
    /// Current score on this axis.
    /// </summary>
    public required int CurrentScore { get; init; }

    /// <summary>
    /// Maximum possible score on this axis.
    /// </summary>
    public int MaxScore { get; init; }

    /// <summary>
    /// Current level achieved on this axis.
    /// </summary>
    public required int CurrentLevel { get; init; }

    /// <summary>
    /// Score needed to reach the next level.
    /// </summary>
    public int? NextLevelThreshold { get; init; }

    /// <summary>
    /// Progress percentage toward next level (0-100).
    /// </summary>
    public double? NextLevelProgress { get; init; }

    /// <summary>
    /// Achievements earned on this axis.
    /// </summary>
    public IReadOnlyList<AxisAchievementResponse> Achievements { get; init; } = [];

    /// <summary>
    /// Badge tier progress for this axis.
    /// </summary>
    public IReadOnlyList<BadgeTierProgressResponse> Tiers { get; init; } = [];

    /// <summary>
    /// Color associated with this axis for UI display.
    /// </summary>
    public string? Color { get; init; }
}

/// <summary>
/// Represents progress through badge tiers.
/// </summary>
public record BadgeTierProgressResponse
{
    /// <summary>
    /// Unique identifier for the badge.
    /// </summary>
    public string? BadgeId { get; init; }

    /// <summary>
    /// The tier name (e.g., "bronze", "silver", "gold", "platinum").
    /// </summary>
    public required string TierName { get; init; }

    /// <summary>
    /// Tier identifier (e.g., "bronze", "silver", "gold").
    /// </summary>
    public string? Tier { get; init; }

    /// <summary>
    /// Display order of this tier.
    /// </summary>
    public required int TierOrder { get; init; }

    /// <summary>
    /// Title of the badge tier.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Description of the badge tier.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Required score threshold to earn this tier.
    /// </summary>
    public float RequiredScore { get; init; }

    /// <summary>
    /// Media identifier for the tier image.
    /// </summary>
    public string? ImageId { get; init; }

    /// <summary>
    /// Whether the tier has been earned.
    /// </summary>
    public bool IsEarned { get; init; }

    /// <summary>
    /// When the tier was earned, if applicable.
    /// </summary>
    public DateTime? EarnedAt { get; init; }

    /// <summary>
    /// Progress toward the threshold as a percentage (0-1).
    /// </summary>
    public float ProgressToThreshold { get; init; }

    /// <summary>
    /// Remaining score needed to reach this tier.
    /// </summary>
    public float RemainingScore { get; init; }

    /// <summary>
    /// Total badges available in this tier.
    /// </summary>
    public required int TotalBadges { get; init; }

    /// <summary>
    /// Number of badges earned in this tier.
    /// </summary>
    public required int EarnedBadges { get; init; }

    /// <summary>
    /// Completion percentage for this tier (0-100).
    /// </summary>
    public double CompletionPercentage => TotalBadges > 0
        ? (double)EarnedBadges / TotalBadges * 100
        : 0;

    /// <summary>
    /// Whether all badges in this tier have been earned.
    /// </summary>
    public bool IsComplete => EarnedBadges >= TotalBadges;

    /// <summary>
    /// Badges in this tier.
    /// </summary>
    public IReadOnlyList<BadgeResponse> Badges { get; init; } = [];

    /// <summary>
    /// Color associated with this tier for UI display.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Icon URL for this tier.
    /// </summary>
    public string? IconUrl { get; init; }
}

/// <summary>
/// Represents a user's score on a compass axis.
/// </summary>
public record CompassAxisScoreResult
{
    /// <summary>
    /// The compass axis identifier.
    /// </summary>
    public required string CompassAxisId { get; init; }

    /// <summary>
    /// Display name of the axis.
    /// </summary>
    public required string AxisName { get; init; }

    /// <summary>
    /// The calculated score for this axis.
    /// </summary>
    public required double Score { get; init; }

    /// <summary>
    /// Normalized score (0-1 range).
    /// </summary>
    public required double NormalizedScore { get; init; }

    /// <summary>
    /// Percentile rank compared to other users.
    /// </summary>
    public double? Percentile { get; init; }

    /// <summary>
    /// Description of what this score means.
    /// </summary>
    public string? Interpretation { get; init; }

    /// <summary>
    /// Strength level based on score (e.g., "low", "medium", "high", "exceptional").
    /// </summary>
    public string? StrengthLevel { get; init; }

    /// <summary>
    /// Related traits or characteristics for this axis.
    /// </summary>
    public IReadOnlyList<string> RelatedTraits { get; init; } = [];

    /// <summary>
    /// When this score was last calculated.
    /// </summary>
    public DateTimeOffset CalculatedAt { get; init; }
}
