using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.GameSessions;

public class GetInProgressSessionsUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger<GetInProgressSessionsUseCase>> _logger;
    private readonly GetInProgressSessionsUseCase _useCase;

    public GetInProgressSessionsUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger<GetInProgressSessionsUseCase>>();
        _useCase = new GetInProgressSessionsUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithInProgressSessions_ReturnsSessions()
    {
        var sessions = new List<GameSession>
        {
            new() { Id = "gs-1", AccountId = "acc-1", Status = SessionStatus.InProgress }
        };
        _repository.Setup(r => r.GetInProgressSessionsAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var result = await _useCase.ExecuteAsync("acc-1");

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoInProgressSessions_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetInProgressSessionsAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        var result = await _useCase.ExecuteAsync("acc-1");

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyAccountId_ThrowsValidationException(string? accountId)
    {
        var act = () => _useCase.ExecuteAsync(accountId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
