using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Ports.Storage;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.Services;

public class DataDeletionServiceTests
{
    private readonly Mock<IUserProfileRepository> _userProfileRepo;
    private readonly Mock<IGameSessionRepository> _gameSessionRepo;
    private readonly Mock<IBlobService> _blobService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DataDeletionService>> _logger;
    private readonly DataDeletionService _sut;

    public DataDeletionServiceTests()
    {
        _userProfileRepo = new Mock<IUserProfileRepository>();
        _gameSessionRepo = new Mock<IGameSessionRepository>();
        _blobService = new Mock<IBlobService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DataDeletionService>>();

        _sut = new DataDeletionService(
            _userProfileRepo.Object,
            _gameSessionRepo.Object,
            _blobService.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    #region All Scopes Succeed

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopes_ReturnsSuccessTrue()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopes_ReturnsAllThreeCompletedScopes()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.CompletedScopes.Should().HaveCount(3);
        result.CompletedScopes.Should().Contain("CosmosDB");
        result.CompletedScopes.Should().Contain("BlobStorage");
        result.CompletedScopes.Should().Contain("Logs");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopes_ReturnsNoFailedScopes()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.FailedScopes.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopes_ReturnsNoErrorMessage()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.ErrorMessage.Should().BeNull();
    }

    #endregion

    #region CosmosDB Deletion - User Profile

    [Fact]
    public async Task ExecuteDeletionAsync_DeletesUserProfile_WhenProfileExists()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _userProfileRepo.Verify(
            r => r.DeleteAsync("child-123", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteDeletionAsync_CallsUnitOfWorkSaveChanges_WhenProfileExists()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CosmosDB Deletion - Game Session Anonymization

    [Fact]
    public async Task ExecuteDeletionAsync_AnonymizesGameSessions_ClearsPlayerNames()
    {
        // Arrange
        var request = CreateDeletionRequest();
        var session = new GameSession
        {
            Id = "session-1",
            ProfileId = "child-123",
            PlayerNames = new List<string> { "ChildPlayer", "FriendPlayer" }
        };

        SetupProfileExists("child-123");
        _gameSessionRepo
            .Setup(r => r.GetByProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { session });
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        session.PlayerNames.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteDeletionAsync_CallsUpdateAsync_ForEachGameSession()
    {
        // Arrange
        var request = CreateDeletionRequest();
        var sessions = new List<GameSession>
        {
            new() { Id = "session-1", ProfileId = "child-123", PlayerNames = new List<string> { "Player1" } },
            new() { Id = "session-2", ProfileId = "child-123", PlayerNames = new List<string> { "Player2" } }
        };

        SetupProfileExists("child-123");
        _gameSessionRepo
            .Setup(r => r.GetByProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _gameSessionRepo.Verify(
            r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteDeletionAsync_CallsUnitOfWorkSaveChanges_AfterGameSessionAnonymization()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region BlobStorage Deletion

    [Fact]
    public async Task ExecuteDeletionAsync_ListsBlobsWithCorrectPrefix()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _blobService.Verify(
            b => b.ListMediaAsync("profiles/child-123/", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteDeletionAsync_DeletesEachBlob()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");

        var blobs = new List<string> { "profiles/child-123/avatar.png", "profiles/child-123/upload1.jpg" };
        _blobService
            .Setup(b => b.ListMediaAsync("profiles/child-123/", It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobs);
        _blobService
            .Setup(b => b.DeleteMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _blobService.Verify(
            b => b.DeleteMediaAsync("profiles/child-123/avatar.png", It.IsAny<CancellationToken>()),
            Times.Once);
        _blobService.Verify(
            b => b.DeleteMediaAsync("profiles/child-123/upload1.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Partial Failure - BlobStorage Fails

    [Fact]
    public async Task ExecuteDeletionAsync_BlobDeleteThrows_ReturnsSuccessFalse()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");

        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Blob storage unavailable"));

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteDeletionAsync_BlobDeleteThrows_FailedScopesContainsBlobStorage()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");

        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Blob storage unavailable"));

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.FailedScopes.Should().Contain("BlobStorage");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_BlobDeleteThrows_CosmosDBStillCompletes()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");

        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Blob storage unavailable"));

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.CompletedScopes.Should().Contain("CosmosDB");
        result.CompletedScopes.Should().Contain("Logs");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_BlobDeleteThrows_ErrorMessageContainsFailedScopes()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");

        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Blob storage unavailable"));

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("BlobStorage");
    }

    #endregion

    #region Profile Does Not Exist

    [Fact]
    public async Task ExecuteDeletionAsync_ProfileDoesNotExist_StillSucceeds()
    {
        // Arrange
        var request = CreateDeletionRequest();

        _userProfileRepo
            .Setup(r => r.GetByIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);
        _gameSessionRepo
            .Setup(r => r.GetByProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.CompletedScopes.Should().Contain("CosmosDB");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_ProfileDoesNotExist_DoesNotCallDeleteAsync()
    {
        // Arrange
        var request = CreateDeletionRequest();

        _userProfileRepo
            .Setup(r => r.GetByIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);
        _gameSessionRepo
            .Setup(r => r.GetByProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _userProfileRepo.Verify(
            r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteDeletionAsync_ProfileDoesNotExist_StillSavesChangesViaUnitOfWork()
    {
        // Arrange
        var request = CreateDeletionRequest();

        _userProfileRepo
            .Setup(r => r.GetByIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);
        _gameSessionRepo
            .Setup(r => r.GetByProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Audit Entries

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopesSucceed_AddsCosmosDbDeletedAuditEntry()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        request.AuditTrail.Should().Contain(a => a.Action == "CosmosDB_Deleted");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopesSucceed_AddsBlobStorageDeletedAuditEntry()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        request.AuditTrail.Should().Contain(a => a.Action == "BlobStorage_Deleted");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopesSucceed_AddsLogsMarkedForPurgeAuditEntry()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        request.AuditTrail.Should().Contain(a => a.Action == "Logs_MarkedForPurge");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_AllScopesSucceed_AuditEntriesPerformedBySystem()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        SetupSuccessfulBlobDelete("child-123");

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        request.AuditTrail.Should().OnlyContain(a => a.PerformedByHash.StartsWith("System"));
    }

    [Fact]
    public async Task ExecuteDeletionAsync_BlobFails_AddsFailedAuditEntry()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");

        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Storage down"));

        // Act
        await _sut.ExecuteDeletionAsync(request);

        // Assert
        request.AuditTrail.Should().Contain(a => a.Action == "BlobStorage_Failed");
    }

    [Fact]
    public async Task ExecuteDeletionAsync_CosmosDbFails_AddsFailedAuditEntry()
    {
        // Arrange
        var request = CreateDeletionRequest();

        _userProfileRepo
            .Setup(r => r.GetByIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        request.AuditTrail.Should().Contain(a => a.Action == "CosmosDB_Failed");
        result.FailedScopes.Should().Contain("CosmosDB");
    }

    #endregion

    #region Scope Filtering

    [Fact]
    public async Task ExecuteDeletionAsync_OnlyCosmosDbScope_DoesNotDeleteBlobs()
    {
        // Arrange
        var request = new DataDeletionRequest
        {
            ChildProfileId = "child-123",
            DeletionScope = new List<string> { "CosmosDB" }
        };
        SetupSuccessfulCosmosDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.CompletedScopes.Should().Contain("CosmosDB");
        result.CompletedScopes.Should().NotContain("BlobStorage");
        _blobService.Verify(
            b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteDeletionAsync_OnlyBlobStorageScope_DoesNotDeleteProfile()
    {
        // Arrange
        var request = new DataDeletionRequest
        {
            ChildProfileId = "child-123",
            DeletionScope = new List<string> { "BlobStorage" }
        };
        SetupSuccessfulBlobDelete("child-123");

        // Act
        var result = await _sut.ExecuteDeletionAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.CompletedScopes.Should().Contain("BlobStorage");
        result.CompletedScopes.Should().NotContain("CosmosDB");
        _userProfileRepo.Verify(
            r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region OperationCanceledException Propagation

    [Fact]
    public async Task ExecuteDeletionAsync_CosmosDbThrowsOperationCanceled_Propagates()
    {
        // Arrange
        var request = CreateDeletionRequest();
        _userProfileRepo
            .Setup(r => r.GetByIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.ExecuteDeletionAsync(request));
    }

    [Fact]
    public async Task ExecuteDeletionAsync_BlobStorageThrowsOperationCanceled_Propagates()
    {
        // Arrange
        var request = CreateDeletionRequest();
        SetupSuccessfulCosmosDelete("child-123");
        _blobService
            .Setup(b => b.ListMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.ExecuteDeletionAsync(request));
    }

    #endregion

    #region Helpers

    private static DataDeletionRequest CreateDeletionRequest()
    {
        return new DataDeletionRequest
        {
            ChildProfileId = "child-123",
            DeletionScope = new List<string> { "CosmosDB", "BlobStorage", "Logs" },
            RequestedBy = DeletionRequestSource.Parent
        };
    }

    private void SetupProfileExists(string profileId)
    {
        _userProfileRepo
            .Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = profileId, Name = "Test Child" });
    }

    private void SetupSuccessfulCosmosDelete(string profileId)
    {
        SetupProfileExists(profileId);
        _gameSessionRepo
            .Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
    }

    private void SetupSuccessfulBlobDelete(string profileId)
    {
        _blobService
            .Setup(b => b.ListMediaAsync($"profiles/{profileId}/", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
    }

    #endregion
}
