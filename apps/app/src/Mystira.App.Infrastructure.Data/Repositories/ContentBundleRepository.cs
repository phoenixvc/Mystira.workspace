using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ContentBundle entity
/// </summary>
public class ContentBundleRepository : Repository<ContentBundle>, IContentBundleRepository
{
    public ContentBundleRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ContentBundle>> GetByAgeGroupAsync(string ageGroup, CancellationToken ct = default)
    {
        return await _dbSet.Where(b => b.AgeGroupId == ageGroup).ToListAsync(ct);
    }
}
