using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class PauseGameSessionCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public PauseGameSessionCommandHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithInProgressSession_PausesSuccessfully()
    {
        // Arrange
        var sessionId = "session-123";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.InProgress,
            IsPaused = false
        };

        var command = new PauseGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await PauseGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(SessionStatus.Paused);
    }

    [Fact]
    public async Task Handle_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        var command = new PauseGameSessionCommand("non-existent");

        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        // Act
        var result = await PauseGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithAlreadyPausedSession_ReturnsNull()
    {
        // Arrange
        var sessionId = "paused-session";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.Paused,
            IsPaused = true
        };

        var command = new PauseGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await PauseGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCompletedSession_ReturnsNull()
    {
        // Arrange
        var sessionId = "completed-session";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.Completed
        };

        var command = new PauseGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await PauseGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryUpdate_WhenSessionIsInProgress()
    {
        // Arrange
        var sessionId = "session-456";
        var session = new GameSession { Id = sessionId, Status = SessionStatus.InProgress };
        var command = new PauseGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        await PauseGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.Is<GameSession>(s => s.Id == sessionId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotCallUpdate_WhenSessionNotInProgress()
    {
        // Arrange
        var session = new GameSession { Id = "session", Status = SessionStatus.Completed };
        var command = new PauseGameSessionCommand("session");

        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        await PauseGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
