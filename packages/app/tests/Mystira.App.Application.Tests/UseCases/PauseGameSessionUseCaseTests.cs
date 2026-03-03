using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases;

public class PauseGameSessionUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<PauseGameSessionUseCase>> _logger;
    private readonly PauseGameSessionUseCase _useCase;

    public PauseGameSessionUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<PauseGameSessionUseCase>>();
        _useCase = new PauseGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithInProgressSession_PausesSuccessfully()
    {
        var session = new GameSession
        {
            Id = "session-1",
            Status = SessionStatus.InProgress,
            IsPaused = false
        };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.Status.Should().Be(SessionStatus.Paused);
        result.IsPaused.Should().BeTrue();
        result.PausedAt.Should().NotBeNull();
        result.PausedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptySessionId_ThrowsArgumentException(string? sessionId)
    {
        var act = () => _useCase.ExecuteAsync(sessionId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ThrowsArgumentException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var act = () => _useCase.ExecuteAsync("missing");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedSession_ThrowsInvalidOperationException()
    {
        var session = new GameSession { Id = "session-1", Status = SessionStatus.Completed };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var act = () => _useCase.ExecuteAsync("session-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*only pause sessions in progress*");
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyPausedSession_ThrowsInvalidOperationException()
    {
        var session = new GameSession { Id = "session-1", Status = SessionStatus.Paused };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var act = () => _useCase.ExecuteAsync("session-1");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*only pause sessions in progress*");
    }
}
