using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;

namespace Mystira.App.Infrastructure.Data.Tests.Repositories;

public class GameSessionRepositoryTests : IDisposable
{
    private readonly MystiraAppDbContext _context;
    private readonly GameSessionRepository _repository;

    public GameSessionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MystiraAppDbContext(options);
        _repository = new GameSessionRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsSessionsForAccount()
    {
        // Arrange
        var session1 = CreateSession("s1", "account-1", "profile-1", SessionStatus.InProgress);
        var session2 = CreateSession("s2", "account-1", "profile-2", SessionStatus.Completed);
        var session3 = CreateSession("s3", "account-2", "profile-3", SessionStatus.InProgress);
        await _context.GameSessions.AddRangeAsync(session1, session2, session3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountIdAsync("account-1")).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.AccountId == "account-1");
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsOrderedByStartTimeDescending()
    {
        // Arrange
        var older = CreateSession("s1", "account-1", "p1", SessionStatus.InProgress);
        older.StartTime = DateTime.UtcNow.AddHours(-2);
        var newer = CreateSession("s2", "account-1", "p1", SessionStatus.InProgress);
        newer.StartTime = DateTime.UtcNow.AddHours(-1);
        await _context.GameSessions.AddRangeAsync(older, newer);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountIdAsync("account-1")).ToList();

        // Assert
        result.First().Id.Should().Be("s2");
        result.Last().Id.Should().Be("s1");
    }

    [Fact]
    public async Task GetByProfileIdAsync_ReturnsSessionsForProfile()
    {
        // Arrange
        var session = CreateSession("s1", "account-1", "profile-1", SessionStatus.InProgress);
        await _context.GameSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByProfileIdAsync("profile-1")).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().ProfileId.Should().Be("profile-1");
    }

    [Fact]
    public async Task GetInProgressSessionsAsync_ReturnsOnlyActiveAndPaused()
    {
        // Arrange
        var inProgress = CreateSession("s1", "account-1", "p1", SessionStatus.InProgress);
        var paused = CreateSession("s2", "account-1", "p1", SessionStatus.Paused);
        var completed = CreateSession("s3", "account-1", "p1", SessionStatus.Completed);
        var abandoned = CreateSession("s4", "account-1", "p1", SessionStatus.Abandoned);
        await _context.GameSessions.AddRangeAsync(inProgress, paused, completed, abandoned);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetInProgressSessionsAsync("account-1")).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s =>
            s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused);
    }

    [Fact]
    public async Task GetActiveSessionForScenarioAsync_ReturnsActiveSession()
    {
        // Arrange
        var active = CreateSession("s1", "account-1", "p1", SessionStatus.InProgress);
        active.ScenarioId = "scenario-1";
        var completed = CreateSession("s2", "account-1", "p1", SessionStatus.Completed);
        completed.ScenarioId = "scenario-1";
        await _context.GameSessions.AddRangeAsync(active, completed);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionForScenarioAsync("account-1", "scenario-1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("s1");
    }

    [Fact]
    public async Task GetActiveSessionForScenarioAsync_ReturnsNull_WhenNoActiveSession()
    {
        // Arrange
        var completed = CreateSession("s1", "account-1", "p1", SessionStatus.Completed);
        completed.ScenarioId = "scenario-1";
        await _context.GameSessions.AddAsync(completed);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionForScenarioAsync("account-1", "scenario-1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSessionsCountAsync_CountsActiveAndPausedSessions()
    {
        // Arrange
        await _context.GameSessions.AddRangeAsync(
            CreateSession("s1", "a1", "p1", SessionStatus.InProgress),
            CreateSession("s2", "a2", "p2", SessionStatus.Paused),
            CreateSession("s3", "a3", "p3", SessionStatus.Completed));
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetActiveSessionsCountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetByAccountIdAsync_WithNoSessions_ReturnsEmpty()
    {
        var result = await _repository.GetByAccountIdAsync("nonexistent");
        result.Should().BeEmpty();
    }

    private static GameSession CreateSession(string id, string accountId, string profileId, SessionStatus status)
    {
        return new GameSession
        {
            Id = id,
            AccountId = accountId,
            ProfileId = profileId,
            ScenarioId = "default-scenario",
            Status = status,
            StartTime = DateTime.UtcNow,
            CurrentSceneId = "scene-1"
        };
    }
}
