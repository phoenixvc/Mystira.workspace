using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Badge entity with domain-specific queries.
/// Supports specification pattern for flexible querying.
/// </summary>
public class BadgeRepository : IBadgeRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BadgeRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a badge by its unique identifier.
    /// </summary>
    /// <param name="id">The badge ID.</param>
    /// <returns>The badge, or null if not found.</returns>
    public async Task<Badge?> GetByIdAsync(string id)
    {
        return await _context.Badges.FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Retrieves all badges.
    /// </summary>
    /// <returns>A collection of all badges.</returns>
    public async Task<IEnumerable<Badge>> GetAllAsync()
    {
        return await _context.Badges.ToListAsync();
    }

    /// <summary>
    /// Finds badges matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A collection of matching badges.</returns>
    public async Task<IEnumerable<Badge>> FindAsync(System.Linq.Expressions.Expression<Func<Badge, bool>> predicate)
    {
        return await _context.Badges.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Adds a new badge to the repository.
    /// </summary>
    /// <param name="entity">The badge to add.</param>
    /// <returns>The added badge.</returns>
    public async Task<Badge> AddAsync(Badge entity)
    {
        await _context.Badges.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Updates an existing badge in the repository.
    /// </summary>
    /// <param name="entity">The badge to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(Badge entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Badges.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a badge from the repository.
    /// </summary>
    /// <param name="id">The ID of the badge to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Badges.Remove(entity);
        }
    }

    /// <summary>
    /// Checks if a badge with the specified ID exists.
    /// </summary>
    /// <param name="id">The badge ID to check.</param>
    /// <returns>True if the badge exists; otherwise, false.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Badges.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Retrieves a single badge matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The badge, or null if not found.</returns>
    public async Task<Badge?> GetBySpecAsync(ISpecification<Badge> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves all badges matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>A collection of matching badges.</returns>
    public async Task<IEnumerable<Badge>> ListAsync(ISpecification<Badge> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    /// <summary>
    /// Counts badges matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The count of matching badges.</returns>
    public async Task<int> CountAsync(ISpecification<Badge> spec)
    {
        return await ApplySpecification(spec).CountAsync();
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

    private IQueryable<Badge> ApplySpecification(ISpecification<Badge> spec)
    {
        return SpecificationEvaluator.Default.GetQuery(_context.Badges.AsQueryable(), spec);
    }
}
