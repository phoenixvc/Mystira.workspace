using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to update an existing user profile
/// </summary>
public record UpdateUserProfileCommand(string ProfileId, UpdateUserProfileRequest Request) : ICommand<UserProfile?>;
