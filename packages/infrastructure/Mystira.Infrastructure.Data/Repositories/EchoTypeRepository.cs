using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for EchoTypeDefinition entity with domain-specific queries.
/// Supports soft-delete pattern for echo type management.
/// </summary>
public class EchoTypeRepository : IEchoTypeRepository
{
    private readonly MystiraAppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoTypeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EchoTypeRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all non-deleted echo types, ordered by name.
    /// </summary>
    /// <returns>A list of echo type definitions.</returns>
    public async Task<List<EchoTypeDefinition>> GetAllAsync()
    {
        return await _context.EchoTypeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves an echo type by its unique identifier.
    /// </summary>
    /// <param name="id">The echo type ID.</param>
    /// <returns>The echo type definition, or null if not found or deleted.</returns>
    public async Task<EchoTypeDefinition?> GetByIdAsync(string id)
    {
        return await _context.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    /// <summary>
    /// Retrieves an echo type by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The echo type name.</param>
    /// <returns>The echo type definition, or null if not found or deleted.</returns>
    public async Task<EchoTypeDefinition?> GetByNameAsync(string name)
    {
        return await _context.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Checks if an echo type with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The echo type name to check.</param>
    /// <returns>True if the echo type exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.EchoTypeDefinitions.AnyAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
    }

    /// <summary>
    /// Adds a new echo type to the repository.
    /// </summary>
    /// <param name="echoType">The echo type to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(EchoTypeDefinition echoType)
    {
        await _context.EchoTypeDefinitions.AddAsync(echoType);
    }

    /// <summary>
    /// Updates an existing echo type in the repository.
    /// </summary>
    /// <param name="echoType">The echo type to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(EchoTypeDefinition echoType)
    {
        _context.EchoTypeDefinitions.Update(echoType);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes an echo type by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the echo type to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id)
    {
        var echoType = await _context.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (echoType != null)
        {
            // Soft delete instead of hard delete
            echoType.IsDeleted = true;
            echoType.UpdatedAt = DateTime.UtcNow;
            _context.EchoTypeDefinitions.Update(echoType);
        }
    }
}
