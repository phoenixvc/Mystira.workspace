using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.GameSessions;

public class GetGameSessionsByAccountUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger<GetGameSessionsByAccountUseCase>> _logger;
    private readonly GetGameSessionsByAccountUseCase _useCase;

    public GetGameSessionsByAccountUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger<GetGameSessionsByAccountUseCase>>();
        _useCase = new GetGameSessionsByAccountUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingAccount_ReturnsSessions()
    {
        var sessions = new List<GameSession>
        {
            new() { Id = "gs-1", AccountId = "acc-1", Status = SessionStatus.Completed },
            new() { Id = "gs-2", AccountId = "acc-1", Status = SessionStatus.InProgress }
        };
        _repository.Setup(r => r.GetByAccountIdAsync("acc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var result = await _useCase.ExecuteAsync("acc-1");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoSessions_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetByAccountIdAsync("acc-1", It.IsAny<CancellationToken>()))
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
