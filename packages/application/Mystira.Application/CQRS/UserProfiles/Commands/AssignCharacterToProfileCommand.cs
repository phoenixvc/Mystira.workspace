namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to assign a character to a user profile.
/// Updates the profile's IsNpc flag based on the assignment type.
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile.</param>
/// <param name="CharacterId">The unique identifier of the character to assign.</param>
/// <param name="IsNpc">Indicates whether the character is an NPC (non-player character).</param>
public record AssignCharacterToProfileCommand(
    string ProfileId,
    string CharacterId,
    bool IsNpc = false
) : ICommand<bool>;
