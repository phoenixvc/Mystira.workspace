using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for AxisAchievement entity with domain-specific queries.
/// Supports specification pattern for flexible querying.
/// </summary>
public class AxisAchievementRepository : IAxisAchievementRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AxisAchievementRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AxisAchievementRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves an axis achievement by its unique identifier.
    /// </summary>
    /// <param name="id">The axis achievement ID.</param>
    /// <returns>The axis achievement, or null if not found.</returns>
    public async Task<AxisAchievement?> GetByIdAsync(string id)
    {
        return await _context.AxisAchievements.FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Retrieves all axis achievements.
    /// </summary>
    /// <returns>A collection of all axis achievements.</returns>
    public async Task<IEnumerable<AxisAchievement>> GetAllAsync()
    {
        return await _context.AxisAchievements.ToListAsync();
    }

    /// <summary>
    /// Finds axis achievements matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A collection of matching axis achievements.</returns>
    public async Task<IEnumerable<AxisAchievement>> FindAsync(System.Linq.Expressions.Expression<Func<AxisAchievement, bool>> predicate)
    {
        return await _context.AxisAchievements.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Adds a new axis achievement to the repository.
    /// </summary>
    /// <param name="entity">The axis achievement to add.</param>
    /// <returns>The added axis achievement.</returns>
    public async Task<AxisAchievement> AddAsync(AxisAchievement entity)
    {
        await _context.AxisAchievements.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Updates an existing axis achievement in the repository.
    /// </summary>
    /// <param name="entity">The axis achievement to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(AxisAchievement entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.AxisAchievements.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an axis achievement from the repository.
    /// </summary>
    /// <param name="id">The ID of the axis achievement to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.AxisAchievements.Remove(entity);
        }
    }

    /// <summary>
    /// Checks if an axis achievement with the specified ID exists.
    /// </summary>
    /// <param name="id">The axis achievement ID to check.</param>
    /// <returns>True if the axis achievement exists; otherwise, false.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.AxisAchievements.AnyAsync(x => x.Id == id);
    }

    /// <summary>
    /// Retrieves a single axis achievement matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The axis achievement, or null if not found.</returns>
    public async Task<AxisAchievement?> GetBySpecAsync(ISpecification<AxisAchievement> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves all axis achievements matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>A collection of matching axis achievements.</returns>
    public async Task<IEnumerable<AxisAchievement>> ListAsync(ISpecification<AxisAchievement> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    /// <summary>
    /// Counts axis achievements matching the specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The count of matching axis achievements.</returns>
    public async Task<int> CountAsync(ISpecification<AxisAchievement> spec)
    {
        return await ApplySpecification(spec).CountAsync();
    }

    /// <summary>
    /// Retrieves all axis achievements for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of axis achievements for the specified user.</returns>
    public async Task<IEnumerable<AxisAchievement>> GetByUserIdAsync(string userId)
    {
        return await _context.AxisAchievements
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all axis achievements for a specific compass axis.
    /// </summary>
    /// <param name="axisId">The axis ID (compass axis identifier).</param>
    /// <returns>A collection of axis achievements for the specified compass axis.</returns>
    public async Task<IEnumerable<AxisAchievement>> GetByAxisIdAsync(string axisId)
    {
        // AxisAchievement uses AxisId (with CompassAxisId as an alias)
        return await _context.AxisAchievements
            .Where(x => x.AxisId == axisId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId)
    {
        // AxisAchievement doesn't have a direct AgeGroupId property.
        // To filter by age group, we would need to join with UserProfile.
        // For now, return achievements for users in the specified age group.
        return await _context.AxisAchievements
            .Join(_context.UserProfiles,
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
        return await GetByAxisIdAsync(compassAxisId);
    }

    private IQueryable<AxisAchievement> ApplySpecification(ISpecification<AxisAchievement> spec)
    {
        return SpecificationEvaluator.Default.GetQuery(_context.AxisAchievements.AsQueryable(), spec);
    }
}
