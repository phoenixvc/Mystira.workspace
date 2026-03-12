using Mystira.Domain.Models;

namespace Mystira.Application.Ports;

/// <summary>
/// Port interface for orchestrating child data deletion across all data stores.
/// Implements COPPA data deletion requirements by coordinating deletion
/// of user data from Cosmos DB, Blob Storage, and logs.
/// </summary>
public interface IDataDeletionService
{
    /// <summary>
    /// Execute the full deletion workflow for a child profile.
    /// Deletes/anonymizes data across Cosmos DB, Blob Storage, and logs.
    /// </summary>
    /// <param name="request">The data deletion request containing profile and scope information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating which scopes succeeded and which failed.</returns>
    Task<DeletionResult> ExecuteDeletionAsync(DataDeletionRequest request, CancellationToken ct = default);
}

/// <summary>
/// Result of a data deletion operation, tracking which scopes completed
/// and which failed for retry purposes.
/// </summary>
/// <param name="Success">Whether all deletion scopes completed successfully.</param>
/// <param name="CompletedScopes">List of scopes that were successfully deleted.</param>
/// <param name="FailedScopes">List of scopes that failed deletion.</param>
/// <param name="ErrorMessage">Optional error message if any scopes failed.</param>
public record DeletionResult(
    bool Success,
    List<string> CompletedScopes,
    List<string> FailedScopes,
    string? ErrorMessage = null);
