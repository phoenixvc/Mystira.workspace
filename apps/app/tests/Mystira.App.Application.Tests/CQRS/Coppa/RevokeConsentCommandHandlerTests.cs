using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Coppa.Commands;
using Mystira.App.Application.Ports;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Coppa;

public class RevokeConsentCommandHandlerTests
{
    private readonly Mock<ICoppaConsentRepository> _consentRepository;
    private readonly Mock<IDataDeletionRepository> _deletionRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<RevokeConsentCommand>> _logger;

    public RevokeConsentCommandHandlerTests()
    {
        _consentRepository = new Mock<ICoppaConsentRepository>();
        _deletionRepository = new Mock<IDataDeletionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<RevokeConsentCommand>>();
    }

    #region Consent Not Found

    [Fact]
    public async Task Handle_NoConsentFound_ReturnsNotFound()
    {
        // Arrange
        var command = new RevokeConsentCommand("child-unknown", "parent-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);

        // Act
        var result = await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("NotFound");
        result.ConsentId.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoConsentFound_DoesNotCallUpdateOrDeletion()
    {
        // Arrange
        var command = new RevokeConsentCommand("child-unknown", "parent-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _deletionRepository.Verify(
            r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Unauthorized (Email Hash Mismatch)

    [Fact]
    public async Task Handle_ParentEmailHashMismatch_ReturnsUnauthorized()
    {
        // Arrange
        var consent = CreateActiveConsent("correct-hash");
        var command = new RevokeConsentCommand("child-1", "wrong-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Unauthorized");
        result.ConsentId.Should().BeEmpty();
        result.Message.Should().Contain("does not match");
    }

    [Fact]
    public async Task Handle_Unauthorized_DoesNotRevokeOrCreateDeletion()
    {
        // Arrange
        var consent = CreateActiveConsent("correct-hash");
        var command = new RevokeConsentCommand("child-1", "wrong-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        consent.Status.Should().NotBe(ConsentStatus.Revoked);
        _consentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _deletionRepository.Verify(
            r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Successful Revocation

    [Fact]
    public async Task Handle_ValidRequest_RevokesConsentAndReturnsRevoked()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Revoked");
        result.ConsentId.Should().Be(consent.Id);
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsConsentStatusToRevoked()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        consent.Status.Should().Be(ConsentStatus.Revoked);
        consent.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsUpdateAsyncOnConsentRepository()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepository.Verify(
            r => r.UpdateAsync(consent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Data Deletion Request

    [Fact]
    public async Task Handle_ValidRequest_CreatesDataDeletionRequestWithPendingStatus()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");
        DataDeletionRequest? capturedRequest = null;

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        _deletionRepository
            .Setup(r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DataDeletionRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Status.Should().Be(DeletionStatus.Pending);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesDataDeletionRequestWithParentSource()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");
        DataDeletionRequest? capturedRequest = null;

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        _deletionRepository
            .Setup(r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DataDeletionRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestedBy.Should().Be(DeletionRequestSource.Parent);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesDataDeletionRequestWithCorrectChildProfileId()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");
        DataDeletionRequest? capturedRequest = null;

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        _deletionRepository
            .Setup(r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DataDeletionRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ChildProfileId.Should().Be("child-1");
    }

    [Fact]
    public async Task Handle_ValidRequest_SchedulesDeletionWithSevenDaySla()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");
        DataDeletionRequest? capturedRequest = null;
        var beforeExecution = DateTime.UtcNow;

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        _deletionRepository
            .Setup(r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DataDeletionRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ScheduledDeletionAt.Should().BeCloseTo(
            beforeExecution.AddDays(7), TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Audit Trail

    [Fact]
    public async Task Handle_ValidRequest_AddsConsentRevokedAuditEntry()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");
        DataDeletionRequest? capturedRequest = null;

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        _deletionRepository
            .Setup(r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<DataDeletionRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.AuditTrail.Should().ContainSingle();
        capturedRequest.AuditTrail[0].Action.Should().Be("ConsentRevoked");
        capturedRequest.AuditTrail[0].PerformedByHash.Should().Be("parent-hash");
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsAddAsyncOnDeletionRepository()
    {
        // Arrange
        var consent = CreateActiveConsent("parent-hash");
        var command = new RevokeConsentCommand("child-1", "parent-hash");

        _consentRepository
            .Setup(r => r.GetByChildProfileIdAsync("child-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await RevokeConsentCommandHandler.Handle(
            command, _consentRepository.Object, _deletionRepository.Object,
            _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _deletionRepository.Verify(
            r => r.AddAsync(It.IsAny<DataDeletionRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Creates an active consent record with the specified parent email hash.
    /// </summary>
    private static ParentalConsent CreateActiveConsent(string parentEmailHash)
    {
        return new ParentalConsent
        {
            Id = "consent-abc",
            ChildProfileId = "child-1",
            ParentEmailHash = parentEmailHash,
            Status = ConsentStatus.Verified,
            RevokedAt = null,
            VerificationMethod = ConsentVerificationMethod.Email,
            ConsentedAt = DateTime.UtcNow.AddDays(-30),
            VerifiedAt = DateTime.UtcNow.AddDays(-30)
        };
    }

    #endregion
}
