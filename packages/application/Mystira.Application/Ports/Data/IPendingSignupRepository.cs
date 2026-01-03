using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for PendingSignup entity with domain-specific queries.
/// </summary>
public interface IPendingSignupRepository : IRepository<PendingSignup>
{
    /// <summary>
    /// Gets a pending signup by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pending signup if found; otherwise, null.</returns>
    Task<PendingSignup?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending signup by verification token.
    /// </summary>
    /// <param name="token">The verification token to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pending signup if found; otherwise, null.</returns>
    Task<PendingSignup?> GetByVerificationTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending signups for a given email address (including expired/cancelled).
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of pending signups for the email.</returns>
    Task<IReadOnlyList<PendingSignup>> GetAllByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a pending signup exists for the given email address.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a pending signup exists; otherwise, false.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all expired pending signups that need cleanup.
    /// </summary>
    /// <param name="cutoffDate">The date before which signups are considered expired.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of expired pending signups.</returns>
    Task<IReadOnlyList<PendingSignup>> GetExpiredSignupsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all expired or completed pending signups older than the specified date.
    /// </summary>
    /// <param name="olderThan">Delete signups older than this date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of signups deleted.</returns>
    Task<int> CleanupOldSignupsAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of verification emails sent for a given email in the last 24 hours.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of verification emails sent.</returns>
    Task<int> GetVerificationEmailCountLast24HoursAsync(string email, CancellationToken cancellationToken = default);
}
