using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification to filter user badges by profile ID.
/// Migrated from UserBadgesByProfileSpecification.
/// </summary>
public sealed class UserBadgesByProfileSpec : BaseEntitySpecification<UserBadge>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserBadgesByProfileSpec"/> class.
    /// </summary>
    /// <param name="userProfileId">The user profile identifier to filter by.</param>
    public UserBadgesByProfileSpec(string userProfileId)
    {
        Query.Where(b => b.UserProfileId == userProfileId)
             .OrderByDescending(b => b.EarnedAt);
    }
}

/// <summary>
/// Specification to filter user badges by profile and axis.
/// Migrated from UserBadgesByAxisSpecification.
/// </summary>
public sealed class UserBadgesByAxisSpec : BaseEntitySpecification<UserBadge>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserBadgesByAxisSpec"/> class.
    /// </summary>
    /// <param name="userProfileId">The user profile identifier to filter by.</param>
    /// <param name="axis">The axis to filter by.</param>
    public UserBadgesByAxisSpec(string userProfileId, string axis)
    {
        Query.Where(b => b.UserProfileId == userProfileId && b.Axis == axis)
             .OrderByDescending(b => b.EarnedAt);
    }
}

/// <summary>
/// Specification to get a user badge by ID.
/// </summary>
public sealed class UserBadgeByIdSpec : SingleEntitySpecification<UserBadge>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserBadgeByIdSpec"/> class.
    /// </summary>
    /// <param name="id">The user badge identifier.</param>
    public UserBadgeByIdSpec(string id)
    {
        Query.Where(b => b.Id == id);
    }
}
