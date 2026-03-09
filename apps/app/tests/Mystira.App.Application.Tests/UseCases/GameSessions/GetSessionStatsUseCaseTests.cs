using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.GameSessions;

public class GetSessionStatsUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger<GetSessionStatsUseCase>> _logger;
    private readonly GetSessionStatsUseCase _useCase;

    public GetSessionStatsUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger<GetSessionStatsUseCase>>();
        _useCase = new GetSessionStatsUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingSession_ReturnsStats()
    {
        // Arrange
        var session = new GameSession
        {
            Id = "session-1",
            ProfileId = "profile-1",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            Status = SessionStatus.Completed,
            ChoiceHistory = new List<SessionChoice>
            {
                new()
                {
                    SceneId = "scene-1",
                    ChoiceText = "Help the stranger",
                    CompassAxis = "empathy",
                    CompassDelta = 0.5,
                    PlayerId = "profile-1",
                    ChosenAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new()
                {
                    SceneId = "scene-2",
                    ChoiceText = "Share the treasure",
                    CompassAxis = "generosity",
                    CompassDelta = 0.3,
                    PlayerId = "profile-1",
                    ChosenAt = DateTime.UtcNow.AddMinutes(-15)
                }
            },
            EchoHistory = new List<EchoLog>
            {
                new() { EchoType = "honesty", Description = "Told the truth", Strength = 0.8, Timestamp = DateTime.UtcNow.AddMinutes(-20) }
            },
            Achievements = new List<SessionAchievement>
            {
                new() { Title = "First Choice", Type = AchievementType.FirstChoice }
            }
        };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _useCase.ExecuteAsync("session-1");

        // Assert
        result.Should().NotBeNull();
        result!.TotalChoices.Should().Be(2);
        result.CompassValues.Should().ContainKey("empathy");
        result.CompassValues["empathy"].Should().Be(0.5);
        result.PlayerCompassProgressTotals.Should().NotBeEmpty();
        result.RecentEchoes.Should().HaveCount(1);
        result.Achievements.Should().HaveCount(1);
        result.SessionDuration.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        // Act
        var result = await _useCase.ExecuteAsync("missing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithInProgressSession_CalculatesDuration()
    {
        // Arrange
        var session = new GameSession
        {
            Id = "session-2",
            ProfileId = "profile-1",
            StartTime = DateTime.UtcNow.AddMinutes(-30),
            EndTime = null,
            Status = SessionStatus.InProgress,
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>()
        };

        _repository.Setup(r => r.GetByIdAsync("session-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _useCase.ExecuteAsync("session-2");

        // Assert
        result.Should().NotBeNull();
        result!.SessionDuration.Should().BeGreaterThan(TimeSpan.Zero);
        result.TotalChoices.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsArgumentException(string? sessionId)
    {
        var act = () => _useCase.ExecuteAsync(sessionId!);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
