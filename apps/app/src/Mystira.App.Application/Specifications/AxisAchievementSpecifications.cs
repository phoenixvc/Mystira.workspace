using Ardalis.Specification;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Specifications;

public sealed class AxisAchievementByIdSpec : SingleResultSpecification<AxisAchievement>
{
    public AxisAchievementByIdSpec(string id)
    {
        Query.Where(a => a.Id == id);
    }
}

public sealed class AxisAchievementsByAgeGroupSpec : Specification<AxisAchievement>
{
    public AxisAchievementsByAgeGroupSpec(string ageGroupId)
    {
        Query
            .Where(a => a.AgeGroupId == ageGroupId)
            .OrderBy(a => a.CompassAxisId);
    }
}

public sealed class AxisAchievementsByCompassAxisSpec : Specification<AxisAchievement>
{
    public AxisAchievementsByCompassAxisSpec(string compassAxisId)
    {
        Query
            .Where(a => a.CompassAxisId == compassAxisId)
            .OrderBy(a => a.AgeGroupId);
    }
}
