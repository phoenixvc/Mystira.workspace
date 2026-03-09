using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Coppa.Commands;
using Mystira.App.Application.Ports;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Coppa;

public class VerifyParentalConsentCommandHandlerTests
{
    private readonly Mock<ICoppaConsentRepository> _consentRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<VerifyParentalConsentCommand>> _logger;

    public VerifyParentalConsentCommandHandlerTests()
    {
        _consentRepository = new Mock<ICoppaConsentRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<VerifyParentalConsentCommand>>();
    }

    #region Empty Token Validation

    [Fact]
    public async Task Handle_EmptyVerificationToken_ThrowsArgumentException()
    {
        // Arrange
        var command = new VerifyParentalConsentCommand("", "Email");

        // Act
        var act = () => VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("VerificationToken");
    }

    [Fact]
    public async Task Handle_WhitespaceVerificationToken_ThrowsArgumentException()
    {
        // Arrange
        var command = new VerifyParentalConsentCommand("   ", "Email");

        // Act
        var act = () => VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("VerificationToken");
    }

    [Fact]
    public async Task Handle_NullVerificationToken_ThrowsArgumentException()
    {
        // Arrange
        var command = new VerifyParentalConsentCommand(null!, "Email");

        // Act
        var act = () => VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Token Not Found

    [Fact]
    public async Task Handle_TokenNotFoundInRepository_ReturnsNotFound()
    {
        // Arrange
        var command = new VerifyParentalConsentCommand("nonexistent-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("nonexistent-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("NotFound");
        result.ConsentId.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_TokenNotFound_DoesNotCallUpdateAsync()
    {
        // Arrange
        var command = new VerifyParentalConsentCommand("nonexistent-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("nonexistent-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);

        // Act
        await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Expired Token

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsExpired()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            Id = "consent-123",
            ChildProfileId = "child-1",
            Status = ConsentStatus.Pending,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(-1) // expired
        };

        var command = new VerifyParentalConsentCommand("expired-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Expired");
        result.ConsentId.Should().Be("consent-123");
        result.Message.Should().Contain("expired");
    }

    [Fact]
    public async Task Handle_ExpiredToken_UpdatesConsentStatusToExpired()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            Id = "consent-123",
            ChildProfileId = "child-1",
            Status = ConsentStatus.Pending,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        var command = new VerifyParentalConsentCommand("expired-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        consent.Status.Should().Be(ConsentStatus.Expired);
        _consentRepository.Verify(
            r => r.UpdateAsync(consent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Already Verified

    [Fact]
    public async Task Handle_ConsentAlreadyActive_ReturnsAlreadyVerified()
    {
        // Arrange — IsActive requires Status == Verified && RevokedAt == null
        var consent = new ParentalConsent
        {
            Id = "consent-456",
            ChildProfileId = "child-2",
            Status = ConsentStatus.Verified,
            RevokedAt = null,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(1) // not expired
        };

        var command = new VerifyParentalConsentCommand("valid-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("AlreadyVerified");
        result.ConsentId.Should().Be("consent-456");
    }

    [Fact]
    public async Task Handle_AlreadyVerified_DoesNotCallUpdateAsync()
    {
        // Arrange
        var consent = new ParentalConsent
        {
            Id = "consent-456",
            ChildProfileId = "child-2",
            Status = ConsentStatus.Verified,
            RevokedAt = null,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var command = new VerifyParentalConsentCommand("valid-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Invalid Verification Method

    [Fact]
    public async Task Handle_InvalidVerificationMethod_ReturnsInvalidMethod()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "FakeMethod");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("InvalidMethod");
        result.Message.Should().Contain("FakeMethod");
    }

    [Fact]
    public async Task Handle_NoneVerificationMethod_ReturnsInvalidMethod()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "None");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("InvalidMethod");
    }

    [Fact]
    public async Task Handle_EmptyStringVerificationMethod_ReturnsInvalidMethod()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("InvalidMethod");
    }

    [Fact]
    public async Task Handle_InvalidMethod_DoesNotCallApproveOrUpdate()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "FakeMethod");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        consent.Status.Should().NotBe(ConsentStatus.Verified);
        _consentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Successful Verification

    [Fact]
    public async Task Handle_ValidEmailMethod_ReturnsVerified()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Verified");
        result.ConsentId.Should().Be(consent.Id);
        result.Message.Should().Contain("verified");
    }

    [Fact]
    public async Task Handle_ValidMethod_CallsApproveOnConsent()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "Email");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        consent.Status.Should().Be(ConsentStatus.Verified);
        consent.VerificationMethod.Should().Be(ConsentVerificationMethod.Email);
        consent.ConsentedAt.Should().NotBeNull();
        consent.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidMethod_CallsUpdateAsync()
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", "CreditCard");

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepository.Verify(
            r => r.UpdateAsync(consent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("CreditCard", ConsentVerificationMethod.CreditCard)]
    [InlineData("GovernmentId", ConsentVerificationMethod.GovernmentId)]
    [InlineData("VideoCall", ConsentVerificationMethod.VideoCall)]
    [InlineData("SignedForm", ConsentVerificationMethod.SignedForm)]
    public async Task Handle_AllValidMethods_ReturnsVerified(
        string methodString,
        ConsentVerificationMethod expectedMethod)
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", methodString);

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Verified");
        consent.VerificationMethod.Should().Be(expectedMethod);
    }

    #endregion

    #region Case-Insensitive Method Parsing

    [Theory]
    [InlineData("email")]
    [InlineData("EMAIL")]
    [InlineData("Email")]
    [InlineData("eMaIl")]
    public async Task Handle_EmailMethodCaseInsensitive_ReturnsVerified(string method)
    {
        // Arrange
        var consent = CreatePendingConsent();
        var command = new VerifyParentalConsentCommand("valid-token", method);

        _consentRepository
            .Setup(r => r.GetByVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(consent);

        // Act
        var result = await VerifyParentalConsentCommandHandler.Handle(
            command, _consentRepository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Verified");
        consent.VerificationMethod.Should().Be(ConsentVerificationMethod.Email);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Creates a pending consent that is not expired and not active,
    /// so the handler can proceed to method validation and approval.
    /// </summary>
    private static ParentalConsent CreatePendingConsent()
    {
        return new ParentalConsent
        {
            Id = "consent-789",
            ChildProfileId = "child-3",
            Status = ConsentStatus.Pending,
            RevokedAt = null,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(1), // not expired
            VerificationToken = "valid-token",
            ParentEmailHash = "hashed-parent-email"
        };
    }

    #endregion
}
