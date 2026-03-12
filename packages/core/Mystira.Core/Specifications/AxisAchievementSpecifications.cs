using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Core.Specifications;

/// <summary>Find an axis achievement by ID.</summary>
public sealed class AxisAchievementByIdSpec : SingleResultSpecification<AxisAchievement>
{
    /// <summary>Initializes a new instance.</summary>
    public AxisAchievementByIdSpec(string id)
    {
        Query.Where(a => a.Id == id);
    }
}

/// <summary>Find axis achievements by age group.</summary>
public sealed class AxisAchievementsByAgeGroupSpec : Specification<AxisAchievement>
{
    /// <summary>Initializes a new instance.</summary>
    public AxisAchievementsByAgeGroupSpec(string ageGroupId)
    {
        Query
            .Where(a => a.AgeGroupId == ageGroupId)
            .OrderBy(a => a.CompassAxisId);
    }
}

/// <summary>Find axis achievements by compass axis.</summary>
public sealed class AxisAchievementsByCompassAxisSpec : Specification<AxisAchievement>
{
    /// <summary>Initializes a new instance.</summary>
    public AxisAchievementsByCompassAxisSpec(string compassAxisId)
    {
        Query
            .Where(a => a.CompassAxisId == compassAxisId)
            .OrderBy(a => a.AgeGroupId);
    }
}
