using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for UserProfile entity with domain-specific queries
/// </summary>
public interface IUserProfileRepository : IRepository<UserProfile>
{
    /// <summary>
    /// Gets all user profiles associated with a specific account.
    /// </summary>
    Task<IEnumerable<UserProfile>> GetByAccountIdAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets all guest user profiles.
    /// </summary>
    Task<IEnumerable<UserProfile>> GetGuestProfilesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all non-guest user profiles.
    /// </summary>
    Task<IEnumerable<UserProfile>> GetNonGuestProfilesAsync(CancellationToken ct = default);
}
