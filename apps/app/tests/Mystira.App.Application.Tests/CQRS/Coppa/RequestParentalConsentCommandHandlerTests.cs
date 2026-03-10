using Mystira.Shared.Exceptions;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Coppa.Commands;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Coppa;

public class RequestParentalConsentCommandHandlerTests
{
    private readonly Mock<ICoppaConsentRepository> _consentRepo;
    private readonly Mock<IEmailService> _emailService;
    private readonly ConsentEmailBuilder _emailBuilder;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<RequestParentalConsentCommand>> _logger;

    public RequestParentalConsentCommandHandlerTests()
    {
        _consentRepo = new Mock<ICoppaConsentRepository>();
        _emailService = new Mock<IEmailService>();
        _emailService
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:BaseUrl"] = "https://test.mystira.app"
            })
            .Build();
        _emailBuilder = new ConsentEmailBuilder(config);
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<RequestParentalConsentCommand>>();
    }

    #region Validation

    [Fact]
    public async Task Handle_EmptyChildProfileId_ThrowsValidationException()
    {
        // Arrange
        var command = new RequestParentalConsentCommand("", "parent@example.com", "ChildName");

        // Act
        var act = () => RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhitespaceChildProfileId_ThrowsValidationException()
    {
        // Arrange
        var command = new RequestParentalConsentCommand("   ", "parent@example.com", "ChildName");

        // Act
        var act = () => RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_EmptyParentEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new RequestParentalConsentCommand("child-123", "", "ChildName");

        // Act
        var act = () => RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhitespaceParentEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new RequestParentalConsentCommand("child-123", "   ", "ChildName");

        // Act
        var act = () => RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_NullParentEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new RequestParentalConsentCommand("child-123", null!, "ChildName");

        // Act
        var act = () => RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region AlreadyVerified

    [Fact]
    public async Task Handle_ExistingActiveConsent_ReturnsAlreadyVerified()
    {
        // Arrange
        var existingConsent = new ParentalConsent
        {
            Id = "existing-consent-id",
            ChildProfileId = "child-123",
            Status = ConsentStatus.Verified,
            RevokedAt = null // IsActive = Verified && RevokedAt == null
        };

        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsent);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        var result = await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("AlreadyVerified");
        result.ConsentId.Should().Be("existing-consent-id");
        result.Message.Should().Contain("already verified");
    }

    [Fact]
    public async Task Handle_ExistingActiveConsent_DoesNotCallAddAsync()
    {
        // Arrange
        var existingConsent = new ParentalConsent
        {
            ChildProfileId = "child-123",
            Status = ConsentStatus.Verified,
            RevokedAt = null
        };

        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsent);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepo.Verify(
            r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingRevokedConsent_ReusesExistingRecord()
    {
        // Arrange - consent exists but was revoked (IsActive = false)
        var revokedConsent = new ParentalConsent
        {
            ChildProfileId = "child-123",
            Status = ConsentStatus.Revoked,
            RevokedAt = DateTime.UtcNow.AddDays(-1)
        };

        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedConsent);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        var result = await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert - handler re-uses existing record via UpdateAsync, not AddAsync
        result.Status.Should().Be("EmailSent");
        _consentRepo.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        _consentRepo.Verify(
            r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingPendingConsent_ReusesExistingRecord()
    {
        // Arrange - consent exists but is only Pending (not active)
        var pendingConsent = new ParentalConsent
        {
            ChildProfileId = "child-123",
            Status = ConsentStatus.Pending
        };

        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync("child-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingConsent);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        var result = await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert - handler re-uses existing Pending record via UpdateAsync
        result.Status.Should().Be("EmailSent");
        _consentRepo.Verify(
            r => r.UpdateAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region New Consent Creation

    [Fact]
    public async Task Handle_NoExistingConsent_CreatesNewConsentAndReturnsEmailSent()
    {
        // Arrange
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        var result = await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Status.Should().Be("EmailSent");
        result.Message.Should().Contain("email sent");
        result.ConsentId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_NoExistingConsent_CallsAddAsyncOnce()
    {
        // Arrange
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        _consentRepo.Verify(
            r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoExistingConsent_SetsChildProfileIdOnConsent()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.ChildProfileId.Should().Be("child-123");
        capturedConsent.ChildDisplayName.Should().Be("ChildName");
    }

    [Fact]
    public async Task Handle_NoExistingConsent_SetsStatusToEmailSent()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.Status.Should().Be(ConsentStatus.EmailSent);
    }

    #endregion

    #region Email Hashing

    [Fact]
    public async Task Handle_StoresHashedEmailNotPlainText()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.ParentEmailHash.Should().NotBe("parent@example.com");
        capturedConsent.ParentEmailHash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_HashesEmailUsingSha256()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        const string email = "parent@example.com";
        var expectedHash = ComputeSha256Hash(email);

        var command = new RequestParentalConsentCommand("child-123", email, "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.ParentEmailHash.Should().Be(expectedHash);
    }

    [Fact]
    public async Task Handle_NormalizesEmailBeforeHashing()
    {
        // Arrange - email with mixed case and whitespace should produce
        // the same hash as the trimmed lowercase version.
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        const string email = "  Parent@Example.COM  ";
        var expectedHash = ComputeSha256Hash("parent@example.com");

        var command = new RequestParentalConsentCommand("child-123", email, "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.ParentEmailHash.Should().Be(expectedHash);
    }

    #endregion

    #region Verification Token

    [Fact]
    public async Task Handle_GeneratesNonEmptyVerificationToken()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.VerificationToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_GeneratesUniqueVerificationTokensAcrossCalls()
    {
        // Arrange
        var capturedTokens = new List<string>();
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedTokens.Add(c.VerificationToken!));

        var command1 = new RequestParentalConsentCommand("child-1", "parent1@example.com", "Child1");
        var command2 = new RequestParentalConsentCommand("child-2", "parent2@example.com", "Child2");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command1, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);
        await RequestParentalConsentCommandHandler.Handle(
            command2, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedTokens.Should().HaveCount(2);
        capturedTokens[0].Should().NotBe(capturedTokens[1]);
    }

    [Fact]
    public async Task Handle_VerificationTokenIsBase64Encoded()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        var act = () => Convert.FromBase64String(capturedConsent!.VerificationToken!);
        act.Should().NotThrow("the verification token should be a valid Base64 string");
    }

    #endregion

    #region Token Expiration

    [Fact]
    public async Task Handle_SetsVerificationTokenExpiresAt48HoursFromNow()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");
        var before = DateTime.UtcNow.AddHours(48);

        // Act
        await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        var after = DateTime.UtcNow.AddHours(48);

        // Assert
        capturedConsent.Should().NotBeNull();
        capturedConsent!.VerificationTokenExpiresAt.Should().NotBeNull();
        capturedConsent.VerificationTokenExpiresAt!.Value.Should().BeOnOrAfter(before);
        capturedConsent.VerificationTokenExpiresAt!.Value.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Result Mapping

    [Fact]
    public async Task Handle_NoExistingConsent_ReturnsConsentIdMatchingPersistedRecord()
    {
        // Arrange
        ParentalConsent? capturedConsent = null;
        _consentRepo
            .Setup(r => r.GetByChildProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParentalConsent?)null);
        _consentRepo
            .Setup(r => r.AddAsync(It.IsAny<ParentalConsent>(), It.IsAny<CancellationToken>()))
            .Callback<ParentalConsent, CancellationToken>((c, _) => capturedConsent = c);

        var command = new RequestParentalConsentCommand("child-123", "parent@example.com", "ChildName");

        // Act
        var result = await RequestParentalConsentCommandHandler.Handle(
            command, _consentRepo.Object, _emailService.Object, _emailBuilder, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        // Assert
        capturedConsent.Should().NotBeNull();
        result.ConsentId.Should().Be(capturedConsent!.Id);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Mirrors the handler's internal HashEmail logic for test assertions.
    /// </summary>
    private static string ComputeSha256Hash(string email)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    #endregion
}
