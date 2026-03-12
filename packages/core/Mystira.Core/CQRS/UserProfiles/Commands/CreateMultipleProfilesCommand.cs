using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to create multiple user profiles in a batch operation.
/// Used during onboarding when creating profiles for family members.
/// </summary>
/// <param name="Request">The request containing the data for creating multiple user profiles.</param>
public record CreateMultipleProfilesCommand(
    CreateMultipleProfilesRequest Request
) : ICommand<List<UserProfile>>;
