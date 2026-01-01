using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CompassAxis entity with domain-specific queries.
/// Supports soft-delete pattern for compass axis management.
/// </summary>
public class CompassAxisRepository : ICompassAxisRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompassAxisRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CompassAxisRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all non-deleted compass axes, ordered by name.
    /// </summary>
    /// <returns>A list of compass axes.</returns>
    public async Task<List<CompassAxis>> GetAllAsync()
    {
        return await _context.CompassAxes
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a compass axis by its unique identifier.
    /// </summary>
    /// <param name="id">The compass axis ID.</param>
    /// <returns>The compass axis, or null if not found or deleted.</returns>
    public async Task<CompassAxis?> GetByIdAsync(string id)
    {
        return await _context.CompassAxes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    /// <summary>
    /// Retrieves a compass axis by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The compass axis name.</param>
    /// <returns>The compass axis, or null if not found or deleted.</returns>
    public async Task<CompassAxis?> GetByNameAsync(string name)
    {
        return await _context.CompassAxes.FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Checks if a compass axis with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The compass axis name to check.</param>
    /// <returns>True if the compass axis exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.CompassAxes.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Adds a new compass axis to the repository.
    /// </summary>
    /// <param name="axis">The compass axis to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(CompassAxis axis)
    {
        await _context.CompassAxes.AddAsync(axis);
    }

    /// <summary>
    /// Updates an existing compass axis in the repository.
    /// </summary>
    /// <param name="axis">The compass axis to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(CompassAxis axis)
    {
        _context.CompassAxes.Update(axis);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes a compass axis by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the compass axis to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var axis = await _context.CompassAxes.FirstOrDefaultAsync(x => x.Id == id);
        if (axis != null)
        {
            // Soft delete instead of hard delete
            axis.IsDeleted = true;
            axis.UpdatedAt = DateTime.UtcNow;
            _context.CompassAxes.Update(axis);
        }
    }
}
