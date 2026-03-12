using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Cosmos DB repository for DataDeletionRequest entities (COPPA compliance).
/// Partitioned by ChildProfileId for efficient per-child queries.
/// </summary>
public class DataDeletionRepository : Repository<DataDeletionRequest>, IDataDeletionRepository
{
    public DataDeletionRepository(DbContext context) : base(context)
    {
    }

    public async Task<DataDeletionRequest?> GetByChildProfileIdAsync(string childProfileId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.ChildProfileId == childProfileId, ct);
    }

    public async Task<List<DataDeletionRequest>> GetPendingDeletionsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(d =>
                // Pending deletions past their scheduled date
                (d.Status == DeletionStatus.Pending && d.ScheduledDeletionAt <= now)
                // Failed deletions eligible for retry (below max retries and past retry backoff)
                || (d.Status == DeletionStatus.Failed
                    && d.RetryCount < DataDeletionRequest.MaxRetries
                    && d.NextRetryAt != null
                    && d.NextRetryAt <= now))
            .ToListAsync(ct);
    }

    // Explicit interface implementations - no SaveChangesAsync; callers use IUnitOfWork for commit
    async Task IDataDeletionRepository.AddAsync(DataDeletionRequest request, CancellationToken ct)
    {
        await base.AddAsync(request, ct);
    }

    async Task IDataDeletionRepository.UpdateAsync(DataDeletionRequest request, CancellationToken ct)
    {
        await base.UpdateAsync(request, ct);
    }
}
