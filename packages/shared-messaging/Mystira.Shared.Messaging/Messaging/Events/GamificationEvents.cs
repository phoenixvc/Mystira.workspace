namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user unlocks an achievement.
/// </summary>
public sealed record AchievementUnlocked : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The achievement ID.
    /// </summary>
    public required string AchievementId { get; init; }

    /// <summary>
    /// Achievement name.
    /// </summary>
    public required string AchievementName { get; init; }

    /// <summary>
    /// Achievement category (e.g., "story", "social", "exploration").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Rarity level (common, uncommon, rare, epic, legendary).
    /// </summary>
    public string? Rarity { get; init; }

    /// <summary>
    /// XP reward for this achievement.
    /// </summary>
    public int XpReward { get; init; }

    /// <summary>
    /// The scenario ID if achievement was earned in a specific scenario.
    /// </summary>
    public string? ScenarioId { get; init; }
}

/// <summary>
/// Published when a user earns a badge.
/// </summary>
public sealed record BadgeEarned : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The badge ID.
    /// </summary>
    public required string BadgeId { get; init; }

    /// <summary>
    /// Badge name.
    /// </summary>
    public required string BadgeName { get; init; }

    /// <summary>
    /// Badge tier (bronze, silver, gold, platinum).
    /// </summary>
    public string? Tier { get; init; }

    /// <summary>
    /// How the badge was earned.
    /// </summary>
    public string? EarnedVia { get; init; }
}

/// <summary>
/// Published when a user earns experience points.
/// </summary>
public sealed record XPEarned : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Amount of XP earned.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Source of XP (e.g., "chapter_complete", "achievement", "daily_login").
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Total XP after this earning.
    /// </summary>
    public required long TotalXP { get; init; }

    /// <summary>
    /// Any multiplier applied (e.g., 2x weekend bonus).
    /// </summary>
    public decimal Multiplier { get; init; } = 1.0m;

    /// <summary>
    /// Related entity ID (scenario, achievement, etc.).
    /// </summary>
    public string? RelatedEntityId { get; init; }
}

/// <summary>
/// Published when a user levels up.
/// </summary>
public sealed record LevelUp : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Previous level.
    /// </summary>
    public required int FromLevel { get; init; }

    /// <summary>
    /// New level.
    /// </summary>
    public required int ToLevel { get; init; }

    /// <summary>
    /// Total XP at this level.
    /// </summary>
    public required long TotalXP { get; init; }

    /// <summary>
    /// Rewards unlocked at this level.
    /// </summary>
    public string[]? UnlockedRewards { get; init; }
}

/// <summary>
/// Published when a user's streak is updated.
/// </summary>
public sealed record StreakUpdated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Streak type (daily, weekly).
    /// </summary>
    public required string StreakType { get; init; }

    /// <summary>
    /// Current streak count.
    /// </summary>
    public required int CurrentStreak { get; init; }

    /// <summary>
    /// Previous streak count.
    /// </summary>
    public int PreviousStreak { get; init; }

    /// <summary>
    /// Longest streak ever.
    /// </summary>
    public int LongestStreak { get; init; }

    /// <summary>
    /// Whether the streak was broken and reset.
    /// </summary>
    public bool WasBroken { get; init; }

    /// <summary>
    /// Bonus XP earned for streak milestone.
    /// </summary>
    public int BonusXP { get; init; }
}

/// <summary>
/// Published when leaderboard position changes.
/// </summary>
public sealed record LeaderboardUpdated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Leaderboard ID (global, scenario-specific, etc.).
    /// </summary>
    public required string LeaderboardId { get; init; }

    /// <summary>
    /// Previous rank (0 if new entry).
    /// </summary>
    public int PreviousRank { get; init; }

    /// <summary>
    /// New rank.
    /// </summary>
    public required int NewRank { get; init; }

    /// <summary>
    /// Score or points value.
    /// </summary>
    public required long Score { get; init; }

    /// <summary>
    /// Time period (daily, weekly, monthly, all-time).
    /// </summary>
    public required string Period { get; init; }
}
