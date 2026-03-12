using Microsoft.EntityFrameworkCore;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for EchoTypeDefinition entity with domain-specific queries.
/// Supports soft-delete pattern for echo type management.
/// </summary>
public class EchoTypeRepository : IEchoTypeRepository
{
    private readonly MystiraAppDbContext _appContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoTypeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EchoTypeRepository(MystiraAppDbContext context)
    {
        _appContext = context;
    }

    /// <summary>
    /// Retrieves all non-deleted echo types, ordered by name.
    /// </summary>
    /// <returns>A list of echo type definitions.</returns>
    public async Task<List<EchoTypeDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _appContext.EchoTypeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves an echo type by its unique identifier.
    /// </summary>
    /// <param name="id">The echo type ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The echo type definition, or null if not found or deleted.</returns>
    public async Task<EchoTypeDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _appContext.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Retrieves an echo type by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The echo type name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The echo type definition, or null if not found or deleted.</returns>
    public async Task<EchoTypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _appContext.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Checks if an echo type with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The echo type name to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the echo type exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _appContext.EchoTypeDefinitions.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Adds a new echo type to the repository.
    /// </summary>
    /// <param name="echoType">The echo type to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(EchoTypeDefinition echoType, CancellationToken ct = default)
    {
        await _appContext.EchoTypeDefinitions.AddAsync(echoType, ct);
    }

    /// <summary>
    /// Updates an existing echo type in the repository.
    /// </summary>
    /// <param name="echoType">The echo type to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(EchoTypeDefinition echoType, CancellationToken ct = default)
    {
        _appContext.EchoTypeDefinitions.Update(echoType);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes an echo type by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the echo type to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var echoType = await _appContext.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (echoType != null)
        {
            // Soft delete instead of hard delete
            echoType.IsDeleted = true;
            echoType.UpdatedAt = DateTime.UtcNow;
            _appContext.EchoTypeDefinitions.Update(echoType);
        }
    }
}
