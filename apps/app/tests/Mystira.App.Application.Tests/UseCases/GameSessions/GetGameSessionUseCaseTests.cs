using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.GameSessions;

public class GetGameSessionUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger<GetGameSessionUseCase>> _logger;
    private readonly GetGameSessionUseCase _useCase;

    public GetGameSessionUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger<GetGameSessionUseCase>>();
        _useCase = new GetGameSessionUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingSession_ReturnsSession()
    {
        var session = new GameSession
        {
            Id = "session-1",
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            Status = SessionStatus.InProgress
        };
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _useCase.ExecuteAsync("session-1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("session-1");
        result.ScenarioId.Should().Be("scenario-1");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeNull();
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
    public async Task ExecuteAsync_CallsRepositoryOnce()
    {
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameSession { Id = "session-1" });

        await _useCase.ExecuteAsync("session-1");

        _repository.Verify(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
