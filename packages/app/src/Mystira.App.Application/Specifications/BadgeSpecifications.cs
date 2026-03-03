using Ardalis.Specification;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Specifications;

public sealed class BadgeByIdSpec : SingleResultSpecification<Badge>
{
    public BadgeByIdSpec(string badgeId)
    {
        Query.Where(b => b.Id == badgeId);
    }
}

public sealed class BadgesByAgeGroupSpec : Specification<Badge>
{
    public BadgesByAgeGroupSpec(string ageGroupId)
    {
        Query
            .Where(b => b.AgeGroupId == ageGroupId)
            .OrderBy(b => b.TierOrder);
    }
}

public sealed class BadgesByCompassAxisSpec : Specification<Badge>
{
    public BadgesByCompassAxisSpec(string compassAxisId)
    {
        Query
            .Where(b => b.CompassAxisId == compassAxisId)
            .OrderBy(b => b.TierOrder);
    }
}

public sealed class BadgesByAgeGroupAndAxisSpec : Specification<Badge>
{
    public BadgesByAgeGroupAndAxisSpec(string ageGroupId, string compassAxisId)
    {
        Query
            .Where(b => b.AgeGroupId == ageGroupId && b.CompassAxisId == compassAxisId)
            .OrderBy(b => b.TierOrder);
    }
}

public sealed class BadgeByAgeGroupAxisAndTierSpec : SingleResultSpecification<Badge>
{
    public BadgeByAgeGroupAxisAndTierSpec(string ageGroupId, string compassAxisId, string tier)
    {
        Query.Where(b =>
            b.AgeGroupId == ageGroupId &&
            b.CompassAxisId == compassAxisId &&
            b.Tier == tier);
    }
}
