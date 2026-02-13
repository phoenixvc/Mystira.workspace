using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.Accounts.Commands;

public class CreateAccountCommandHandlerTests
{
    private readonly Mock<ICreateAccountUseCase> _useCase;
    private readonly Mock<ILogger> _logger;

    public CreateAccountCommandHandlerTests()
    {
        _useCase = new Mock<ICreateAccountUseCase>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WhenUseCaseSucceeds_ReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            Id = "account-1",
            Email = "test@example.com",
            ExternalUserId = "ext-1",
            DisplayName = "TestUser"
        };
        var command = new CreateAccountCommand("ext-1", "test@example.com", "TestUser", null, null, null);

        _useCase.Setup(u => u.ExecuteAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UseCaseResult<Account>.Success(account));

        // Act
        var result = await CreateAccountCommandHandler.Handle(
            command, _useCase.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("TestUser");
    }

    [Fact]
    public async Task Handle_WhenUseCaseFails_ReturnsNull()
    {
        // Arrange
        var command = new CreateAccountCommand("ext-1", "existing@example.com", null, null, null, null);

        _useCase.Setup(u => u.ExecuteAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UseCaseResult<Account>.Failure("Account with this email already exists"));

        // Act
        var result = await CreateAccountCommandHandler.Handle(
            command, _useCase.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUseCaseFails_LogsWarning()
    {
        // Arrange
        var command = new CreateAccountCommand("ext-1", "test@example.com", null, null, null, null);

        _useCase.Setup(u => u.ExecuteAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UseCaseResult<Account>.Failure("Email already exists"));

        // Act
        await CreateAccountCommandHandler.Handle(
            command, _useCase.Object, _logger.Object, CancellationToken.None);

        // Assert
        _logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
