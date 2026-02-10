using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.Accounts;

public class ValidateAccountQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public ValidateAccountQueryHandlerTests()
    {
        _repository = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsTrue()
    {
        var query = new ValidateAccountQuery("user@example.com");
        _repository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = "acc-1", Email = "user@example.com" });

        var result = await ValidateAccountQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistingEmail_ReturnsFalse()
    {
        var query = new ValidateAccountQuery("missing@example.com");
        _repository.Setup(r => r.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Account));

        var result = await ValidateAccountQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithNullOrEmptyEmail_ReturnsFalse(string? email)
    {
        var query = new ValidateAccountQuery(email!);

        var result = await ValidateAccountQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFalse()
    {
        var query = new ValidateAccountQuery("error@example.com");
        _repository.Setup(r => r.GetByEmailAsync("error@example.com", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await ValidateAccountQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DoesNotLogRawEmail()
    {
        // Verify that raw email is never passed to any log call
        var query = new ValidateAccountQuery("sensitive@example.com");
        _repository.Setup(r => r.GetByEmailAsync("sensitive@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = "acc-1", Email = "sensitive@example.com" });

        await ValidateAccountQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        // Verify no log call receives the raw email as a parameter
        _logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("sensitive@example.com")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "Raw email should never appear in log output");
    }
}
