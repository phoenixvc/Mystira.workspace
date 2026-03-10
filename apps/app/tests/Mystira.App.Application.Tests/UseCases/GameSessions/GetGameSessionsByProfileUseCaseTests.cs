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

public class GetGameSessionsByProfileUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger<GetGameSessionsByProfileUseCase>> _logger;
    private readonly GetGameSessionsByProfileUseCase _useCase;

    public GetGameSessionsByProfileUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger<GetGameSessionsByProfileUseCase>>();
        _useCase = new GetGameSessionsByProfileUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProfile_ReturnsSessions()
    {
        var sessions = new List<GameSession>
        {
            new() { Id = "gs-1", ProfileId = "p1" },
            new() { Id = "gs-2", ProfileId = "p1" }
        };
        _repository.Setup(r => r.GetByProfileIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var result = await _useCase.ExecuteAsync("p1");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoSessions_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetByProfileIdAsync("p1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());

        var result = await _useCase.ExecuteAsync("p1");

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyProfileId_ThrowsValidationException(string? profileId)
    {
        var act = () => _useCase.ExecuteAsync(profileId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
