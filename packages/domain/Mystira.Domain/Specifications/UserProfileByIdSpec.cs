using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find a user profile by its unique identifier.
/// Returns a single result for read-only access.
/// </summary>
public sealed class UserProfileByIdSpec : Specification<UserProfile>, ISingleResultSpecification<UserProfile>
{
    public UserProfileByIdSpec(string profileId)
    {
        Query
            .Where(p => p.Id == profileId)
            .AsNoTracking();
    }
}
