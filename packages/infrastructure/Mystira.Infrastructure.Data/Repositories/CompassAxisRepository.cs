using Microsoft.EntityFrameworkCore;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CompassAxisDefinition entity with domain-specific queries.
/// Supports soft-delete pattern for compass axis management.
/// </summary>
public class CompassAxisRepository : ICompassAxisRepository
{
    private readonly MystiraAppDbContext _appContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompassAxisRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CompassAxisRepository(MystiraAppDbContext context)
    {
        _appContext = context;
    }

    /// <summary>
    /// Retrieves all non-deleted compass axes, ordered by name.
    /// </summary>
    /// <returns>A list of compass axis definitions.</returns>
    public async Task<List<CompassAxisDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _appContext.CompassAxes
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves a compass axis definition by its unique identifier.
    /// </summary>
    /// <param name="id">The compass axis ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The compass axis definition, or null if not found or deleted.</returns>
    public async Task<CompassAxisDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _appContext.CompassAxes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Retrieves a compass axis definition by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The compass axis name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The compass axis definition, or null if not found or deleted.</returns>
    public async Task<CompassAxisDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _appContext.CompassAxes.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Checks if a compass axis definition with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The compass axis name to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the compass axis exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _appContext.CompassAxes.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Adds a new compass axis definition to the repository.
    /// </summary>
    /// <param name="axis">The compass axis definition to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(CompassAxisDefinition axis, CancellationToken ct = default)
    {
        await _appContext.CompassAxes.AddAsync(axis, ct);
    }

    /// <summary>
    /// Updates an existing compass axis definition in the repository.
    /// </summary>
    /// <param name="axis">The compass axis definition to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(CompassAxisDefinition axis, CancellationToken ct = default)
    {
        axis.UpdatedAt = DateTime.UtcNow;
        _appContext.CompassAxes.Update(axis);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes a compass axis definition by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the compass axis definition to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var axis = await _appContext.CompassAxes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (axis != null)
        {
            // Soft delete instead of hard delete
            axis.IsDeleted = true;
            axis.UpdatedAt = DateTime.UtcNow;
            _appContext.CompassAxes.Update(axis);
        }
    }
}
