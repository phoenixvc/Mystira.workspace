using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserProfiles.Queries;

/// <summary>
/// Query to retrieve all user profiles for a specific account
/// </summary>
public record GetProfilesByAccountQuery(string AccountId) : IQuery<List<UserProfile>>;
