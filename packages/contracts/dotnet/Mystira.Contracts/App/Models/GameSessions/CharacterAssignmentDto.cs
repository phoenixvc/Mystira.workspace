namespace Mystira.Contracts.App.Models.GameSessions;

/// <summary>
/// Data transfer object representing a character assignment in a game session.
/// </summary>
public class CharacterAssignmentDto
{
    /// <summary>
    /// The unique identifier of the character.
    /// </summary>
    public string? CharacterId { get; set; }

    /// <summary>
    /// The display name of the character.
    /// </summary>
    public string? CharacterName { get; set; }

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
    public string? Role { get; set; }

    /// <summary>
    /// The archetype classification of the character.
    /// </summary>
    public string? Archetype { get; set; }

    /// <summary>
    /// Indicates whether this character is not assigned to any player.
    /// </summary>
    public bool IsUnused { get; set; }

    /// <summary>
    /// Optional player assignment information for this character.
    /// </summary>
    public PlayerAssignmentDto? PlayerAssignment { get; set; }
}

/// <summary>
/// Data transfer object representing a player assignment to a character.
/// </summary>
public class PlayerAssignmentDto
{
    /// <summary>
    /// The type of player assignment (e.g., Profile, Guest, NPC).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Optional profile identifier if assigned to a registered profile.
    /// </summary>
    public string? ProfileId { get; set; }

    /// <summary>
    /// Optional profile name if assigned to a registered profile.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Optional URL or identifier for the profile's image.
    /// </summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Optional identifier for the selected avatar media.
    /// </summary>
    public string? SelectedAvatarMediaId { get; set; }

    /// <summary>
    /// Optional guest name if assigned to a guest player.
    /// </summary>
    public string? GuestName { get; set; }

    /// <summary>
    /// Optional age range for guest players.
    /// </summary>
    public string? GuestAgeRange { get; set; }

    /// <summary>
    /// Optional avatar identifier for guest players.
    /// </summary>
    public string? GuestAvatar { get; set; }

    /// <summary>
    /// Indicates whether to save the guest as a new profile.
    /// </summary>
    public bool SaveAsProfile { get; set; }
}
