using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class AgeGroupRepository : IAgeGroupRepository
{
    private readonly MystiraAppDbContext _context;

    public AgeGroupRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgeGroupDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.AgeGroupDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.MinimumAge)
            .ToListAsync(ct);
    }

    public async Task<AgeGroupDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.AgeGroupDefinitions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<AgeGroupDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.AgeGroupDefinitions.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task<AgeGroupDefinition?> GetByValueAsync(string value, CancellationToken ct = default)
    {
        return await _context.AgeGroupDefinitions.FirstOrDefaultAsync(x => x.Value == value && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.AgeGroupDefinitions.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByValueAsync(string value, CancellationToken ct = default)
    {
        return await _context.AgeGroupDefinitions.AnyAsync(x => x.Value == value && !x.IsDeleted, ct);
    }

    public async Task AddAsync(AgeGroupDefinition ageGroup, CancellationToken ct = default)
    {
        await _context.AgeGroupDefinitions.AddAsync(ageGroup, ct);
    }

    public Task UpdateAsync(AgeGroupDefinition ageGroup, CancellationToken ct = default)
    {
        _context.AgeGroupDefinitions.Update(ageGroup);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var ageGroup = await _context.AgeGroupDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (ageGroup != null)
        {
            // Soft delete instead of hard delete
            ageGroup.IsDeleted = true;
            ageGroup.UpdatedAt = DateTime.UtcNow;
            _context.AgeGroupDefinitions.Update(ageGroup);
        }
    }
}
