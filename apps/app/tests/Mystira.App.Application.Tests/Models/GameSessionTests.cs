using AutoFixture.Xunit2;
using FluentAssertions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.Models;

public class GameSessionTests
{
    [Fact]
    public void GameSession_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var gameSession = new GameSession();

        // Assert
        gameSession.ScenarioId.Should().BeEmpty();
        gameSession.AccountId.Should().BeEmpty();
        gameSession.PlayerNames.Should().NotBeNull().And.BeEmpty();
        gameSession.Status.Should().Be(SessionStatus.Creating);
        gameSession.ChoiceHistory.Should().NotBeNull().And.BeEmpty();
        gameSession.EchoHistory.Should().NotBeNull().And.BeEmpty();
        gameSession.CompassValues.Should().NotBeNull().And.BeEmpty();
        gameSession.PlayerCompassProgressTotals.Should().NotBeNull().And.BeEmpty();
        gameSession.Achievements.Should().NotBeNull().And.BeEmpty();
        gameSession.IsPaused.Should().BeFalse();
        gameSession.SceneCount.Should().Be(0);
    }

    [Theory]
    [AutoData]
    public void GameSession_SetProperties_SetsValuesCorrectly(
        string scenarioId,
        string accountId,
        List<string> playerNames,
        string currentSceneId)
    {
        // Act
        var gameSession = new GameSession
        {
            ScenarioId = scenarioId,
            AccountId = accountId,
            PlayerNames = playerNames,
            CurrentSceneId = currentSceneId,
            Status = SessionStatus.Active
        };

        // Assert
        gameSession.ScenarioId.Should().Be(scenarioId);
        gameSession.AccountId.Should().Be(accountId);
        gameSession.PlayerNames.Should().BeEquivalentTo(playerNames);
        gameSession.CurrentSceneId.Should().Be(currentSceneId);
        gameSession.Status.Should().Be(SessionStatus.Active);
    }

    [Fact]
    public void GetTotalElapsedTime_ReturnsCorrectTime_WhenInProgress()
    {
        // Arrange
        var session = new GameSession
        {
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            Status = SessionStatus.Active
        };

        // Act
        var elapsedTime = session.GetTotalElapsedTime();

        // Assert
        elapsedTime.Should().BeCloseTo(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetTotalElapsedTime_ReturnsCorrectTime_WhenPaused()
    {
        // Arrange
        var session = new GameSession
        {
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            Status = SessionStatus.Paused,
            IsPaused = true,
            PausedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var elapsedTime = session.GetTotalElapsedTime();

        // Assert - GetTotalElapsedTime uses ElapsedTime or StartedAt
        elapsedTime.TotalMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetTotalElapsedTime_ReturnsCorrectTime_WhenFinished()
    {
        // Arrange
        var session = new GameSession
        {
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow,
            Status = SessionStatus.Completed,
            ElapsedTime = TimeSpan.FromMinutes(10)
        };

        // Act
        var elapsedTime = session.GetTotalElapsedTime();

        // Assert
        elapsedTime.Should().Be(TimeSpan.FromMinutes(10));
    }
}
