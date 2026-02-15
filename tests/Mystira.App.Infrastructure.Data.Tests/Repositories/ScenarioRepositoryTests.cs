using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Mystira.App.Domain.Models;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;

namespace Mystira.App.Infrastructure.Data.Tests.Repositories;

public class ScenarioRepositoryTests : IDisposable
{
    private readonly MystiraAppDbContext _context;
    private readonly ScenarioRepository _repository;

    public ScenarioRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MystiraAppDbContext(options);
        _repository = new ScenarioRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetByAgeGroupAsync_ReturnsMatchingScenarios()
    {
        // Arrange
        var scenario1 = CreateScenario("s1", "Adventure 1", "6-9");
        var scenario2 = CreateScenario("s2", "Adventure 2", "6-9");
        var scenario3 = CreateScenario("s3", "Teen Quest", "10-13");
        await _context.Scenarios.AddRangeAsync(scenario1, scenario2, scenario3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAgeGroupAsync("6-9")).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.AgeGroup == "6-9");
    }

    [Fact]
    public async Task GetByTitleAsync_WithExistingTitle_ReturnsScenario()
    {
        // Arrange
        var scenario = CreateScenario("s1", "The Lost Forest", "6-9");
        await _context.Scenarios.AddAsync(scenario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTitleAsync("The Lost Forest");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("The Lost Forest");
    }

    [Fact]
    public async Task GetByTitleAsync_WithNonExistingTitle_ReturnsNull()
    {
        var result = await _repository.GetByTitleAsync("Nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByTitleAsync_WithExistingTitle_ReturnsTrue()
    {
        // Arrange
        var scenario = CreateScenario("s1", "Existing Scenario", "6-9");
        await _context.Scenarios.AddAsync(scenario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByTitleAsync("Existing Scenario");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByTitleAsync_WithNonExistingTitle_ReturnsFalse()
    {
        var result = await _repository.ExistsByTitleAsync("Nonexistent");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetQueryable_ReturnsQueryableInterface()
    {
        // Arrange
        var scenario = CreateScenario("s1", "Test", "6-9");
        await _context.Scenarios.AddAsync(scenario);
        await _context.SaveChangesAsync();

        // Act
        var queryable = _repository.GetQueryable();

        // Assert
        queryable.Should().NotBeNull();
        var result = await queryable.ToListAsync();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CountAsync_WithNoPredicate_ReturnsTotal()
    {
        // Arrange
        await _context.Scenarios.AddRangeAsync(
            CreateScenario("s1", "A", "6-9"),
            CreateScenario("s2", "B", "6-9"),
            CreateScenario("s3", "C", "10-13"));
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsFilteredCount()
    {
        // Arrange
        await _context.Scenarios.AddRangeAsync(
            CreateScenario("s1", "A", "6-9"),
            CreateScenario("s2", "B", "10-13"));
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync(s => s.AgeGroup == "6-9");

        // Assert
        count.Should().Be(1);
    }

    private static Scenario CreateScenario(string id, string title, string ageGroup)
    {
        return new Scenario
        {
            Id = id,
            Title = title,
            Description = "Test scenario",
            AgeGroup = ageGroup,
            MinimumAge = 6,
            Tags = new List<string>(),
            Archetypes = new List<Archetype>(),
            CoreAxes = new List<CoreAxis>(),
            Characters = new List<ScenarioCharacter>(),
            Scenes = new List<Scene>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
