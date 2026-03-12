using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Port interface for data deletion request persistence.
/// Manages <see cref="DataDeletionRequest"/> entities for COPPA-compliant
/// child data deletion workflows with retry support.
/// </summary>
public interface IDataDeletionRepository
{
    /// <summary>
    /// Gets a data deletion request by the child profile identifier.
    /// </summary>
    /// <param name="childProfileId">The child profile identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The data deletion request if found; otherwise, null.</returns>
    Task<DataDeletionRequest?> GetByChildProfileIdAsync(string childProfileId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new data deletion request.
    /// </summary>
    /// <param name="request">The data deletion request to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(DataDeletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing data deletion request.
    /// </summary>
    /// <param name="request">The data deletion request to update.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateAsync(DataDeletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns deletions ready to process: Pending past their scheduled date,
    /// or Failed with retry available (NextRetryAt in the past and RetryCount below max).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of pending data deletion requests ready for processing.</returns>
    Task<List<DataDeletionRequest>> GetPendingDeletionsAsync(CancellationToken ct = default);
}
