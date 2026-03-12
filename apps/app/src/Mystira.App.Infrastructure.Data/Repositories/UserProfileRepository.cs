using Microsoft.EntityFrameworkCore;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for UserProfile entity with domain-specific queries
/// </summary>
public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserProfile>> GetByAccountIdAsync(string accountId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.EarnedBadges)
            .Where(p => p.AccountId == accountId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<UserProfile>> GetGuestProfilesAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.EarnedBadges)
            .Where(p => p.IsGuest)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<UserProfile>> GetNonGuestProfilesAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.EarnedBadges)
            .Where(p => !p.IsGuest)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}
