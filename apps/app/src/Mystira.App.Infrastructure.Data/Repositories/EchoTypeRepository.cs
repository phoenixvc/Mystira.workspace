using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class EchoTypeRepository : IEchoTypeRepository
{
    private readonly MystiraAppDbContext _context;

    public EchoTypeRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EchoTypeDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.EchoTypeDefinitions
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<EchoTypeDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<EchoTypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.EchoTypeDefinitions.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task AddAsync(EchoTypeDefinition echoType, CancellationToken ct = default)
    {
        await _context.EchoTypeDefinitions.AddAsync(echoType, ct);
    }

    public Task UpdateAsync(EchoTypeDefinition echoType, CancellationToken ct = default)
    {
        _context.EchoTypeDefinitions.Update(echoType);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var echoType = await _context.EchoTypeDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (echoType != null)
        {
            // Soft delete instead of hard delete
            echoType.IsDeleted = true;
            echoType.UpdatedAt = DateTime.UtcNow;
            _context.EchoTypeDefinitions.Update(echoType);
        }
    }
}
