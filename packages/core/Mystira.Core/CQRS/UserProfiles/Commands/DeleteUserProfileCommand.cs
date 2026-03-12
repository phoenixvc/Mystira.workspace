namespace Mystira.Core.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to delete a user profile
/// </summary>
public record DeleteUserProfileCommand(string ProfileId) : ICommand<bool>;
