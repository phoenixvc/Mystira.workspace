using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class FantasyThemeRepository : IFantasyThemeRepository
{
    private readonly MystiraAppDbContext _context;

    public FantasyThemeRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FantasyThemeDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.FantasyThemeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<FantasyThemeDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.FantasyThemeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<FantasyThemeDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.FantasyThemeDefinitions.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.FantasyThemeDefinitions.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task AddAsync(FantasyThemeDefinition fantasyTheme, CancellationToken ct = default)
    {
        await _context.FantasyThemeDefinitions.AddAsync(fantasyTheme, ct);
    }

    public Task UpdateAsync(FantasyThemeDefinition fantasyTheme, CancellationToken ct = default)
    {
        _context.FantasyThemeDefinitions.Update(fantasyTheme);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var fantasyTheme = await _context.FantasyThemeDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (fantasyTheme != null)
        {
            // Soft delete instead of hard delete
            fantasyTheme.IsDeleted = true;
            fantasyTheme.UpdatedAt = DateTime.UtcNow;
            _context.FantasyThemeDefinitions.Update(fantasyTheme);
        }
    }
}
