namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user begins a multi-story arc.
/// </summary>
public sealed record StoryArcStarted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The story arc ID (collection of connected scenarios).
    /// </summary>
    public required string StoryArcId { get; init; }

    /// <summary>
    /// Story arc name.
    /// </summary>
    public required string ArcName { get; init; }

    /// <summary>
    /// First scenario in the arc.
    /// </summary>
    public required string FirstScenarioId { get; init; }

    /// <summary>
    /// Total scenarios in this arc.
    /// </summary>
    public required int TotalScenarios { get; init; }

    /// <summary>
    /// Character used for this arc.
    /// </summary>
    public string? CharacterId { get; init; }
}

/// <summary>
/// Published when user progresses through a story arc.
/// </summary>
public sealed record StoryArcProgressed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The story arc ID.
    /// </summary>
    public required string StoryArcId { get; init; }

    /// <summary>
    /// Scenario just completed.
    /// </summary>
    public required string CompletedScenarioId { get; init; }

    /// <summary>
    /// Next scenario unlocked if any.
    /// </summary>
    public string? NextScenarioId { get; init; }

    /// <summary>
    /// Scenarios completed so far.
    /// </summary>
    public required int ScenariosCompleted { get; init; }

    /// <summary>
    /// Total scenarios in arc.
    /// </summary>
    public required int TotalScenarios { get; init; }

    /// <summary>
    /// Arc completion percentage.
    /// </summary>
    public required int ProgressPercent { get; init; }
}

/// <summary>
/// Published when user completes a story arc.
/// </summary>
public sealed record StoryArcCompleted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The story arc ID.
    /// </summary>
    public required string StoryArcId { get; init; }

    /// <summary>
    /// Total duration across all scenarios.
    /// </summary>
    public required int TotalDurationSeconds { get; init; }

    /// <summary>
    /// Scenarios completed in this arc.
    /// </summary>
    public required int ScenariosCompleted { get; init; }

    /// <summary>
    /// Arc ending/outcome achieved.
    /// </summary>
    public required string Ending { get; init; }

    /// <summary>
    /// XP earned for arc completion.
    /// </summary>
    public int XpReward { get; init; }

    /// <summary>
    /// Items/rewards unlocked.
    /// </summary>
    public string[]? Rewards { get; init; }
}

/// <summary>
/// Published when character state is carried between stories.
/// </summary>
public sealed record CharacterStateCarriedOver : IntegrationEventBase
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
    /// Source scenario.
    /// </summary>
    public required string FromScenarioId { get; init; }

    /// <summary>
    /// Target scenario.
    /// </summary>
    public required string ToScenarioId { get; init; }

    /// <summary>
    /// State elements carried (relationships, items, knowledge, stats).
    /// </summary>
    public required string[] CarriedElements { get; init; }

    /// <summary>
    /// Story arc if applicable.
    /// </summary>
    public string? StoryArcId { get; init; }
}

/// <summary>
/// Published when player acquires knowledge that persists across stories.
/// </summary>
public sealed record PersistentKnowledgeAcquired : IntegrationEventBase
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
    /// Knowledge ID.
    /// </summary>
    public required string KnowledgeId { get; init; }

    /// <summary>
    /// Knowledge name/type (lore, secret, skill, language).
    /// </summary>
    public required string KnowledgeType { get; init; }

    /// <summary>
    /// Description of what was learned.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Scenario where acquired.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Other scenarios this knowledge affects.
    /// </summary>
    public string[]? AffectsScenarios { get; init; }
}

/// <summary>
/// Published when a story/scenario is unlocked based on previous progress.
/// </summary>
public sealed record StorySequenceUnlocked : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Scenario that is now unlocked.
    /// </summary>
    public required string UnlockedScenarioId { get; init; }

    /// <summary>
    /// Prerequisite scenario that was completed.
    /// </summary>
    public required string PrerequisiteScenarioId { get; init; }

    /// <summary>
    /// Specific ending/branch that unlocked it.
    /// </summary>
    public string? RequiredEnding { get; init; }

    /// <summary>
    /// Story arc if applicable.
    /// </summary>
    public string? StoryArcId { get; init; }

    /// <summary>
    /// Character that unlocked it.
    /// </summary>
    public string? CharacterId { get; init; }
}

/// <summary>
/// Published when a world/universe event affects all players.
/// </summary>
public sealed record UniverseEventOccurred : IntegrationEventBase
{
    /// <summary>
    /// Universe event ID.
    /// </summary>
    public required string EventId { get; init; }

    /// <summary>
    /// Event name.
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Event type (seasonal, story_milestone, community_achievement).
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Scenarios affected by this event.
    /// </summary>
    public string[]? AffectedScenarios { get; init; }

    /// <summary>
    /// When the event becomes active.
    /// </summary>
    public required DateTimeOffset StartsAt { get; init; }

    /// <summary>
    /// When the event ends (null = permanent).
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>
    /// Trigger source (community, calendar, admin).
    /// </summary>
    public required string TriggerSource { get; init; }
}

/// <summary>
/// Published when a player's choice affects others' story worlds.
/// </summary>
public sealed record CrossPlayerChoiceImpact : IntegrationEventBase
{
    /// <summary>
    /// Player who made the choice.
    /// </summary>
    public required string ActorAccountId { get; init; }

    /// <summary>
    /// The choice made.
    /// </summary>
    public required string ChoiceId { get; init; }

    /// <summary>
    /// Scenario where choice was made.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Impact type (world_state, npc_availability, story_branch).
    /// </summary>
    public required string ImpactType { get; init; }

    /// <summary>
    /// Players affected by this choice.
    /// </summary>
    public required string[] AffectedAccountIds { get; init; }

    /// <summary>
    /// Description of the impact.
    /// </summary>
    public string? ImpactDescription { get; init; }
}

/// <summary>
/// Published when relationships carry over between stories.
/// </summary>
public sealed record RelationshipCarriedOver : IntegrationEventBase
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
    /// Related NPC or character.
    /// </summary>
    public required string RelatedEntityId { get; init; }

    /// <summary>
    /// Relationship level being carried.
    /// </summary>
    public required int RelationshipLevel { get; init; }

    /// <summary>
    /// Source scenario.
    /// </summary>
    public required string FromScenarioId { get; init; }

    /// <summary>
    /// Target scenario.
    /// </summary>
    public required string ToScenarioId { get; init; }

    /// <summary>
    /// Whether this affects story options.
    /// </summary>
    public bool UnlocksDialogue { get; init; }
}

/// <summary>
/// Published when a metanarrative milestone is reached.
/// </summary>
public sealed record MetanarrativeMilestone : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Milestone ID.
    /// </summary>
    public required string MilestoneId { get; init; }

    /// <summary>
    /// Milestone name.
    /// </summary>
    public required string MilestoneName { get; init; }

    /// <summary>
    /// Milestone type (arcs_completed, endings_collected, lore_discovered).
    /// </summary>
    public required string MilestoneType { get; init; }

    /// <summary>
    /// Progress value.
    /// </summary>
    public required int Value { get; init; }

    /// <summary>
    /// Rewards unlocked.
    /// </summary>
    public string[]? Rewards { get; init; }
}
