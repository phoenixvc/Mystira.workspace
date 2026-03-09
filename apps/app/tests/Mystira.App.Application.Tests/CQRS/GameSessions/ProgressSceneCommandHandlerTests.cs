using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class ProgressSceneCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public ProgressSceneCommandHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_UpdatesCurrentScene()
    {
        var session = new GameSession { Id = "session-1", Status = SessionStatus.InProgress };
        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-2" };
        var command = new ProgressSceneCommand(request);

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await ProgressSceneCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.CurrentSceneId.Should().Be("scene-2");
        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SessionNotFound_ReturnsNull()
    {
        var request = new ProgressSceneRequest { SessionId = "missing", SceneId = "scene-1" };
        var command = new ProgressSceneCommand(request);

        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var result = await ProgressSceneCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SessionNotInProgress_ThrowsInvalidOperation()
    {
        var session = new GameSession { Id = "session-1", Status = SessionStatus.Completed };
        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-2" };
        var command = new ProgressSceneCommand(request);

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var act = () => ProgressSceneCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("", "scene-1")]
    [InlineData("session-1", "")]
    public async Task Handle_MissingRequiredFields_ThrowsArgumentException(string sessionId, string sceneId)
    {
        var request = new ProgressSceneRequest { SessionId = sessionId, SceneId = sceneId };
        var command = new ProgressSceneCommand(request);

        var act = () => ProgressSceneCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var session = new GameSession { Id = "session-1", Status = SessionStatus.InProgress };
        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-2" };
        var command = new ProgressSceneCommand(request);

        _repository.Setup(r => r.GetByIdAsync("session-1", ct)).ReturnsAsync(session);

        await ProgressSceneCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, ct);

        _repository.Verify(r => r.GetByIdAsync("session-1", ct), Times.Once);
        _repository.Verify(r => r.UpdateAsync(session, ct), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
