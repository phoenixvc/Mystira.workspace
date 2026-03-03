using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find user profiles with recent play activity.
/// Filters by LastPlayedAt being on or after the computed cutoff date, with pagination.
/// </summary>
public sealed class UserProfilesWithRecentActivitySpec : Specification<UserProfile>
{
    public UserProfilesWithRecentActivitySpec(DateTime cutoff, int page = 1, int pageSize = 50)
    {
        Query
            .Where(p => p.LastPlayedAt != null && p.LastPlayedAt >= cutoff)
            .OrderByDescending(p => p.LastPlayedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
