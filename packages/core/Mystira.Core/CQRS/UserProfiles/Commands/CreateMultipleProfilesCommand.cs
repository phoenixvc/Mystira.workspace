using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to create multiple user profiles in a batch operation.
/// Used during onboarding when creating profiles for family members.
/// </summary>
public record CreateMultipleProfilesCommand(
    CreateMultipleProfilesRequest Request
) : ICommand<List<UserProfile>>;
