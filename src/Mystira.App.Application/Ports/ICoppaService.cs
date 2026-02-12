using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports;

/// <summary>
/// Port interface for COPPA compliance operations.
/// </summary>
public interface ICoppaConsentRepository
{
    Task<ParentalConsent?> GetByChildProfileIdAsync(string childProfileId, CancellationToken ct = default);
    Task<ParentalConsent?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<ParentalConsent?> GetByVerificationTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(ParentalConsent consent, CancellationToken ct = default);
    Task UpdateAsync(ParentalConsent consent, CancellationToken ct = default);
}

/// <summary>
/// Port interface for data deletion request persistence.
/// </summary>
public interface IDataDeletionRepository
{
    Task<DataDeletionRequest?> GetByChildProfileIdAsync(string childProfileId, CancellationToken ct = default);
    Task AddAsync(DataDeletionRequest request, CancellationToken ct = default);
    Task UpdateAsync(DataDeletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns deletions ready to process: Pending past their scheduled date,
    /// or Failed with retry available (NextRetryAt in the past and RetryCount below max).
    /// </summary>
    Task<List<DataDeletionRequest>> GetPendingDeletionsAsync(CancellationToken ct = default);
}
