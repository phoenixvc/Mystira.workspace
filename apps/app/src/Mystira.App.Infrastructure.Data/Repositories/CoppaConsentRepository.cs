using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Cosmos DB repository for ParentalConsent entities (COPPA compliance).
/// Partitioned by ChildProfileId for efficient per-child queries.
/// </summary>
public class CoppaConsentRepository : Repository<ParentalConsent>, ICoppaConsentRepository
{
    public CoppaConsentRepository(DbContext context) : base(context)
    {
    }

    public async Task<ParentalConsent?> GetByChildProfileIdAsync(string childProfileId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.ChildProfileId == childProfileId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ParentalConsent?> GetByVerificationTokenAsync(string token, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.VerificationToken == token, ct);
    }

    // GetByIdAsync is inherited from Repository<ParentalConsent>

    // Explicit interface implementations - no SaveChangesAsync; callers use IUnitOfWork for commit
    async Task ICoppaConsentRepository.AddAsync(ParentalConsent consent, CancellationToken ct)
    {
        await base.AddAsync(consent, ct);
    }

    async Task ICoppaConsentRepository.UpdateAsync(ParentalConsent consent, CancellationToken ct)
    {
        await base.UpdateAsync(consent, ct);
    }
}
