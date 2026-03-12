using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.GameSessions.Commands;
using Mystira.Core.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.GameSessions;

namespace Mystira.App.Application.Tests.CQRS.GameSessions.Commands;

public class StartGameSessionCommandHandlerTests
{
    private readonly Mock<ICreateGameSessionUseCase> _useCase;
    private readonly Mock<ILogger> _logger;

    public StartGameSessionCommandHandlerTests()
    {
        _useCase = new Mock<ICreateGameSessionUseCase>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WhenUseCaseSucceeds_ReturnsGameSession()
    {
        // Arrange
        var session = new GameSession
        {
            Id = "session-1",
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            Status = SessionStatus.InProgress
        };
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };
        var command = new StartGameSessionCommand(request);

        _useCase.Setup(u => u.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UseCaseResult<GameSession>.Success(session));

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("session-1");
        result.Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact]
    public async Task Handle_WhenUseCaseFails_ReturnsNull()
    {
        // Arrange
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };
        var command = new StartGameSessionCommand(request);

        _useCase.Setup(u => u.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UseCaseResult<GameSession>.Failure("Scenario not found"));

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase.Object, _logger.Object, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUseCaseFails_LogsWarning()
    {
        // Arrange
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };
        var command = new StartGameSessionCommand(request);

        _useCase.Setup(u => u.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UseCaseResult<GameSession>.Failure("ScenarioId is required"));

        // Act
        await StartGameSessionCommandHandler.Handle(
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
