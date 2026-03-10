using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for UserBadge entity with domain-specific queries
/// </summary>
public interface IUserBadgeRepository : IRepository<UserBadge, string>
{
    Task<IEnumerable<UserBadge>> GetByUserProfileIdAsync(string userProfileId, CancellationToken ct = default);
    Task<UserBadge?> GetByUserProfileIdAndBadgeConfigIdAsync(string userProfileId, string badgeConfigurationId, CancellationToken ct = default);
    Task<IEnumerable<UserBadge>> GetByGameSessionIdAsync(string gameSessionId, CancellationToken ct = default);
    Task<IEnumerable<UserBadge>> GetByScenarioIdAsync(string scenarioId, CancellationToken ct = default);
    Task<IEnumerable<UserBadge>> GetByUserProfileIdAndAxisAsync(string userProfileId, string axis, CancellationToken ct = default);
}

