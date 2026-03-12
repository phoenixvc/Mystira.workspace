using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Core.Specifications;

/// <summary>Find a badge by ID.</summary>
public sealed class BadgeByIdSpec : SingleResultSpecification<Badge>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgeByIdSpec(string badgeId)
    {
        Query.Where(b => b.Id == badgeId);
    }
}

/// <summary>Find badges by age group.</summary>
public sealed class BadgesByAgeGroupSpec : Specification<Badge>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgesByAgeGroupSpec(string ageGroupId)
    {
        Query
            .Where(b => b.AgeGroupId == ageGroupId)
            .OrderBy(b => b.TierOrder);
    }
}

/// <summary>Find badges by compass axis.</summary>
public sealed class BadgesByCompassAxisSpec : Specification<Badge>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgesByCompassAxisSpec(string compassAxisId)
    {
        Query
            .Where(b => b.CompassAxisId == compassAxisId)
            .OrderBy(b => b.TierOrder);
    }
}

/// <summary>Find badges by age group and compass axis.</summary>
public sealed class BadgesByAgeGroupAndAxisSpec : Specification<Badge>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgesByAgeGroupAndAxisSpec(string ageGroupId, string compassAxisId)
    {
        Query
            .Where(b => b.AgeGroupId == ageGroupId && b.CompassAxisId == compassAxisId)
            .OrderBy(b => b.TierOrder);
    }
}

/// <summary>Find a badge by age group, compass axis, and tier.</summary>
public sealed class BadgeByAgeGroupAxisAndTierSpec : SingleResultSpecification<Badge>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgeByAgeGroupAxisAndTierSpec(string ageGroupId, string compassAxisId, string tier)
    {
        Query.Where(b =>
            b.AgeGroupId == ageGroupId &&
            b.CompassAxisId == compassAxisId &&
            b.Tier == tier);
    }
}
