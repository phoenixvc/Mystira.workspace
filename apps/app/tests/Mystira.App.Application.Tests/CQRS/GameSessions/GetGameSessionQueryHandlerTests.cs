using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class GetGameSessionQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetGameSessionQueryHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingSessionId_ReturnsSession()
    {
        // Arrange
        var sessionId = "session-123";
        var expectedSession = new GameSession
        {
            Id = sessionId,
            AccountId = "account-1",
            ScenarioId = "scenario-1",
            Status = SessionStatus.InProgress
        };

        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await GetGameSessionQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sessionId);
        result.AccountId.Should().Be("account-1");
        result.Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact]
    public async Task Handle_WithNonExistingSessionId_ReturnsNull()
    {
        // Arrange
        var sessionId = "non-existent-session";
        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        // Act
        var result = await GetGameSessionQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_LogsDebug()
    {
        // Arrange
        var sessionId = "missing-session";
        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        // Act
        await GetGameSessionQueryHandler.Handle(
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
    public async Task Handle_WhenSessionFound_LogsDebug()
    {
        // Arrange
        var sessionId = "found-session";
        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameSession { Id = sessionId });

        // Act
        await GetGameSessionQueryHandler.Handle(
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
    public async Task Handle_ReturnsSessionWithAllProperties()
    {
        // Arrange
        var sessionId = "complete-session";
        var now = DateTime.UtcNow;
        var expectedSession = new GameSession
        {
            Id = sessionId,
            AccountId = "account-456",
            ScenarioId = "scenario-789",
            Status = SessionStatus.InProgress,
            CurrentSceneId = "scene-1",
            StartTime = now.AddHours(-1),
            ChoiceHistory = new List<SessionChoice>
            {
                new SessionChoice { SceneId = "intro", ChoiceText = "Begin" },
                new SessionChoice { SceneId = "scene-1", ChoiceText = "Continue" }
            },
            CharacterAssignments = new List<SessionCharacterAssignment>
            {
                new SessionCharacterAssignment { CharacterId = "char-1", CharacterName = "Hero" }
            }
        };

        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await GetGameSessionQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sessionId);
        result.AccountId.Should().Be("account-456");
        result.ScenarioId.Should().Be("scenario-789");
        result.Status.Should().Be(SessionStatus.InProgress);
        result.CurrentSceneId.Should().Be("scene-1");
        result.ChoiceHistory.Should().HaveCount(2);
        result.CharacterAssignments.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(SessionStatus.InProgress)]
    [InlineData(SessionStatus.Completed)]
    [InlineData(SessionStatus.Abandoned)]
    [InlineData(SessionStatus.Paused)]
    public async Task Handle_ReturnsSessionsWithDifferentStatuses(SessionStatus status)
    {
        // Arrange
        var sessionId = $"session-{status}";
        var expectedSession = new GameSession
        {
            Id = sessionId,
            Status = status
        };

        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await GetGameSessionQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(status);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectSessionId()
    {
        // Arrange
        var sessionId = "specific-session-id";
        var query = new GetGameSessionQuery(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameSession { Id = sessionId });

        // Act
        await GetGameSessionQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
