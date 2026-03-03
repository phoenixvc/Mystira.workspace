using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

public class CompassAxisRepository : ICompassAxisRepository
{
    private readonly MystiraAppDbContext _context;

    public CompassAxisRepository(MystiraAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompassAxis>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.CompassAxes
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<CompassAxis?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.CompassAxes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<CompassAxis?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.CompassAxes.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.CompassAxes.AnyAsync(x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, ct);
    }

    public async Task AddAsync(CompassAxis axis, CancellationToken ct = default)
    {
        await _context.CompassAxes.AddAsync(axis, ct);
    }

    public Task UpdateAsync(CompassAxis axis, CancellationToken ct = default)
    {
        _context.CompassAxes.Update(axis);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var axis = await _context.CompassAxes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (axis != null)
        {
            // Soft delete instead of hard delete
            axis.IsDeleted = true;
            axis.UpdatedAt = DateTime.UtcNow;
            _context.CompassAxes.Update(axis);
        }
    }
}
