using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for UserProfile entity with domain-specific queries
/// </summary>
public interface IUserProfileRepository : IRepository<UserProfile>
{
    /// <summary>
    /// Gets all user profiles associated with a specific account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A collection of user profiles for the specified account.</returns>
    Task<IEnumerable<UserProfile>> GetByAccountIdAsync(string accountId);

    /// <summary>
    /// Gets all guest user profiles.
    /// </summary>
    /// <returns>A collection of guest user profiles.</returns>
    Task<IEnumerable<UserProfile>> GetGuestProfilesAsync();

    /// <summary>
    /// Gets all non-guest user profiles.
    /// </summary>
    /// <returns>A collection of non-guest user profiles.</returns>
    Task<IEnumerable<UserProfile>> GetNonGuestProfilesAsync();
}

