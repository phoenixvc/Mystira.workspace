using Microsoft.EntityFrameworkCore;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Data;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for UserBadge entity.
/// Uses strategy pattern to handle InMemory vs Cosmos DB differences.
/// </summary>
public class UserBadgeRepository : Repository<UserBadge>, IUserBadgeRepository
{
    private readonly bool _isInMemory;

    public UserBadgeRepository(DbContext context) : base(context)
    {
        _isInMemory = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
    }

    public async Task<IEnumerable<UserBadge>> GetByUserProfileIdAsync(string userProfileId, CancellationToken ct = default)
    {
        var badges = await QueryBadgesAsync(
            inMemoryQuery: () => _context.Set<UserBadge>()
                .AsNoTracking()
                .Where(b => b.UserProfileId == userProfileId),
            cosmosQuery: () => _context.Set<UserProfile>()
                .AsNoTracking()
                .Where(p => p.Id == userProfileId)
                .SelectMany(p => p.EarnedBadges),
            ct
        );
        return OrderByEarnedAtDescending(badges);
    }

    public async Task<UserBadge?> GetByUserProfileIdAndBadgeConfigIdAsync(string userProfileId, string badgeConfigurationId, CancellationToken ct = default)
    {
        if (_isInMemory)
        {
            return await _context.Set<UserBadge>()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserProfileId == userProfileId && b.BadgeConfigurationId == badgeConfigurationId, ct);
        }

        return await _context.Set<UserProfile>()
            .AsNoTracking()
            .Where(p => p.Id == userProfileId)
            .SelectMany(p => p.EarnedBadges)
            .FirstOrDefaultAsync(b => b.BadgeConfigurationId == badgeConfigurationId, ct);
    }

    public async Task<IEnumerable<UserBadge>> GetByGameSessionIdAsync(string gameSessionId, CancellationToken ct = default)
    {
        var badges = await QueryBadgesAsync(
            inMemoryQuery: () => _context.Set<UserBadge>()
                .AsNoTracking()
                .Where(b => b.GameSessionId == gameSessionId),
            cosmosQuery: () => _context.Set<UserProfile>()
                .AsNoTracking()
                .SelectMany(p => p.EarnedBadges)
                .Where(b => b.GameSessionId == gameSessionId),
            ct
        );
        return OrderByEarnedAtDescending(badges);
    }

    public async Task<IEnumerable<UserBadge>> GetByScenarioIdAsync(string scenarioId, CancellationToken ct = default)
    {
        var badges = await QueryBadgesAsync(
            inMemoryQuery: () => _context.Set<UserBadge>()
                .AsNoTracking()
                .Where(b => b.ScenarioId == scenarioId),
            cosmosQuery: () => _context.Set<UserProfile>()
                .AsNoTracking()
                .SelectMany(p => p.EarnedBadges)
                .Where(b => b.ScenarioId == scenarioId),
            ct
        );
        return OrderByEarnedAtDescending(badges);
    }

    public async Task<IEnumerable<UserBadge>> GetByUserProfileIdAndAxisAsync(string userProfileId, string axis, CancellationToken ct = default)
    {
        var badges = await QueryBadgesAsync(
            inMemoryQuery: () => _context.Set<UserBadge>()
                .AsNoTracking()
                .Where(b => b.UserProfileId == userProfileId && b.Axis == axis),
            cosmosQuery: () => _context.Set<UserProfile>()
                .AsNoTracking()
                .Where(p => p.Id == userProfileId)
                .SelectMany(p => p.EarnedBadges)
                .Where(b => b.Axis == axis),
            ct
        );
        return OrderByEarnedAtDescending(badges);
    }

    public override async Task<UserBadge> AddAsync(UserBadge entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_isInMemory)
        {
            await _context.Set<UserBadge>().AddAsync(entity, cancellationToken);
            return entity;
        }

        // For Cosmos DB: Add the owned entity to the owner's collection
        var profile = await _context.Set<UserProfile>()
            .FirstOrDefaultAsync(p => p.Id == entity.UserProfileId, cancellationToken);

        if (profile == null)
        {
            throw new InvalidOperationException($"UserProfile not found: {entity.UserProfileId}");
        }

        profile.EarnedBadges.Add(entity);
        return entity;
    }

    /// <summary>
    /// Executes the appropriate query based on database provider
    /// </summary>
    private async Task<List<UserBadge>> QueryBadgesAsync(
        Func<IQueryable<UserBadge>> inMemoryQuery,
        Func<IQueryable<UserBadge>> cosmosQuery,
        CancellationToken ct = default)
    {
        var query = _isInMemory ? inMemoryQuery() : cosmosQuery();
        return await query.ToListAsync(ct);
    }

    /// <summary>
    /// Orders badges by earned date descending
    /// </summary>
    private static List<UserBadge> OrderByEarnedAtDescending(List<UserBadge> badges)
    {
        return badges.OrderByDescending(b => b.EarnedAt).ToList();
    }
}
