using Mystira.Domain.Entities;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a badge that can be earned by players.
/// </summary>
public class Badge : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the badge name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the badge slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the badge description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the badge.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the badge tier (bronze, silver, gold, etc.).
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Gets or sets the tier order for sorting.
    /// </summary>
    public int TierOrder { get; set; }

    /// <summary>
    /// Gets or sets the rarity level.
    /// </summary>
    public string? Rarity { get; set; }

    /// <summary>
    /// Gets or sets the title (alias for Name for DTO compatibility).
    /// </summary>
    public string Title
    {
        get => Name;
        set => Name = value;
    }

    /// <summary>
    /// Gets or sets the compass axis ID for this badge.
    /// </summary>
    public string? CompassAxisId { get; set; }

    /// <summary>
    /// Gets or sets the age group ID for this badge.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the required score to earn this badge.
    /// </summary>
    public int? RequiredScore { get; set; }

    /// <summary>
    /// Gets or sets the primary image ID.
    /// </summary>
    public string? ImageId { get; set; }

    /// <summary>
    /// Gets or sets the points value of the badge.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets whether the badge is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the badge is secret (hidden until earned).
    /// </summary>
    public bool IsSecret { get; set; }

    /// <summary>
    /// Gets or sets the badge images.
    /// </summary>
    public virtual ICollection<BadgeImage> Images { get; set; } = new List<BadgeImage>();

    /// <summary>
    /// Gets or sets the badge configuration/criteria.
    /// </summary>
    public virtual BadgeConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets or sets thresholds for multi-tier badges.
    /// </summary>
    public virtual ICollection<BadgeThreshold> Thresholds { get; set; } = new List<BadgeThreshold>();
}

/// <summary>
/// Represents a badge image variant.
/// </summary>
public class BadgeImage : Entity
{
    /// <summary>
    /// Gets or sets the badge ID.
    /// </summary>
    public string BadgeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image size (small, medium, large).
    /// </summary>
    public string Size { get; set; } = "medium";

    /// <summary>
    /// Gets or sets the image format.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the tier this image is for (for multi-tier badges).
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Gets or sets whether this is the primary image.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the raw image data.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// Gets or sets the content type of the image.
    /// </summary>
    public string? ContentType { get; set; }
}

/// <summary>
/// Represents configuration for how a badge is earned.
/// </summary>
public class BadgeConfiguration : Entity
{
    /// <summary>
    /// Gets or sets the badge ID.
    /// </summary>
    public string BadgeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the threshold value.
    /// </summary>
    public int? Threshold { get; set; }

    /// <summary>
    /// Gets or sets the image ID.
    /// </summary>
    public string? ImageId { get; set; }

    /// <summary>
    /// Gets or sets the achievement type required.
    /// </summary>
    public AchievementType? RequiredAchievementType { get; set; }

    /// <summary>
    /// Gets or sets the scenario ID (for scenario-specific badges).
    /// </summary>
    public string? ScenarioId { get; set; }

    private string? _axisId;

    /// <summary>
    /// Gets or sets the core axis ID (for compass badges).
    /// </summary>
    public string? AxisId
    {
        get => _axisId;
        set => _axisId = value;
    }

    /// <summary>
    /// Gets or sets the archetype ID (for archetype badges).
    /// </summary>
    public string? ArchetypeId { get; set; }

    /// <summary>
    /// Gets or sets the required count/threshold.
    /// </summary>
    public int? RequiredCount { get; set; }

    /// <summary>
    /// Gets or sets the required value (for compass badges).
    /// </summary>
    public int? RequiredValue { get; set; }

    /// <summary>
    /// Gets or sets custom criteria as JSON.
    /// </summary>
    public string? CriteriaJson { get; set; }

    /// <summary>
    /// Gets or sets whether this badge can be earned multiple times.
    /// </summary>
    public bool IsRepeatable { get; set; }

    /// <summary>
    /// Gets or sets the cooldown period in days (for repeatable badges).
    /// </summary>
    public int? CooldownDays { get; set; }

    /// <summary>
    /// Gets or sets the core axis (writable for DTO compatibility).
    /// </summary>
    public CoreAxis? Axis
    {
        get => CoreAxis.FromValue(_axisId);
        set => _axisId = value?.Value;
    }

    /// <summary>
    /// Gets the archetype if configured.
    /// </summary>
    public Archetype? Archetype => Archetype.FromValue(ArchetypeId);
}

/// <summary>
/// Represents a threshold tier for a badge.
/// </summary>
public class BadgeThreshold : Entity
{
    /// <summary>
    /// Gets or sets the badge ID.
    /// </summary>
    public string BadgeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tier name (bronze, silver, gold).
    /// </summary>
    public string Tier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the threshold value required.
    /// </summary>
    public int ThresholdValue { get; set; }

    /// <summary>
    /// Gets or sets the points awarded for this tier.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Represents a badge earned by a user.
/// </summary>
public class UserBadge : Entity
{
    /// <summary>
    /// Gets or sets the user profile ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user profile ID (alias for UserId for DTO compatibility).
    /// </summary>
    public string UserProfileId
    {
        get => UserId;
        set => UserId = value;
    }

    /// <summary>
    /// Gets or sets the badge ID.
    /// </summary>
    public string BadgeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the badge configuration ID.
    /// </summary>
    public string? BadgeConfigurationId { get; set; }

    /// <summary>
    /// Gets or sets the badge name (denormalized for display).
    /// </summary>
    public string? BadgeName { get; set; }

    /// <summary>
    /// Gets or sets the badge message (denormalized for display).
    /// </summary>
    public string? BadgeMessage { get; set; }

    /// <summary>
    /// Gets or sets the trigger value that earned this badge.
    /// </summary>
    public int? TriggerValue { get; set; }

    /// <summary>
    /// Gets or sets the threshold value for this badge.
    /// </summary>
    public int? Threshold { get; set; }

    /// <summary>
    /// Gets or sets the game session ID where it was earned.
    /// </summary>
    public string? GameSessionId { get; set; }

    /// <summary>
    /// Gets or sets the image ID.
    /// </summary>
    public string? ImageId { get; set; }

    /// <summary>
    /// Gets or sets the compass axis ID (for axis-related badges).
    /// </summary>
    public string? Axis { get; set; }

    /// <summary>
    /// Gets or sets the tier achieved.
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Gets or sets when the badge was earned.
    /// </summary>
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the session where it was earned.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the scenario where it was earned.
    /// </summary>
    public string? ScenarioId { get; set; }

    /// <summary>
    /// Gets or sets the current progress (for progressive badges).
    /// </summary>
    public int? Progress { get; set; }

    /// <summary>
    /// Gets or sets how many times this badge has been earned.
    /// </summary>
    public int EarnedCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether this badge is featured on the user's profile.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Navigation to the badge.
    /// </summary>
    public virtual Badge? Badge { get; set; }

    /// <summary>
    /// Navigation to the user profile.
    /// </summary>
    public virtual UserProfile? User { get; set; }
}

/// <summary>
/// Represents an achievement on a specific compass axis.
/// </summary>
public class AxisAchievement : Entity
{
    /// <summary>
    /// Gets or sets the user profile ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the core axis ID.
    /// </summary>
    public string AxisId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the compass axis ID (alias for AxisId for DTO compatibility).
    /// </summary>
    public string CompassAxisId
    {
        get => AxisId;
        set => AxisId = value;
    }

    /// <summary>
    /// Gets or sets the current value on this axis.
    /// </summary>
    public int CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the highest value ever achieved.
    /// </summary>
    public int HighestValue { get; set; }

    /// <summary>
    /// Gets or sets the lowest value ever achieved.
    /// </summary>
    public int LowestValue { get; set; }

    /// <summary>
    /// Gets or sets how many times this axis was affected.
    /// </summary>
    public int TotalChanges { get; set; }

    /// <summary>
    /// Gets or sets the last update time.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the core axis.
    /// </summary>
    public CoreAxis? Axis => CoreAxis.FromValue(AxisId);

    /// <summary>
    /// Navigation to the user profile.
    /// </summary>
    public virtual UserProfile? User { get; set; }

    /// <summary>
    /// Updates the value on this axis.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    public void UpdateValue(int newValue)
    {
        CurrentValue = Math.Clamp(newValue, -100, 100);

        if (CurrentValue > HighestValue)
            HighestValue = CurrentValue;

        if (CurrentValue < LowestValue)
            LowestValue = CurrentValue;

        TotalChanges++;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
