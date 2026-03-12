using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class BadgeRepository : Repository<Badge>, IBadgeRepository
{
    public BadgeRepository(MystiraAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Badge>> GetByAgeGroupAsync(string ageGroupId, CancellationToken ct = default)
    {
        // Avoid Cosmos ORDER BY to prevent composite index requirement; sort in memory at caller.
        return await _dbSet
            .Where(x => x.AgeGroupId == ageGroupId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Badge>> GetByCompassAxisAsync(string compassAxisId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.CompassAxisId == compassAxisId)
            .OrderBy(x => x.AgeGroupId)
            .ThenBy(x => x.Tier)
            .ThenBy(x => x.TierOrder)
            .ToListAsync(ct);
    }

    public async Task<Badge?> GetByAgeGroupAxisAndTierAsync(string ageGroupId, string compassAxisId, int tierOrder, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.AgeGroupId == ageGroupId
                                   && x.CompassAxisId == compassAxisId
                                   && x.TierOrder == tierOrder, ct);
    }
}
