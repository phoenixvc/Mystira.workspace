using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ContentBundle entity
/// </summary>
public class ContentBundleRepository : Repository<ContentBundle>, IContentBundleRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBundleRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ContentBundleRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ContentBundle>> GetByAgeGroupAsync(string ageGroupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ageGroupId);
        return await _dbSet.Where(b => b.AgeGroupId == ageGroupId).ToListAsync();
    }
}

