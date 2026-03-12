using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for UserProfile entity with domain-specific queries
/// </summary>
public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserProfileRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserProfile>> GetByAccountIdAsync(string accountId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.EarnedBadges)
            .Where(p => p.AccountId == accountId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserProfile>> GetGuestProfilesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.EarnedBadges)
            .Where(p => p.IsGuest)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserProfile>> GetNonGuestProfilesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.EarnedBadges)
            .Where(p => !p.IsGuest)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}
