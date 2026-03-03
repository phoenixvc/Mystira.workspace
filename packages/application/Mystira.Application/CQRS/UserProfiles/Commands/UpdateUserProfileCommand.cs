using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to update an existing user profile
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile to update.</param>
/// <param name="Request">The request containing the updated user profile data.</param>
public record UpdateUserProfileCommand(string ProfileId, UpdateUserProfileRequest Request) : ICommand<UserProfile?>;
