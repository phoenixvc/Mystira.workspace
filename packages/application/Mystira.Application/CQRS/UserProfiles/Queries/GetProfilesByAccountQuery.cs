using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserProfiles.Queries;

/// <summary>
/// Query to retrieve all user profiles for a specific account
/// </summary>
/// <param name="AccountId">The unique identifier of the account.</param>
public record GetProfilesByAccountQuery(string AccountId) : IQuery<List<UserProfile>>;
