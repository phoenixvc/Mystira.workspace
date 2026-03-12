using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for FantasyThemeDefinition entity with domain-specific queries.
/// Supports soft-delete pattern for fantasy theme management.
/// </summary>
public class FantasyThemeRepository : IFantasyThemeRepository
{
    private readonly MystiraAppDbContext _appContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FantasyThemeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public FantasyThemeRepository(MystiraAppDbContext context)
    {
        _appContext = context;
    }

    /// <summary>
    /// Retrieves all non-deleted fantasy themes, ordered by name.
    /// </summary>
    /// <returns>A list of fantasy theme definitions.</returns>
    public async Task<List<FantasyThemeDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _appContext.FantasyThemeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves a fantasy theme by its unique identifier.
    /// </summary>
    /// <param name="id">The fantasy theme ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fantasy theme definition, or null if not found or deleted.</returns>
    public async Task<FantasyThemeDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _appContext.FantasyThemeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Retrieves a fantasy theme by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The fantasy theme name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fantasy theme definition, or null if not found or deleted.</returns>
    public async Task<FantasyThemeDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _appContext.FantasyThemeDefinitions.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Checks if a fantasy theme with the specified name exists (case-insensitive).
    /// </summary>
    /// <param name="name">The fantasy theme name to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the fantasy theme exists and is not deleted; otherwise, false.</returns>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _appContext.FantasyThemeDefinitions.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    /// <summary>
    /// Adds a new fantasy theme to the repository.
    /// </summary>
    /// <param name="fantasyTheme">The fantasy theme to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(FantasyThemeDefinition fantasyTheme, CancellationToken ct = default)
    {
        await _appContext.FantasyThemeDefinitions.AddAsync(fantasyTheme, ct);
    }

    /// <summary>
    /// Updates an existing fantasy theme in the repository.
    /// </summary>
    /// <param name="fantasyTheme">The fantasy theme to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(FantasyThemeDefinition fantasyTheme, CancellationToken ct = default)
    {
        _appContext.FantasyThemeDefinitions.Update(fantasyTheme);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Soft deletes a fantasy theme by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the fantasy theme to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var fantasyTheme = await _appContext.FantasyThemeDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (fantasyTheme != null)
        {
            // Soft delete instead of hard delete
            fantasyTheme.IsDeleted = true;
            fantasyTheme.UpdatedAt = DateTime.UtcNow;
            _appContext.FantasyThemeDefinitions.Update(fantasyTheme);
        }
    }
}
