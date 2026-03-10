using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.Accounts;

public class UpdateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public UpdateAccountCommandHandlerTests()
    {
        _repository = new Mock<IAccountRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingAccount_UpdatesAndReturnsAccount()
    {
        // Arrange
        var accountId = "account-123";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            DisplayName = "Old Name",
            ExternalUserId = "ext-123"
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: "New Name",
            UserProfileIds: null,
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("New Name");

        _repository.Verify(r => r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingAccount_ReturnsNull()
    {
        // Arrange
        var accountId = "non-existent-account";
        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: "New Name",
            UserProfileIds: null,
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _repository.Verify(r => r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullDisplayName_DoesNotUpdateDisplayName()
    {
        // Arrange
        var accountId = "account-456";
        var originalDisplayName = "Original Name";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            DisplayName = originalDisplayName,
            ExternalUserId = "ext-456"
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: null,
            UserProfileIds: new List<string> { "profile-1" },
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be(originalDisplayName);
    }

    [Fact]
    public async Task Handle_WithEmptyDisplayName_DoesNotUpdateDisplayName()
    {
        // Arrange
        var accountId = "account-789";
        var originalDisplayName = "Original Name";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            DisplayName = originalDisplayName,
            ExternalUserId = "ext-789"
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: "",
            UserProfileIds: null,
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be(originalDisplayName);
    }

    [Fact]
    public async Task Handle_WithUserProfileIds_UpdatesProfileIds()
    {
        // Arrange
        var accountId = "account-profiles";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            UserProfileIds = new List<string> { "old-profile" },
            ExternalUserId = "ext-profiles"
        };

        var newProfileIds = new List<string> { "profile-1", "profile-2", "profile-3" };
        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: null,
            UserProfileIds: newProfileIds,
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserProfileIds.Should().BeEquivalentTo(newProfileIds);
    }

    [Fact]
    public async Task Handle_WithSubscription_UpdatesSubscription()
    {
        // Arrange
        var accountId = "account-subscription";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            ExternalUserId = "ext-sub"
        };

        var newSubscription = new SubscriptionDetails
        {
            Tier = "premium",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: null,
            UserProfileIds: null,
            Subscription: newSubscription,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Subscription.Should().NotBeNull();
        result.Subscription.Tier.Should().Be("premium");
    }

    [Fact]
    public async Task Handle_WithSettings_UpdatesSettings()
    {
        // Arrange
        var accountId = "account-settings";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            ExternalUserId = "ext-settings"
        };

        var newSettings = new AccountSettings
        {
            NotificationsEnabled = true,
            PreferredLanguage = "es"
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: null,
            UserProfileIds: null,
            Subscription: null,
            Settings: newSettings
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Settings.Should().NotBeNull();
        result.Settings.NotificationsEnabled.Should().BeTrue();
        result.Settings.PreferredLanguage.Should().Be("es");
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_LogsWarning()
    {
        // Arrange
        var accountId = "missing-account";
        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: "Test",
            UserProfileIds: null,
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        // Act
        await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_LogsInformation()
    {
        // Arrange
        var accountId = "account-log";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            ExternalUserId = "ext-log"
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: "New Name",
            UserProfileIds: null,
            Subscription: null,
            Settings: null
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithAllFieldsProvided_UpdatesAllFields()
    {
        // Arrange
        var accountId = "account-full";
        var existingAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            DisplayName = "Old Name",
            ExternalUserId = "ext-full"
        };

        var command = new UpdateAccountCommand(
            AccountId: accountId,
            DisplayName: "New Display Name",
            UserProfileIds: new List<string> { "profile-a", "profile-b" },
            Subscription: new SubscriptionDetails { Tier = "gold" },
            Settings: new AccountSettings { NotificationsEnabled = false }
        );

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await UpdateAccountCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("New Display Name");
        result.UserProfileIds.Should().HaveCount(2);
        result.Subscription!.Tier.Should().Be("gold");
        result.Settings!.NotificationsEnabled.Should().BeFalse();
    }
}
