using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Queries;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class GetAchievementsQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetAchievementsQueryHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidSession_ReturnsAchievements()
    {
        var achievements = new List<SessionAchievement>
        {
            new() { Title = "First Steps", Description = "Complete your first scene" },
            new() { Title = "Explorer", Description = "Visit all scenes" }
        };
        var session = new GameSession { Id = "session-1", Achievements = achievements };
        var query = new GetAchievementsQuery("session-1");

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await GetAchievementsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("First Steps");
        result[1].Title.Should().Be("Explorer");
    }

    [Fact]
    public async Task Handle_SessionNotFound_ReturnsEmptyList()
    {
        var query = new GetAchievementsQuery("missing");

        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var result = await GetAchievementsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SessionWithNullAchievements_ReturnsEmptyList()
    {
        var session = new GameSession { Id = "session-1", Achievements = null! };
        var query = new GetAchievementsQuery("session-1");

        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await GetAchievementsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var session = new GameSession { Id = "session-1" };
        var query = new GetAchievementsQuery("session-1");

        _repository.Setup(r => r.GetByIdAsync("session-1", ct)).ReturnsAsync(session);

        await GetAchievementsQueryHandler.Handle(
            query, _repository.Object, _logger.Object, ct);

        _repository.Verify(r => r.GetByIdAsync("session-1", ct), Times.Once);
    }
}
