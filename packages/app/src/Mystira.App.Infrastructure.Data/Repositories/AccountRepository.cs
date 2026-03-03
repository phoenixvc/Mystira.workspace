using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Account entity with domain-specific queries
/// </summary>
public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(DbContext context) : base(context)
    {
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower(), ct);
    }

    public async Task<Account?> GetByExternalUserIdAsync(string externalUserId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.ExternalUserId == externalUserId, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(a => a.Email.ToLower() == email.ToLower(), ct);
    }
}
