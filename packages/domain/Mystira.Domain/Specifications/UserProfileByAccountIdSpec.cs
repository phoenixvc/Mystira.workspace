using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find a user profile by its associated account ID.
/// Returns a single result for read-only access.
/// </summary>
public sealed class UserProfileByAccountIdSpec : Specification<UserProfile>, ISingleResultSpecification<UserProfile>
{
    public UserProfileByAccountIdSpec(string accountId)
    {
        Query
            .Where(p => p.AccountId == accountId)
            .AsNoTracking();
    }
}
