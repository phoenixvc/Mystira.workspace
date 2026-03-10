using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class AxisAchievementRepository : Repository<AxisAchievement>, IAxisAchievementRepository
{
    public AxisAchievementRepository(MystiraAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.AgeGroupId == ageGroupId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<AxisAchievement>> GetByCompassAxisAsync(string compassAxisId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.CompassAxisId == compassAxisId)
            .ToListAsync(ct);
    }
}
