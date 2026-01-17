using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Badge entity with domain-specific queries.
/// Extends Repository base class and implements IBadgeRepository interface.
/// </summary>
public class BadgeRepository : Repository<Badge>, IBadgeRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BadgeRepository(MystiraAppDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all badges for a specific age group.
    /// Results are not ordered to avoid composite index requirements in Cosmos DB.
    /// </summary>
    /// <param name="ageGroupId">The age group ID.</param>
    /// <returns>A collection of badges for the specified age group.</returns>
    public async Task<IEnumerable<Badge>> GetByAgeGroupAsync(string ageGroupId)
    {
        // Avoid Cosmos ORDER BY to prevent composite index requirement; sort in memory at caller.
        return await _context.Badges
            .Where(x => x.AgeGroupId == ageGroupId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all badges for a specific compass axis, ordered by age group, tier, and tier order.
    /// </summary>
    /// <param name="compassAxisId">The compass axis ID.</param>
    /// <returns>A collection of badges for the specified compass axis.</returns>
    public async Task<IEnumerable<Badge>> GetByCompassAxisAsync(string compassAxisId)
    {
        return await _context.Badges
            .Where(x => x.CompassAxisId == compassAxisId)
            .OrderBy(x => x.AgeGroupId)
            .ThenBy(x => x.Tier)
            .ThenBy(x => x.TierOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific badge by age group, compass axis, and tier order.
    /// </summary>
    /// <param name="ageGroupId">The age group ID.</param>
    /// <param name="compassAxisId">The compass axis ID.</param>
    /// <param name="tierOrder">The tier order.</param>
    /// <returns>The matching badge, or null if not found.</returns>
    public async Task<Badge?> GetByAgeGroupAxisAndTierAsync(string ageGroupId, string compassAxisId, int tierOrder)
    {
        return await _context.Badges
            .FirstOrDefaultAsync(x => x.AgeGroupId == ageGroupId
                                   && x.CompassAxisId == compassAxisId
                                   && x.TierOrder == tierOrder);
    }
}
