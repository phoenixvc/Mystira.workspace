using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ContentBundle entity
/// </summary>
public class ContentBundleRepository : Repository<ContentBundle>, IContentBundleRepository
{
    public ContentBundleRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets content bundles for a specific age group.
    /// </summary>
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <returns>Content bundles matching the age group.</returns>
    public async Task<IEnumerable<ContentBundle>> GetByAgeGroupAsync(string ageGroupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ageGroupId);
        return await _dbSet.Where(b => b.AgeGroupId == ageGroupId).ToListAsync();
    }
}

