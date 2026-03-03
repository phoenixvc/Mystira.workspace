namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a character/avatar is created.
/// </summary>
public sealed record CharacterCreated : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// Character display name.
    /// </summary>
    public required string CharacterName { get; init; }

    /// <summary>
    /// Character class/archetype (warrior, mage, explorer, etc.).
    /// </summary>
    public string? CharacterClass { get; init; }

    /// <summary>
    /// Whether this is the player's primary character.
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Template or preset used if any.
    /// </summary>
    public string? TemplateId { get; init; }
}

/// <summary>
/// Published when character appearance/traits are customized.
/// </summary>
public sealed record CharacterCustomized : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Category changed (appearance, personality, background).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Specific attributes changed.
    /// </summary>
    public required string[] ChangedAttributes { get; init; }
}

/// <summary>
/// Published when character gains experience/levels up.
/// </summary>
public sealed record CharacterLeveledUp : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

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
    /// Total character XP.
    /// </summary>
    public required long TotalXP { get; init; }

    /// <summary>
    /// Scenario where leveling occurred.
    /// </summary>
    public string? ScenarioId { get; init; }
}

/// <summary>
/// Published when character unlocks an ability/skill.
/// </summary>
public sealed record CharacterAbilityUnlocked : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The ability ID.
    /// </summary>
    public required string AbilityId { get; init; }

    /// <summary>
    /// Ability name.
    /// </summary>
    public required string AbilityName { get; init; }

    /// <summary>
    /// Ability category (combat, exploration, social, narrative).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// How it was unlocked (level, achievement, purchase, story).
    /// </summary>
    public required string UnlockMethod { get; init; }
}

/// <summary>
/// Published when character relationship with NPC/player changes.
/// </summary>
public sealed record CharacterRelationshipChanged : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Related entity (NPC ID or other character ID).
    /// </summary>
    public required string RelatedEntityId { get; init; }

    /// <summary>
    /// Entity type (npc, player_character).
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Relationship type (ally, rival, mentor, romantic, neutral).
    /// </summary>
    public required string RelationshipType { get; init; }

    /// <summary>
    /// Relationship level (-100 to 100).
    /// </summary>
    public required int RelationshipLevel { get; init; }

    /// <summary>
    /// Previous level.
    /// </summary>
    public int PreviousLevel { get; init; }

    /// <summary>
    /// What triggered the change.
    /// </summary>
    public string? Trigger { get; init; }
}

/// <summary>
/// Published when an alternate character is unlocked.
/// </summary>
public sealed record CharacterUnlocked : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The character template/type ID.
    /// </summary>
    public required string CharacterTemplateId { get; init; }

    /// <summary>
    /// How it was unlocked (achievement, purchase, story, event).
    /// </summary>
    public required string UnlockMethod { get; init; }

    /// <summary>
    /// Related unlock source (achievement ID, scenario ID, etc.).
    /// </summary>
    public string? SourceId { get; init; }
}

/// <summary>
/// Published when character is switched as active.
/// </summary>
public sealed record CharacterSwitched : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Previous active character.
    /// </summary>
    public string? FromCharacterId { get; init; }

    /// <summary>
    /// New active character.
    /// </summary>
    public required string ToCharacterId { get; init; }

    /// <summary>
    /// Context (story_start, player_choice, session_start).
    /// </summary>
    public required string Context { get; init; }
}

/// <summary>
/// Published when character state is saved/snapshot taken.
/// </summary>
public sealed record CharacterStateSaved : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Save point ID.
    /// </summary>
    public required string SavePointId { get; init; }

    /// <summary>
    /// Scenario ID where state was saved.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Whether this is an auto-save.
    /// </summary>
    public bool IsAutoSave { get; init; }

    /// <summary>
    /// State version for conflict resolution.
    /// </summary>
    public required int StateVersion { get; init; }
}

/// <summary>
/// Published when character is retired/deactivated.
/// </summary>
public sealed record CharacterRetired : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Reason (user_choice, story_end, deletion).
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Total playtime in seconds.
    /// </summary>
    public long TotalPlaytimeSeconds { get; init; }

    /// <summary>
    /// Stories completed with this character.
    /// </summary>
    public int StoriesCompleted { get; init; }
}

/// <summary>
/// Published when character stats/attributes change.
/// </summary>
public sealed record CharacterStatsUpdated : IntegrationEventBase
{
    /// <summary>
    /// The character ID.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Stat changed (strength, wisdom, charisma, etc.).
    /// </summary>
    public required string Stat { get; init; }

    /// <summary>
    /// Previous value.
    /// </summary>
    public required int FromValue { get; init; }

    /// <summary>
    /// New value.
    /// </summary>
    public required int ToValue { get; init; }

    /// <summary>
    /// Source of change (level_up, item, story_event).
    /// </summary>
    public required string Source { get; init; }
}
