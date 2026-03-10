using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases;

public class EndGameSessionUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<EndGameSessionUseCase>> _logger;
    private readonly EndGameSessionUseCase _useCase;

    public EndGameSessionUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<EndGameSessionUseCase>>();
        _useCase = new EndGameSessionUseCase(
            _repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInProgressSession_CompletesSession()
    {
        var session = CreateTestSession(SessionStatus.InProgress);
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.Should().NotBeNull();
        result.Status.Should().Be(SessionStatus.Completed);
        result.EndTime.Should().NotBeNull();
        result.IsPaused.Should().BeFalse();
        result.PausedAt.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPausedSession_CompletesSession()
    {
        var session = CreateTestSession(SessionStatus.Paused);
        session.IsPaused = true;
        session.PausedAt = DateTime.UtcNow.AddMinutes(-5);
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.Status.Should().Be(SessionStatus.Completed);
        result.IsPaused.Should().BeFalse();
        result.PausedAt.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyCompletedSession_ReturnsSameSession()
    {
        var session = CreateTestSession(SessionStatus.Completed);
        session.EndTime = DateTime.UtcNow.AddMinutes(-10);
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.Status.Should().Be(SessionStatus.Completed);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SetsElapsedTime()
    {
        var session = CreateTestSession(SessionStatus.InProgress);
        session.StartTime = DateTime.UtcNow.AddMinutes(-30);
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.ElapsedTime.TotalMinutes.Should().BeApproximately(30, 1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ThrowsValidationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var act = () => _useCase.ExecuteAsync("missing");

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptySessionId_ThrowsValidationException(string? sessionId)
    {
        var act = () => _useCase.ExecuteAsync(sessionId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static GameSession CreateTestSession(SessionStatus status)
    {
        return new GameSession
        {
            Id = "session-1",
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            Status = status,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            PlayerNames = new List<string> { "Player1" },
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>(),
            CompassValues = new Dictionary<string, CompassTracking>()
        };
    }
}
