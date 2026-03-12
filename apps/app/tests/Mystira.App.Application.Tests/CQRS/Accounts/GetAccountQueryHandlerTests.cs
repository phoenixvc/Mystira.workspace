using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.Accounts.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Accounts;

public class GetAccountQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetAccountQueryHandlerTests()
    {
        _repository = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingAccountId_ReturnsAccount()
    {
        // Arrange
        var accountId = "account-123";
        var expectedAccount = new Account
        {
            Id = accountId,
            Email = "test@example.com",
            ExternalUserId = "ext-123",
            DisplayName = "Test User"
        };

        var query = new GetAccountQuery(accountId);

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await GetAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountId);
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");

        _repository.Verify(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingAccountId_ReturnsNull()
    {
        // Arrange
        var accountId = "non-existent-id";
        var query = new GetAccountQuery(accountId);

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        // Act
        var result = await GetAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repository.Verify(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyAccountId_ReturnsNull()
    {
        // Arrange
        var query = new GetAccountQuery(string.Empty);

        _repository.Setup(r => r.GetByIdAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        // Act
        var result = await GetAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_LogsDebugMessage()
    {
        // Arrange
        var accountId = "account-456";
        var query = new GetAccountQuery(accountId);

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = accountId, Email = "test@test.com" });

        // Act
        await GetAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancelledToken_DoesNotThrow()
    {
        // Arrange
        var accountId = "account-789";
        var query = new GetAccountQuery(accountId);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel before calling handler

        _repository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = accountId, Email = "test@test.com" });

        // Act & Assert - Handler should handle cancelled token gracefully
        var act = () => GetAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            cts.Token);

        await act.Should().NotThrowAsync();
    }
}
