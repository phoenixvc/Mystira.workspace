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

public class GetSessionsByAccountQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetSessionsByAccountQueryHandlerTests()
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
                Status = SessionStatus.Completed,
                CurrentSceneId = "scene-1",
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9"
            },
            new GameSession
            {
                Id = "session-2",
                AccountId = accountId,
                ScenarioId = "scenario-2",
                ProfileId = "profile-1",
                Status = SessionStatus.InProgress,
                CurrentSceneId = "scene-5",
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                TargetAgeGroup = "6-9"
            }
        };

        var query = new GetSessionsByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetSessionsByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("session-1");
        result[0].Status.Should().Be("Completed");
        result[1].Id.Should().Be("session-2");
        result[1].Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_WithEmptyAccountId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetSessionsByAccountQuery("");

        // Act
        var act = async () => await GetSessionsByAccountQueryHandler.Handle(
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
        var query = new GetSessionsByAccountQuery(null!);

        // Act
        var act = async () => await GetSessionsByAccountQueryHandler.Handle(
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
        var query = new GetSessionsByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        // Act
        var result = await GetSessionsByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsCharacterAssignmentsCorrectly()
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
                        Role = "protagonist",
                        PlayerAssignment = new SessionPlayerAssignment
                        {
                            Type = PlayerType.Profile,
                            ProfileId = "profile-1",
                            ProfileName = "Player One"
                        }
                    }
                }
            }
        };

        var query = new GetSessionsByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetSessionsByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].CharacterAssignments.Should().NotBeNull();
        result[0].CharacterAssignments!.Should().HaveCount(1);
        var assignment = result[0].CharacterAssignments!.Single();
        assignment.CharacterId.Should().Be("char-1");
        assignment.CharacterName.Should().Be("Hero");
        assignment.PlayerAssignment.Should().NotBeNull();
        assignment.PlayerAssignment!.ProfileName.Should().Be("Player One");
    }

    [Fact]
    public async Task Handle_CalculatesChoiceAndEchoCounts()
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
                Status = SessionStatus.Completed,
                CurrentSceneId = "scene-5",
                StartTime = DateTime.UtcNow.AddHours(-1),
                TargetAgeGroup = "6-9",
                ChoiceHistory = new List<SessionChoice>
                {
                    new SessionChoice { SceneId = "scene-1", ChoiceText = "Option A" },
                    new SessionChoice { SceneId = "scene-2", ChoiceText = "Option B" },
                    new SessionChoice { SceneId = "scene-3", ChoiceText = "Option A" }
                },
                EchoHistory = new List<EchoLog>
                {
                    new EchoLog { EchoTypeId = "honesty", Description = "Told the truth" }
                }
            }
        };

        var query = new GetSessionsByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        // Act
        var result = await GetSessionsByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].ChoiceCount.Should().Be(3);
        result[0].EchoCount.Should().Be(1);
        result[0].SceneCount.Should().Be(3); // 3 distinct scenes
    }
}
