using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class EndGameSessionCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<EndGameSessionUseCase>> _logger;

    public EndGameSessionCommandHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<EndGameSessionUseCase>>();
    }

    [Fact]
    public async Task Handle_WithExistingSession_EndsSessionSuccessfully()
    {
        // Arrange
        var sessionId = "session-123";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.InProgress,
            AccountId = "account-1",
            ScenarioId = "scenario-1",
            StartTime = DateTime.UtcNow.AddMinutes(-30)
        };

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var useCase = new EndGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
        var command = new EndGameSessionCommand(sessionId);

        // Act
        var result = await EndGameSessionCommandHandler.Handle(command, useCase, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(SessionStatus.Completed);
        result.EndTime.Should().NotBeNull();
        result.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.IsPaused.Should().BeFalse();
        result.PausedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        var sessionId = "non-existent";
        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var useCase = new EndGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
        var command = new EndGameSessionCommand(sessionId);

        // Act
        var result = await EndGameSessionCommandHandler.Handle(command, useCase, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithAlreadyCompletedSession_ReturnsSessionWithoutUpdate()
    {
        // Arrange
        var sessionId = "completed-session";
        var session = new GameSession
        {
            Id = sessionId,
            Status = SessionStatus.Completed,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddMinutes(-30)
        };

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var useCase = new EndGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
        var command = new EndGameSessionCommand(sessionId);

        // Act
        var result = await EndGameSessionCommandHandler.Handle(command, useCase, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(SessionStatus.Completed);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptySessionId_ReturnsNull()
    {
        // Arrange - UseCase throws ValidationException for empty ID, handler catches and returns null
        var useCase = new EndGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
        var command = new EndGameSessionCommand("");

        // Act
        var result = await EndGameSessionCommandHandler.Handle(command, useCase, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryUpdateAndSaveChanges()
    {
        // Arrange
        var sessionId = "session-456";
        var session = new GameSession { Id = sessionId, Status = SessionStatus.InProgress, StartTime = DateTime.UtcNow.AddMinutes(-10) };

        _repository.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var useCase = new EndGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
        var command = new EndGameSessionCommand(sessionId);

        // Act
        await EndGameSessionCommandHandler.Handle(command, useCase, CancellationToken.None);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.Is<GameSession>(s => s.Id == sessionId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var session = new GameSession { Id = "ct-session", Status = SessionStatus.InProgress, StartTime = DateTime.UtcNow };

        _repository.Setup(r => r.GetByIdAsync("ct-session", ct))
            .ReturnsAsync(session);

        var useCase = new EndGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
        var command = new EndGameSessionCommand("ct-session");

        // Act
        await EndGameSessionCommandHandler.Handle(command, useCase, ct);

        // Assert - verify exact token was passed through UseCase to repository calls
        _repository.Verify(r => r.GetByIdAsync("ct-session", ct), Times.Once);
        _repository.Verify(r => r.UpdateAsync(session, ct), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
