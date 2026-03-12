using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases;

public class ResumeGameSessionUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<ResumeGameSessionUseCase>> _logger;
    private readonly ResumeGameSessionUseCase _useCase;

    public ResumeGameSessionUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<ResumeGameSessionUseCase>>();
        _useCase = new ResumeGameSessionUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithPausedSession_ResumesSuccessfully()
    {
        var session = new GameSession
        {
            Id = "session-1",
            Status = SessionStatus.Paused,
            IsPaused = true,
            PausedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.Status.Should().Be(SessionStatus.InProgress);
        result.IsPaused.Should().BeFalse();
        result.PausedAt.Should().BeNull();

        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ThrowsValidationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var act = () => _useCase.ExecuteAsync("missing");

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WithInProgressSession_ThrowsInvalidOperationException()
    {
        var session = new GameSession { Id = "session-1", Status = SessionStatus.InProgress };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var act = () => _useCase.ExecuteAsync("session-1");

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*only resume paused sessions*");
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedSession_ThrowsInvalidOperationException()
    {
        var session = new GameSession { Id = "session-1", Status = SessionStatus.Completed };

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var act = () => _useCase.ExecuteAsync("session-1");

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*only resume paused sessions*");
    }
}
