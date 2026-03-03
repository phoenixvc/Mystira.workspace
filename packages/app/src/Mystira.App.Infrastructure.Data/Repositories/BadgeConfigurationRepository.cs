using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

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
