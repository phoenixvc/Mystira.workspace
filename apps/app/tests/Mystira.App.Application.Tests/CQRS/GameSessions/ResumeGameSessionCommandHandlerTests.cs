using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class ResumeGameSessionCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public ResumeGameSessionCommandHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithPausedSession_ResumesSuccessfully()
    {
        // Arrange
        var sessionId = "session-123";
        var pausedAt = DateTime.UtcNow.AddMinutes(-10);
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.Paused,
            IsPaused = true,
            PausedAt = pausedAt
        };

        var command = new ResumeGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await ResumeGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(SessionStatus.InProgress);
        result.IsPaused.Should().BeFalse();
        result.PausedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        var command = new ResumeGameSessionCommand("non-existent");

        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        // Act
        var result = await ResumeGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonPausedSession_ReturnsNull()
    {
        // Arrange - Session is InProgress (not Paused), so Resume() domain method
        // returns false and the handler returns null
        var sessionId = "in-progress-session";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.InProgress,
            IsPaused = false
        };

        var command = new ResumeGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await ResumeGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsRepositoryUpdate()
    {
        // Arrange
        var sessionId = "session-456";
        var session = new GameSession { Id = sessionId, Status = SessionStatus.Paused, IsPaused = true };
        var command = new ResumeGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        await ResumeGameSessionCommandHandler.Handle(
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
    public async Task Handle_ClearsPausedAt_WhenResuming()
    {
        // Arrange
        var sessionId = "session-789";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.Paused,
            IsPaused = true,
            PausedAt = DateTime.UtcNow.AddHours(-1)
        };

        var command = new ResumeGameSessionCommand(sessionId);

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await ResumeGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PausedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_DoesNotCallUpdate()
    {
        // Arrange
        var command = new ResumeGameSessionCommand("missing-session");

        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        // Act
        await ResumeGameSessionCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
