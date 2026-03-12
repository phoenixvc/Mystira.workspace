using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class GetSessionsByProfileQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetSessionsByProfileQueryHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidProfileId_ReturnsSessions()
    {
        // Arrange
        var profileId = "profile-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-1",
                AccountId = "account-123",
                ScenarioId = "scenario-1",
                ProfileId = profileId,
                Status = SessionStatus.Completed,
                CurrentSceneId = "scene-1",
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            },
            new GameSession
            {
                Id = "session-2",
                AccountId = "account-123",
                ScenarioId = "scenario-2",
                ProfileId = profileId,
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-5",
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetSessionsByProfileQuery(profileId);

        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetSessionsByProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].ProfileId.Should().Be(profileId);
        result[1].ProfileId.Should().Be(profileId);
    }

    [Fact]
    public async Task Handle_WithEmptyProfileId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetSessionsByProfileQuery("");

        // Act
        var act = async () => await GetSessionsByProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*ProfileId is required*");
    }

    [Fact]
    public async Task Handle_WithNullProfileId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetSessionsByProfileQuery(null!);

        // Act
        var act = async () => await GetSessionsByProfileQueryHandler.Handle(
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
        var profileId = "profile-no-sessions";
        var query = new GetSessionsByProfileQuery(profileId);

        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        // Act
        var result = await GetSessionsByProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CalculatesChoiceAndEchoCounts()
    {
        // Arrange
        var profileId = "profile-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-1",
                AccountId = "account-123",
                ScenarioId = "scenario-1",
                ProfileId = profileId,
                Status = SessionStatus.Completed,
                CurrentSceneId = "scene-5",
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9",
                ChoiceHistory = new List<SessionChoice>
                {
                    new SessionChoice { SceneId = "scene-1", ChoiceText = "Option A" },
                    new SessionChoice { SceneId = "scene-2", ChoiceText = "Option B" },
                    new SessionChoice { SceneId = "scene-1", ChoiceText = "Option C" } // Same scene, different choice
                },
                EchoHistory = new List<EchoLog>
                {
                    new EchoLog { EchoTypeId = "honesty", Description = "Told the truth" },
                    new EchoLog { EchoTypeId = "courage", Description = "Faced fear" }
                },
                Achievements = new List<SessionAchievement>
                {
                    new SessionAchievement { Id = "ach-1", Title = "First Steps" }
                }
            }
        };

        var query = new GetSessionsByProfileQuery(profileId);

        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetSessionsByProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].ChoiceCount.Should().Be(3);
        result[0].EchoCount.Should().Be(2);
        result[0].AchievementCount.Should().Be(1);
        result[0].SceneCount.Should().Be(2); // 2 distinct scenes (scene-1 and scene-2)
    }

    [Fact]
    public async Task Handle_IdentifiesPausedSessions()
    {
        // Arrange
        var profileId = "profile-123";
        var sessions = new List<GameSession>
        {
            new GameSession
            {
                Id = "session-1",
                ProfileId = profileId,
                Status = SessionStatus.Paused,
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            },
            new GameSession
            {
                Id = "session-2",
                ProfileId = profileId,
                Status = SessionStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetSessionsByProfileQuery(profileId);

        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetSessionsByProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsPaused.Should().BeTrue();
        result[1].IsPaused.Should().BeFalse();
    }
}
