using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserProfiles.Queries;

/// <summary>
/// Query to retrieve a user profile by ID
/// </summary>
public record GetUserProfileQuery(string ProfileId) : IQuery<UserProfile?>;
