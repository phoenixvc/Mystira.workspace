using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for AgeGroupDefinition entity with domain-specific queries.
/// Supports soft-delete pattern for age group management.
/// </summary>
public class AgeGroupRepository : IAgeGroupRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgeGroupRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AgeGroupRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all non-deleted age groups, ordered by minimum age.
    /// </summary>
    /// <returns>A list of age group definitions.</returns>
    public async Task<List<AgeGroupDefinition>> GetAllAsync()
    {
        return await _context.AgeGroupDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.MinimumAge)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves an age group by its unique identifier.
    /// </summary>
    /// <param name="id">The age group ID.</param>
    /// <returns>The age group definition, or null if not found or deleted.</returns>
    public async Task<AgeGroupDefinition?> GetByIdAsync(string id)
    {
        return await _context.AgeGroupDefinitions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    /// <summary>
    /// Retrieves an age group by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The age group name.</param>
    /// <returns>The age group definition, or null if not found or deleted.</returns>
    public async Task<AgeGroupDefinition?> GetByNameAsync(string name)
    {
        return await _context.AgeGroupDefinitions.FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Retrieves an age group by its value identifier.
    /// </summary>
    /// <param name="value">The age group value.</param>
    /// <returns>The age group definition, or null if not found or deleted.</returns>
    public async Task<AgeGroupDefinition?> GetByValueAsync(string value)
    {
        return await _context.AgeGroupDefinitions.FirstOrDefaultAsync(x => x.Value == value && !x.IsDeleted);
    }

    /// <summary>
    /// Checks if an age group with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The age group name to check.</param>
    /// <returns>True if the age group exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.AgeGroupDefinitions.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Checks if an age group with the specified value exists.
    /// </summary>
    /// <param name="value">The age group value to check.</param>
    /// <returns>True if the age group exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByValueAsync(string value)
    {
        return await _context.AgeGroupDefinitions.AnyAsync(x => x.Value == value && !x.IsDeleted);
    }

    /// <summary>
    /// Adds a new age group to the repository.
    /// </summary>
    /// <param name="ageGroup">The age group to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(AgeGroupDefinition ageGroup)
    {
        await _context.AgeGroupDefinitions.AddAsync(ageGroup);
    }

    /// <summary>
    /// Updates an existing age group in the repository.
    /// </summary>
    /// <param name="ageGroup">The age group to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(AgeGroupDefinition ageGroup)
    {
        _context.AgeGroupDefinitions.Update(ageGroup);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes an age group by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the age group to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var ageGroup = await _context.AgeGroupDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (ageGroup != null)
        {
            // Soft delete instead of hard delete
            ageGroup.IsDeleted = true;
            ageGroup.UpdatedAt = DateTime.UtcNow;
            _context.AgeGroupDefinitions.Update(ageGroup);
        }
    }
}
