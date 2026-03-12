using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for pending email signup records.
/// </summary>
public interface IPendingSignupRepository
{
    /// <summary>
    /// Gets a pending signup by email address.
    /// </summary>
    Task<PendingSignup?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets a pending signup by its verification token hash.
    /// </summary>
    Task<PendingSignup?> GetByVerificationTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Adds a new pending signup.
    /// </summary>
    Task AddAsync(PendingSignup pendingSignup, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing pending signup.
    /// </summary>
    Task UpdateAsync(PendingSignup pendingSignup, CancellationToken ct = default);
}
