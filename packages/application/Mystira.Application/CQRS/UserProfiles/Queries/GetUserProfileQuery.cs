using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserProfiles.Queries;

/// <summary>
/// Query to retrieve a user profile by ID
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile.</param>
public record GetUserProfileQuery(string ProfileId) : IQuery<UserProfile?>;
