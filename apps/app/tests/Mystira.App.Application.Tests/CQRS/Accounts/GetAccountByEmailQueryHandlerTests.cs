using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Queries;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Accounts;

public class GetAccountByEmailQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetAccountByEmailQueryHandlerTests()
    {
        _repository = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsAccount()
    {
        // Arrange
        var email = "test@example.com";
        var expectedAccount = new Account
        {
            Id = "account-123",
            Email = email,
            ExternalUserId = "ext-123",
            DisplayName = "Test User"
        };

        var query = new GetAccountByEmailQuery(email);

        _repository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await GetAccountByEmailQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Id.Should().Be("account-123");
        result.DisplayName.Should().Be("Test User");

        _repository.Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var query = new GetAccountByEmailQuery(email);

        _repository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        // Act
        var result = await GetAccountByEmailQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repository.Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("USER@DOMAIN.COM")]
    [InlineData("user.name@domain.co.uk")]
    public async Task Handle_WithVariousEmailFormats_CallsRepositoryCorrectly(string email)
    {
        // Arrange
        var query = new GetAccountByEmailQuery(email);
        var expectedAccount = new Account
        {
            Id = "account-test",
            Email = email,
            ExternalUserId = "ext-test"
        };

        _repository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await GetAccountByEmailQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ReturnsNull()
    {
        // Arrange
        var query = new GetAccountByEmailQuery(string.Empty);

        _repository.Setup(r => r.GetByEmailAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        // Act
        var result = await GetAccountByEmailQueryHandler.Handle(
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
        var email = "logging@example.com";
        var query = new GetAccountByEmailQuery(email);

        _repository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = "test-id", Email = email });

        // Act
        await GetAccountByEmailQueryHandler.Handle(
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
    public async Task Handle_DoesNotModifyRepository()
    {
        // Arrange
        var email = "readonly@example.com";
        var query = new GetAccountByEmailQuery(email);

        _repository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = "test", Email = email });

        // Act
        await GetAccountByEmailQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert - verify no write operations were called
        _repository.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
