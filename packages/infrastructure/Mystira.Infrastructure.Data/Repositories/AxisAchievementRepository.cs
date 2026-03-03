using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for AxisAchievement entity with domain-specific queries.
/// Extends Repository base class and implements IAxisAchievementRepository interface.
/// </summary>
public class AxisAchievementRepository : Repository<AxisAchievement>, IAxisAchievementRepository
{
    private readonly MystiraAppDbContext _appContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AxisAchievementRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AxisAchievementRepository(MystiraAppDbContext context) : base(context)
    {
        _appContext = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId)
    {
        // AxisAchievement doesn't have a direct AgeGroupId property.
        // To filter by age group, we join with UserProfile.
        return await _appContext.AxisAchievements
            .Join(_appContext.UserProfiles,
                achievement => achievement.UserId,
                profile => profile.Id,
                (achievement, profile) => new { achievement, profile })
            .Where(x => x.profile.AgeGroupId == ageGroupId)
            .Select(x => x.achievement)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AxisAchievement>> GetByCompassAxisAsync(string compassAxisId)
    {
        // CompassAxisId is an alias for AxisId in the AxisAchievement model
        return await _appContext.AxisAchievements
            .Where(x => x.AxisId == compassAxisId)
            .ToListAsync();
    }
}
