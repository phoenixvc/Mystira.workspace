using Mystira.Domain.Entities;
using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a mapping of players to characters in a game session.
/// </summary>
public class CharacterMap : Entity
{
    /// <summary>
    /// Gets or sets the game session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario character ID.
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player's user profile ID.
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the character's display name (may be customized by player).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the archetype ID.
    /// </summary>
    public string? ArchetypeId { get; set; }

    /// <summary>
    /// Gets or sets whether this is an AI-controlled character.
    /// </summary>
    public bool IsAiControlled { get; set; }

    /// <summary>
    /// Gets or sets the AI behavior profile (for AI characters).
    /// </summary>
    public string? AiBehaviorProfile { get; set; }

    /// <summary>
    /// Gets or sets the current compass tracking.
    /// </summary>
    public CompassTracking? CompassTracking { get; set; }

    /// <summary>
    /// Gets or sets revealed echoes for this character as JSON array of IDs.
    /// </summary>
    public string? RevealedEchosJson { get; set; }

    /// <summary>
    /// Gets or sets the character's current location/scene.
    /// </summary>
    public string? CurrentSceneId { get; set; }

    /// <summary>
    /// Gets or sets custom character attributes as JSON.
    /// </summary>
    public string? AttributesJson { get; set; }

    /// <summary>
    /// Gets the character's archetype.
    /// </summary>
    public Archetype? Archetype => Archetype.FromValue(ArchetypeId);

    /// <summary>
    /// Navigation to the scenario character.
    /// </summary>
    public virtual ScenarioCharacter? Character { get; set; }
}
