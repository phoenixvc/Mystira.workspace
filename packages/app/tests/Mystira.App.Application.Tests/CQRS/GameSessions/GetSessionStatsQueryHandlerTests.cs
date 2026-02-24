using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class GetSessionStatsQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetSessionStatsQueryHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidSession_ReturnsStats()
    {
        var session = new GameSession
        {
            Id = "session-1",
            ChoiceHistory = new List<SessionChoice>
            {
                new() { SceneId = "s1" },
                new() { SceneId = "s2" }
            },
            Achievements = new List<SessionAchievement>
            {
                new() { Title = "First Steps" }
            }
        };
        var query = new GetSessionStatsQuery("session-1");

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await GetSessionStatsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalChoices.Should().Be(2);
        result.Achievements.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_SessionNotFound_ReturnsNull()
    {
        var query = new GetSessionStatsQuery("missing");

        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var result = await GetSessionStatsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SessionWithNoHistory_ReturnsZeroChoices()
    {
        var session = new GameSession
        {
            Id = "session-1",
            ChoiceHistory = null!,
            EchoHistory = null!,
            CompassValues = null!,
            Achievements = null!
        };
        var query = new GetSessionStatsQuery("session-1");

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await GetSessionStatsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalChoices.Should().Be(0);
        result.CompassValues.Should().BeEmpty();
        result.RecentEchoes.Should().BeEmpty();
        result.Achievements.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var session = new GameSession { Id = "session-1" };
        var query = new GetSessionStatsQuery("session-1");

        _repository.Setup(r => r.GetByIdAsync("session-1", ct)).ReturnsAsync(session);

        await GetSessionStatsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, ct);

        _repository.Verify(r => r.GetByIdAsync("session-1", ct), Times.Once);
    }
}
