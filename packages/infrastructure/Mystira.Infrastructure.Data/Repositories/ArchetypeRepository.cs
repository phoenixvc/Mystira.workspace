using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ArchetypeDefinition entity with domain-specific queries.
/// Supports soft-delete pattern for archetype management.
/// </summary>
public class ArchetypeRepository : IArchetypeRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchetypeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ArchetypeRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all non-deleted archetypes, ordered by name.
    /// </summary>
    /// <returns>A list of archetype definitions.</returns>
    public async Task<List<ArchetypeDefinition>> GetAllAsync()
    {
        return await _context.ArchetypeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves an archetype by its unique identifier.
    /// </summary>
    /// <param name="id">The archetype ID.</param>
    /// <returns>The archetype definition, or null if not found or deleted.</returns>
    public async Task<ArchetypeDefinition?> GetByIdAsync(string id)
    {
        return await _context.ArchetypeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    /// <summary>
    /// Retrieves an archetype by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The archetype name.</param>
    /// <returns>The archetype definition, or null if not found or deleted.</returns>
    public async Task<ArchetypeDefinition?> GetByNameAsync(string name)
    {
        return await _context.ArchetypeDefinitions.FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Checks if an archetype with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The archetype name to check.</param>
    /// <returns>True if the archetype exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.ArchetypeDefinitions.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Adds a new archetype to the repository.
    /// </summary>
    /// <param name="archetype">The archetype to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(ArchetypeDefinition archetype)
    {
        await _context.ArchetypeDefinitions.AddAsync(archetype);
    }

    /// <summary>
    /// Updates an existing archetype in the repository.
    /// </summary>
    /// <param name="archetype">The archetype to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(ArchetypeDefinition archetype)
    {
        _context.ArchetypeDefinitions.Update(archetype);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes an archetype by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the archetype to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var archetype = await _context.ArchetypeDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (archetype != null)
        {
            // Soft delete instead of hard delete
            archetype.IsDeleted = true;
            archetype.UpdatedAt = DateTime.UtcNow;
            _context.ArchetypeDefinitions.Update(archetype);
        }
    }
}
