using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Account entity with domain-specific queries
/// </summary>
public class AccountRepository : Repository<Account>, IAccountRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AccountRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
    }

    /// <inheritdoc/>
    public async Task<Account?> GetByExternalUserIdAsync(string externalUserId)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.ExternalUserId == externalUserId);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(a => a.Email.ToLower() == email.ToLower());
    }
}

