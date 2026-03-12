using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class BadgeConfigurationRepository : Repository<BadgeConfiguration>, IBadgeConfigurationRepository
{
    public BadgeConfigurationRepository(MystiraAppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BadgeConfiguration>> GetByAxisAsync(string axis, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.Axis == axis)
            .ToListAsync(ct);
    }
}
