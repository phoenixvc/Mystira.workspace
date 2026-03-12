using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class ArchetypeRepository : IArchetypeRepository
{
    private readonly MystiraAppDbContext _context;

    public ArchetypeRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArchetypeDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ArchetypeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<ArchetypeDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.ArchetypeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<ArchetypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.ArchetypeDefinitions.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.ArchetypeDefinitions.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task AddAsync(ArchetypeDefinition archetype, CancellationToken ct = default)
    {
        await _context.ArchetypeDefinitions.AddAsync(archetype, ct);
    }

    public Task UpdateAsync(ArchetypeDefinition archetype, CancellationToken ct = default)
    {
        _context.ArchetypeDefinitions.Update(archetype);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var archetype = await _context.ArchetypeDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (archetype != null)
        {
            // Soft delete instead of hard delete
            archetype.IsDeleted = true;
            archetype.UpdatedAt = DateTime.UtcNow;
            _context.ArchetypeDefinitions.Update(archetype);
        }
    }
}
