using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for Account entity with domain-specific queries
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// Gets an account by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets an account by external user identifier.
    /// </summary>
    /// <param name="externalUserId">The external user identifier to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    Task<Account?> GetByExternalUserIdAsync(string externalUserId, CancellationToken ct = default);

    /// <summary>
    /// Checks if an account exists with the specified email address.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if an account exists with the email; otherwise, false.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}

