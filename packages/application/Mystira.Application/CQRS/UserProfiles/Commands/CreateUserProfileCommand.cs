using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to create a new user profile
/// </summary>
/// <param name="Request">The request containing the user profile data to create.</param>
public record CreateUserProfileCommand(CreateUserProfileRequest Request) : ICommand<UserProfile>;
