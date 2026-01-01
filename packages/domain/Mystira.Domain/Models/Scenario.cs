using Mystira.Domain.Entities;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a playable scenario in the Mystira system.
/// A scenario is an interactive story with scenes, characters, and branching paths.
/// </summary>
public class Scenario : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the scenario title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario slug (URL-friendly identifier).
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the short summary for listings.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the author/creator ID.
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content bundle ID this scenario belongs to.
    /// </summary>
    public string? ContentBundleId { get; set; }

    /// <summary>
    /// Gets or sets the difficulty level.
    /// </summary>
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    /// <summary>
    /// Gets or sets the expected session length.
    /// </summary>
    public SessionLength SessionLength { get; set; } = SessionLength.Medium;

    /// <summary>
    /// Gets or sets the publication status.
    /// </summary>
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;

    /// <summary>
    /// Gets or sets the target age group ID.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the fantasy theme ID.
    /// </summary>
    public string? ThemeId { get; set; }

    /// <summary>
    /// Gets or sets the cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the image reference (alias for CoverImageUrl for DTO compatibility).
    /// </summary>
    public string? Image
    {
        get => CoverImageUrl;
        set => CoverImageUrl = value;
    }

    /// <summary>
    /// Gets or sets the music palette/theme for this scenario.
    /// </summary>
    public MusicPalette? MusicPalette { get; set; }

    /// <summary>
    /// Gets or sets the starting scene ID.
    /// </summary>
    public string? StartSceneId { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the scenario is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Gets or sets when the scenario was published.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the total play count.
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// Gets or sets the average rating (1-5).
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the number of ratings.
    /// </summary>
    public int RatingCount { get; set; }

    /// <summary>
    /// Gets or sets the estimated play time in minutes.
    /// </summary>
    public int? EstimatedPlayTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the minimum player count.
    /// </summary>
    public int MinPlayers { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum player count.
    /// </summary>
    public int MaxPlayers { get; set; } = 1;

    /// <summary>
    /// Gets or sets tags as JSON array.
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Gets or sets the tags list (for DTO compatibility).
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the archetypes used in this scenario.
    /// </summary>
    public List<string> Archetypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum age requirement.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Gets or sets the core axes featured in this scenario.
    /// </summary>
    public List<string> CoreAxes { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the scenario is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the Story Protocol metadata.
    /// </summary>
    public ScenarioStoryProtocol? StoryProtocol { get; set; }

    /// <summary>
    /// Gets or sets the characters in this scenario.
    /// </summary>
    public virtual ICollection<ScenarioCharacter> Characters { get; set; } = new List<ScenarioCharacter>();

    /// <summary>
    /// Gets or sets the scenes in this scenario.
    /// </summary>
    public virtual ICollection<Scene> Scenes { get; set; } = new List<Scene>();

    /// <summary>
    /// Gets or sets the media references.
    /// </summary>
    public virtual ICollection<MediaReference> MediaReferences { get; set; } = new List<MediaReference>();

    /// <summary>
    /// Gets the target age group.
    /// </summary>
    public AgeGroup? AgeGroup => AgeGroup.FromId(AgeGroupId);

    /// <summary>
    /// Gets the fantasy theme.
    /// </summary>
    public FantasyTheme? Theme => FantasyTheme.FromValue(ThemeId);

    /// <summary>
    /// Gets the starting scene.
    /// </summary>
    public Scene? StartScene => Scenes.FirstOrDefault(s => s.Id == StartSceneId);

    /// <summary>
    /// Publishes the scenario.
    /// </summary>
    public void Publish()
    {
        if (Status == PublicationStatus.Draft || Status == PublicationStatus.UnderReview)
        {
            Status = PublicationStatus.Published;
            PublishedAt = DateTime.UtcNow;
            Version++;
        }
    }

    /// <summary>
    /// Archives the scenario.
    /// </summary>
    public void Archive()
    {
        Status = PublicationStatus.Archived;
    }
}

/// <summary>
/// Represents a character in a scenario.
/// </summary>
public class ScenarioCharacter : Entity
{
    /// <summary>
    /// Gets or sets the scenario ID.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the character role/type.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the archetype ID.
    /// </summary>
    public string? ArchetypeId { get; set; }

    /// <summary>
    /// Gets or sets the character's avatar image URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the character image reference (for DTO compatibility).
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the character audio reference (for DTO compatibility).
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Gets or sets the character metadata (for DTO compatibility).
    /// </summary>
    public ScenarioCharacterMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets whether this is a player-controlled character.
    /// </summary>
    public bool IsPlayable { get; set; }

    /// <summary>
    /// Gets or sets whether this is the main protagonist.
    /// </summary>
    public bool IsProtagonist { get; set; }

    /// <summary>
    /// Gets or sets the character's personality traits as JSON.
    /// </summary>
    public string? PersonalityTraitsJson { get; set; }

    /// <summary>
    /// Gets or sets the character's backstory.
    /// </summary>
    public string? Backstory { get; set; }

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets the character's archetype.
    /// </summary>
    public Archetype? Archetype => Archetype.FromValue(ArchetypeId);
}

/// <summary>
/// Represents a scene in a scenario.
/// </summary>
public class Scene : Entity
{
    /// <summary>
    /// Gets or sets the scenario ID.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene content/narrative.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene description (alias for Content for DTO compatibility).
    /// </summary>
    public string Description
    {
        get => Content;
        set => Content = value;
    }

    /// <summary>
    /// Gets or sets the scene type.
    /// </summary>
    public SceneType Type { get; set; } = SceneType.Standard;

    /// <summary>
    /// Gets or sets the scene slug.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the next scene ID for linear progression.
    /// </summary>
    public string? NextSceneId { get; set; }

    /// <summary>
    /// Gets or sets the background image URL.
    /// </summary>
    public string? BackgroundUrl { get; set; }

    /// <summary>
    /// Gets or sets the background music URL.
    /// </summary>
    public string? MusicUrl { get; set; }

    /// <summary>
    /// Gets or sets ambient sound URL.
    /// </summary>
    public string? AmbientSoundUrl { get; set; }

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the difficulty level (0-3, nullable).
    /// </summary>
    public int? Difficulty { get; set; }

    /// <summary>
    /// Gets or sets media references for this scene.
    /// </summary>
    public MediaReferences? Media { get; set; }

    /// <summary>
    /// Gets or sets music settings for this scene.
    /// </summary>
    public SceneMusicSettings? Music { get; set; }

    /// <summary>
    /// Gets or sets sound effects for this scene.
    /// </summary>
    public List<SceneSoundEffect> SoundEffects { get; set; } = new();

    /// <summary>
    /// Gets or sets the active character ID for choice scenes.
    /// </summary>
    public string? ActiveCharacter { get; set; }

    /// <summary>
    /// Gets or sets whether this is an ending scene.
    /// </summary>
    public bool IsEnding { get; set; }

    /// <summary>
    /// Gets or sets the ending type if this is an ending scene.
    /// </summary>
    public string? EndingType { get; set; }

    /// <summary>
    /// Gets or sets the branches (choices) from this scene.
    /// </summary>
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    /// <summary>
    /// Gets or sets compass changes that occur in this scene.
    /// </summary>
    public virtual ICollection<CompassChange> CompassChanges { get; set; } = new List<CompassChange>();

    /// <summary>
    /// Gets or sets echo reveals in this scene.
    /// </summary>
    public virtual ICollection<EchoReveal> EchoReveals { get; set; } = new List<EchoReveal>();
}

/// <summary>
/// Represents a choice/branch from a scene.
/// </summary>
public class Branch : Entity
{
    /// <summary>
    /// Gets or sets the source scene ID.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target scene ID.
    /// </summary>
    public string TargetSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the next scene ID (alias for TargetSceneId for DTO compatibility).
    /// </summary>
    public string NextSceneId
    {
        get => TargetSceneId;
        set => TargetSceneId = value;
    }

    /// <summary>
    /// Gets or sets the branch text (what the player sees).
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the choice text (alias for Text for DTO compatibility).
    /// </summary>
    public string Choice
    {
        get => Text;
        set => Text = value;
    }

    /// <summary>
    /// Gets or sets an optional description/hint.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets conditions required to see this branch as JSON.
    /// </summary>
    public string? ConditionsJson { get; set; }

    /// <summary>
    /// Gets or sets whether this is a hidden/secret branch.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// Gets or sets the echo log for this branch.
    /// </summary>
    public EchoLog? EchoLog { get; set; }

    /// <summary>
    /// Gets or sets a single compass change for this branch (DTO compatibility).
    /// </summary>
    public CompassChangeDto? CompassChange { get; set; }

    /// <summary>
    /// Gets or sets compass changes when this branch is taken.
    /// </summary>
    public virtual ICollection<CompassChange> CompassChanges { get; set; } = new List<CompassChange>();
}

/// <summary>
/// Represents a media reference in a scenario.
/// </summary>
public class MediaReference : Entity
{
    /// <summary>
    /// Gets or sets the scenario ID.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene ID (optional, for scene-specific media).
    /// </summary>
    public string? SceneId { get; set; }

    /// <summary>
    /// Gets or sets the media type (image, audio, video).
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the media URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the media key/identifier.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets alt text for accessibility.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Gets or sets the media purpose (background, character, item, etc.).
    /// </summary>
    public string? Purpose { get; set; }
}

/// <summary>
/// Represents an echo log entry.
/// </summary>
public class EchoLog : Entity
{
    /// <summary>
    /// Gets or sets the scenario ID.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the echo type ID.
    /// </summary>
    public string EchoTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the echo title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the echo content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets which character this echo relates to.
    /// </summary>
    public string? CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the scene where this echo is revealed.
    /// </summary>
    public string? RevealSceneId { get; set; }

    /// <summary>
    /// Gets or sets whether this echo is a major plot point.
    /// </summary>
    public bool IsMajor { get; set; }

    /// <summary>
    /// Gets or sets the echo description (alias for Content for DTO compatibility).
    /// </summary>
    public string Description
    {
        get => Content;
        set => Content = value;
    }

    /// <summary>
    /// Gets or sets the echo strength (0.0 to 1.0).
    /// </summary>
    public double Strength { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the timestamp when this echo was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the echo type.
    /// </summary>
    public EchoType? EchoType => EchoType.FromValue(EchoTypeId);
}

/// <summary>
/// Represents an echo reveal event.
/// </summary>
public class EchoReveal
{
    /// <summary>
    /// Gets or sets the echo log ID being revealed.
    /// </summary>
    public string EchoLogId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the echo type (for DTO compatibility).
    /// </summary>
    public string EchoType { get; set; } = "honesty";

    /// <summary>
    /// Gets or sets the reveal trigger type.
    /// </summary>
    public string TriggerType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trigger scene ID (for DTO compatibility).
    /// </summary>
    public string TriggerSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum echo strength required.
    /// </summary>
    public double MinStrength { get; set; }

    /// <summary>
    /// Gets or sets the maximum age (in scenes) for this echo.
    /// </summary>
    public int? MaxAgeScenes { get; set; }

    /// <summary>
    /// Gets or sets the reveal mechanic description.
    /// </summary>
    public string? RevealMechanic { get; set; }

    /// <summary>
    /// Gets or sets whether this echo reveal is required.
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    /// Gets or sets the reveal conditions as JSON.
    /// </summary>
    public string? ConditionsJson { get; set; }
}

/// <summary>
/// Represents a compass value change (entity model).
/// </summary>
public class CompassChange
{
    /// <summary>
    /// Gets or sets the core axis ID.
    /// </summary>
    public string AxisId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the change amount (-100 to +100).
    /// </summary>
    public int Delta { get; set; }

    /// <summary>
    /// Gets or sets the reason for the change.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets the core axis.
    /// </summary>
    public CoreAxis? Axis => CoreAxis.FromValue(AxisId);
}

/// <summary>
/// Represents a compass value change (DTO model for Application layer compatibility).
/// Uses Axis as string and Delta as double to match request/response patterns.
/// </summary>
public class CompassChangeDto
{
    /// <summary>
    /// Gets or sets the axis name/ID.
    /// </summary>
    public string Axis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the change amount.
    /// </summary>
    public double Delta { get; set; }

    /// <summary>
    /// Gets or sets the developmental link description.
    /// </summary>
    public string? DevelopmentalLink { get; set; }
}

/// <summary>
/// Represents compass tracking state for a player.
/// </summary>
public class CompassTracking
{
    /// <summary>
    /// Gets or sets the axis being tracked (for single-axis tracking).
    /// </summary>
    public string Axis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key identifier.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the current value on this axis.
    /// </summary>
    public double CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the value (alias for CurrentValue as int for DTO compatibility).
    /// </summary>
    public int Value
    {
        get => (int)CurrentValue;
        set => CurrentValue = value;
    }

    /// <summary>
    /// Gets or sets the starting value for this tracking.
    /// </summary>
    public int StartingValue { get; set; }

    /// <summary>
    /// Gets or sets when this tracking was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the axis values (axis ID -> value) for multi-axis tracking.
    /// </summary>
    public Dictionary<string, int> AxisValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the history of changes.
    /// </summary>
    public List<CompassChangeRecord> History { get; set; } = new();

    /// <summary>
    /// Gets the value for a specific axis.
    /// </summary>
    public int GetAxisValue(CoreAxis axis) => AxisValues.TryGetValue(axis.Value, out var value) ? value : 0;

    /// <summary>
    /// Applies a compass change.
    /// </summary>
    public void ApplyChange(CompassChange change, string? sceneId = null)
    {
        if (!AxisValues.ContainsKey(change.AxisId))
            AxisValues[change.AxisId] = 0;

        var oldValue = AxisValues[change.AxisId];
        var newValue = Math.Clamp(oldValue + change.Delta, -100, 100);
        AxisValues[change.AxisId] = newValue;

        History.Add(new CompassChangeRecord
        {
            AxisId = change.AxisId,
            OldValue = oldValue,
            NewValue = newValue,
            Delta = change.Delta,
            SceneId = sceneId,
            Reason = change.Reason,
            Timestamp = DateTime.UtcNow
        });

        LastUpdated = DateTime.UtcNow;
    }
}

/// <summary>
/// Records a compass change event.
/// </summary>
public class CompassChangeRecord
{
    /// <summary>
    /// Gets or sets the axis ID.
    /// </summary>
    public string AxisId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value before the change.
    /// </summary>
    public int OldValue { get; set; }

    /// <summary>
    /// Gets or sets the value after the change.
    /// </summary>
    public int NewValue { get; set; }

    /// <summary>
    /// Gets or sets the delta applied.
    /// </summary>
    public int Delta { get; set; }

    /// <summary>
    /// Gets or sets the scene where the change occurred.
    /// </summary>
    public string? SceneId { get; set; }

    /// <summary>
    /// Gets or sets the reason for the change.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets when the change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Represents Story Protocol metadata for IP asset registration on scenarios/bundles.
/// </summary>
public class ScenarioStoryProtocol
{
    /// <summary>
    /// Gets or sets the IP Asset ID on Story Protocol.
    /// </summary>
    public string? IpAssetId { get; set; }

    /// <summary>
    /// Gets or sets the transaction hash of registration.
    /// </summary>
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Gets or sets when the IP asset was registered.
    /// </summary>
    public DateTime? RegisteredAt { get; set; }

    /// <summary>
    /// Gets or sets whether the registration is complete.
    /// </summary>
    public bool IsRegistered { get; set; }

    /// <summary>
    /// Gets or sets the license terms ID.
    /// </summary>
    public string? LicenseTermsId { get; set; }

    /// <summary>
    /// Gets or sets the royalty policy ID.
    /// </summary>
    public string? RoyaltyPolicyId { get; set; }

    /// <summary>
    /// Gets or sets the registration transaction hash (alias for TransactionHash for DTO compatibility).
    /// </summary>
    public string? RegistrationTxHash
    {
        get => TransactionHash;
        set => TransactionHash = value;
    }

    /// <summary>
    /// Gets or sets the royalty module ID (alias for RoyaltyPolicyId for DTO compatibility).
    /// </summary>
    public string? RoyaltyModuleId
    {
        get => RoyaltyPolicyId;
        set => RoyaltyPolicyId = value;
    }

    /// <summary>
    /// Gets or sets the contributors for this IP asset.
    /// </summary>
    public List<Contributor> Contributors { get; set; } = new();

    /// <summary>
    /// Validates that contributor splits sum to 100%.
    /// </summary>
    /// <param name="errors">Output list of validation errors.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public bool ValidateContributorSplits(out List<string> errors)
    {
        errors = new List<string>();

        if (Contributors == null || !Contributors.Any())
        {
            errors.Add("At least one contributor is required");
            return false;
        }

        var totalSplit = Contributors.Sum(c => c.ContributionPercentage);
        if (Math.Abs(totalSplit - 100) > 0.01m)
        {
            errors.Add($"Contributor splits must sum to 100%, currently {totalSplit}%");
            return false;
        }

        return true;
    }
}
