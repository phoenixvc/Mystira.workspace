namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to delete a user profile
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile to delete.</param>
public record DeleteUserProfileCommand(string ProfileId) : ICommand<bool>;
