using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for pending signup records.
/// </summary>
public class PendingSignupRepository : Repository<PendingSignup>, IPendingSignupRepository
{
    public PendingSignupRepository(DbContext context) : base(context)
    {
    }

    public async Task<PendingSignup?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _dbSet
            .Where(x => x.Email.ToLower() == normalizedEmail)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PendingSignup?> GetByVerificationTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.VerificationTokenHash == tokenHash, ct);
    }

    async Task IPendingSignupRepository.AddAsync(PendingSignup pendingSignup, CancellationToken ct)
    {
        await base.AddAsync(pendingSignup, ct);
    }

    async Task IPendingSignupRepository.UpdateAsync(PendingSignup pendingSignup, CancellationToken ct)
    {
        await base.UpdateAsync(pendingSignup, ct);
    }
}
