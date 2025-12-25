namespace Mystira.Contracts.App.Models;

/// <summary>
/// Data transfer object representing a character assignment in a game session.
/// </summary>
public record CharacterAssignmentDto
{
    /// <summary>
    /// The unique identifier of the character.
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the character.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Optional URL or identifier for the character's image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional URL or identifier for the character's audio.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// The role of the character in the story.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The archetype classification of the character.
    /// </summary>
    public string Archetype { get; set; } = string.Empty;

    /// <summary>
    /// Optional player assignment information for this character.
    /// </summary>
    public PlayerAssignmentDto? PlayerAssignment { get; set; }
}

/// <summary>
/// Data transfer object representing a player assignment to a character.
/// </summary>
public record PlayerAssignmentDto
{
    /// <summary>
    /// The type of player assignment (e.g., Profile, Guest, NPC).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional profile identifier if assigned to a registered profile.
    /// </summary>
    public string? ProfileId { get; set; }

    /// <summary>
    /// Optional profile name if assigned to a registered profile.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Optional guest name if assigned to a guest player.
    /// </summary>
    public string? GuestName { get; set; }
}
