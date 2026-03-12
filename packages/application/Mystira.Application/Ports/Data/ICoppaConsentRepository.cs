using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Port interface for COPPA parental consent persistence.
/// Provides access to <see cref="ParentalConsent"/> entities for managing
/// the parental consent lifecycle required by COPPA compliance.
/// </summary>
public interface ICoppaConsentRepository
{
    /// <summary>
    /// Gets the parental consent record for a specific child profile.
    /// </summary>
    /// <param name="childProfileId">The child profile identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parental consent record if found; otherwise, null.</returns>
    Task<ParentalConsent?> GetByChildProfileIdAsync(string childProfileId, CancellationToken ct = default);

    /// <summary>
    /// Gets a parental consent record by its unique identifier.
    /// </summary>
    /// <param name="id">The consent record identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parental consent record if found; otherwise, null.</returns>
    Task<ParentalConsent?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Gets a parental consent record by its verification token.
    /// Used during the email verification flow.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parental consent record if found; otherwise, null.</returns>
    Task<ParentalConsent?> GetByVerificationTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Adds a new parental consent record.
    /// </summary>
    /// <param name="consent">The parental consent record to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(ParentalConsent consent, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing parental consent record.
    /// </summary>
    /// <param name="consent">The parental consent record to update.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateAsync(ParentalConsent consent, CancellationToken ct = default);
}
