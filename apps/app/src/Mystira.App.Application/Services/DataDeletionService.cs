using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.Core.Ports;
using Mystira.Core.Ports.Storage;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;


namespace Mystira.App.Application.Services;

/// <summary>
/// Orchestrates child data deletion across all data stores for COPPA compliance.
/// Handles partial failures gracefully - completed scopes are tracked even if others fail.
/// </summary>
public class DataDeletionService : IDataDeletionService
{
    private readonly IUserProfileRepository _userProfileRepo;
    private readonly IGameSessionRepository _gameSessionRepo;
    private readonly IBlobService _blobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataDeletionService> _logger;

    public DataDeletionService(
        IUserProfileRepository userProfileRepo,
        IGameSessionRepository gameSessionRepo,
        IBlobService blobService,
        IUnitOfWork unitOfWork,
        ILogger<DataDeletionService> logger)
    {
        _userProfileRepo = userProfileRepo;
        _gameSessionRepo = gameSessionRepo;
        _blobService = blobService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeletionResult> ExecuteDeletionAsync(DataDeletionRequest request, CancellationToken ct = default)
    {
        var completedScopes = new List<string>();
        var failedScopes = new List<string>();
        var childProfileId = request.ChildProfileId;

        var hashedId = LogAnonymizer.HashId(childProfileId);
        _logger.LogInformation("Starting data deletion for child profile {ChildProfileIdHash}", hashedId);

        // 1. Delete user profile data from Cosmos DB
        if (request.DeletionScope.Contains("CosmosDB"))
        {
            try
            {
                await DeleteCosmosDataAsync(childProfileId, ct);
                completedScopes.Add("CosmosDB");
                request.AddAuditEntry("CosmosDB_Deleted", "System");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Cosmos DB data for {ChildProfileIdHash}", hashedId);
                failedScopes.Add("CosmosDB");
                request.AddAuditEntry("CosmosDB_Failed", $"System: {LogAnonymizer.SanitizeExceptionMessage(ex.Message)}");
            }
        }

        // 2. Delete blob storage data (avatars, uploads)
        if (request.DeletionScope.Contains("BlobStorage"))
        {
            try
            {
                await DeleteBlobDataAsync(childProfileId, ct);
                completedScopes.Add("BlobStorage");
                request.AddAuditEntry("BlobStorage_Deleted", "System");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Blob Storage data for {ChildProfileIdHash}", hashedId);
                failedScopes.Add("BlobStorage");
                request.AddAuditEntry("BlobStorage_Failed", $"System: {LogAnonymizer.SanitizeExceptionMessage(ex.Message)}");
            }
        }

        // 3. Anonymize logs (mark scope complete - actual log purging is infrastructure-level)
        if (request.DeletionScope.Contains("Logs"))
        {
            completedScopes.Add("Logs");
            request.AddAuditEntry("Logs_MarkedForPurge", "System");
        }

        var success = failedScopes.Count == 0;

        _logger.LogInformation(
            "Data deletion for {ChildProfileIdHash}: completed={Completed}, failed={Failed}",
            hashedId, string.Join(",", completedScopes), string.Join(",", failedScopes));

        return new DeletionResult(
            Success: success,
            CompletedScopes: completedScopes,
            FailedScopes: failedScopes,
            ErrorMessage: success ? null : $"Failed scopes: {string.Join(", ", failedScopes)}");
    }

    private async Task DeleteCosmosDataAsync(string childProfileId, CancellationToken ct)
    {
        // Delete user profile
        var profile = await _userProfileRepo.GetByIdAsync(childProfileId, ct);
        if (profile != null)
        {
            await _userProfileRepo.DeleteAsync(childProfileId, ct);
        }

        // Anonymize game sessions (keep aggregate stats, remove PII)
        var sessions = await _gameSessionRepo.GetByProfileIdAsync(childProfileId, ct);

        foreach (var session in sessions)
        {
            // Anonymize player names
            session.PlayerNames?.Clear();

            // Anonymize character assignment PII
            if (session.CharacterAssignments != null)
            {
                foreach (var assignment in session.CharacterAssignments)
                {
                    if (assignment.PlayerAssignment == null) continue;

                    var pa = assignment.PlayerAssignment;
                    // Clear profile link
                    pa.ProfileId = null;
                    // Clear registered player PII
                    pa.ProfileName = null;
                    pa.ProfileImage = null;
                    pa.SelectedAvatarMediaId = null;
                    // Clear guest player PII
                    pa.GuestName = null;
                    pa.GuestAgeRange = null;
                    pa.GuestAvatar = null;
                }
            }

            await _gameSessionRepo.UpdateAsync(session, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task DeleteBlobDataAsync(string childProfileId, CancellationToken ct)
    {
        // Delete avatar and any uploaded content for this profile
        var prefix = $"profiles/{childProfileId}/";
        var blobs = await _blobService.ListMediaAsync(prefix, ct);
        foreach (var blob in blobs)
        {
            await _blobService.DeleteMediaAsync(blob, ct);
        }
    }
}
