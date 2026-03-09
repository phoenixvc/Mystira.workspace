using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports;

/// <summary>
/// Port interface for orchestrating child data deletion across all data stores.
/// Implements COPPA data deletion requirements.
/// </summary>
public interface IDataDeletionService
{
    /// <summary>
    /// Execute the full deletion workflow for a child profile.
    /// Deletes/anonymizes data across Cosmos DB, Blob Storage, and logs.
    /// </summary>
    Task<DeletionResult> ExecuteDeletionAsync(DataDeletionRequest request, CancellationToken ct = default);
}

/// <summary>
/// Result of a data deletion operation.
/// </summary>
public record DeletionResult(
    bool Success,
    List<string> CompletedScopes,
    List<string> FailedScopes,
    string? ErrorMessage = null);
