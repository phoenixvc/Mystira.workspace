using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to update an existing user profile
/// </summary>
public record UpdateUserProfileCommand(string ProfileId, UpdateUserProfileRequest Request) : ICommand<UserProfile?>;
