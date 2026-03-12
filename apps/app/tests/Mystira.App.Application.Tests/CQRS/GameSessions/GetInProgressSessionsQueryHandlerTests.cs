using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.GameSessions.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class GetInProgressSessionsQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetInProgressSessionsQueryHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidAccountId_ReturnsSessions()
    {
        // Arrange
        var accountId = "account-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-1",
                AccountId = accountId,
                ScenarioId = "scenario-1",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-1",
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetInProgressSessionsQuery(accountId);

        _repository.Setup(r => r.GetInProgressSessionsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("session-1");
        result[0].AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task Handle_WithEmptyAccountId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetInProgressSessionsQuery("");

        // Act
        var act = async () => await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*AccountId is required*");
    }

    [Fact]
    public async Task Handle_WithNullAccountId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetInProgressSessionsQuery(null!);

        // Act
        var act = async () => await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var accountId = "account-no-sessions";
        var query = new GetInProgressSessionsQuery(accountId);

        _repository.Setup(r => r.GetInProgressSessionsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        // Act
        var result = await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FiltersDuplicateSessions_BySameScenarioAndProfile()
    {
        // Arrange
        var accountId = "account-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-older",
                AccountId = accountId,
                ScenarioId = "scenario-1",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-1",
                StartTime = DateTime.UtcNow.AddHours(-2),
                TargetAgeGroup = "6-9"
            },
            new GameSession
            {
                Id = "session-newer",
                AccountId = accountId,
                ScenarioId = "scenario-1",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-2",
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetInProgressSessionsQuery(accountId);

        _repository.Setup(r => r.GetInProgressSessionsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert - should deduplicate and keep the newer one
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("session-newer");
    }

    [Fact]
    public async Task Handle_FiltersEmptySessions_WithNoSceneOrHistory()
    {
        // Arrange
        var accountId = "account-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-empty",
                AccountId = accountId,
                ScenarioId = "scenario-1",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = null!, // No scene
                ChoiceHistory = new List<SessionChoice>(), // No history
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            },
            new GameSession
            {
                Id = "session-valid",
                AccountId = accountId,
                ScenarioId = "scenario-2",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-1", // Has scene
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetInProgressSessionsQuery(accountId);

        _repository.Setup(r => r.GetInProgressSessionsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert - should filter out empty session
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("session-valid");
    }

    [Fact]
    public async Task Handle_IncludesSessionsWithChoiceHistory()
    {
        // Arrange
        var accountId = "account-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-with-history",
                AccountId = accountId,
                ScenarioId = "scenario-1",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = null!, // No current scene but has history
                ChoiceHistory = new List<SessionChoice>
                {
                    new SessionChoice { SceneId = "scene-1", ChoiceText = "Option A" }
                },
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetInProgressSessionsQuery(accountId);

        _repository.Setup(r => r.GetInProgressSessionsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert - should include session with choice history
        result.Should().HaveCount(1);
        result[0].ChoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MapsCharacterAssignments()
    {
        // Arrange
        var accountId = "account-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-1",
                AccountId = accountId,
                ScenarioId = "scenario-1",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-1",
                StartTime = DateTime.UtcNow,
                TargetAgeGroup = "6-9",
                CharacterAssignments = new List<SessionCharacterAssignment>
                {
                    new SessionCharacterAssignment
                    {
                        CharacterId = "char-1",
                        CharacterName = "Hero",
                        Role = "protagonist"
                    }
                }
            }
        };

        var query = new GetInProgressSessionsQuery(accountId);

        _repository.Setup(r => r.GetInProgressSessionsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetInProgressSessionsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].CharacterAssignments.Should().HaveCount(1);
        result[0].CharacterAssignments![0].CharacterId.Should().Be("char-1");
        result[0].CharacterAssignments![0].CharacterName.Should().Be("Hero");
    }
}
