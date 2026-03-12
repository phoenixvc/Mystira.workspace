using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to create a new user profile
/// </summary>
public record CreateUserProfileCommand(CreateUserProfileRequest Request) : ICommand<UserProfile>;
