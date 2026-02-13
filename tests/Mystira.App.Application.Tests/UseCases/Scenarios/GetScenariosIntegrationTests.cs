using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.App.Domain.Models;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;
using Mystira.Contracts.App.Requests.Scenarios;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

/// <summary>
/// Integration tests for GetScenariosUseCase using EF Core InMemory provider
/// to properly support IQueryable with async LINQ operations.
/// </summary>
public class GetScenariosIntegrationTests : IDisposable
{
    private readonly MystiraAppDbContext _context;
    private readonly ScenarioRepository _repository;
    private readonly GetScenariosUseCase _useCase;

    public GetScenariosIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MystiraAppDbContext(options);
        _repository = new ScenarioRepository(_context);
        _useCase = new GetScenariosUseCase(_repository, new Mock<ILogger<GetScenariosUseCase>>().Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task ExecuteAsync_WithNoFilters_ReturnsAllScenarios()
    {
        // Arrange
        await SeedScenarios(3);
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Scenarios.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedScenarios(5);
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 2 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Scenarios.Should().HaveCount(2);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithAgeGroupFilter_ReturnsFiltered()
    {
        // Arrange
        await _context.Scenarios.AddRangeAsync(
            CreateScenario("s1", "Kids Quest", "6-9", 6),
            CreateScenario("s2", "Teen Adventure", "10-13", 10),
            CreateScenario("s3", "Little Story", "6-9", 6));
        await _context.SaveChangesAsync();

        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10, AgeGroup = "6-9" };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Scenarios.Should().OnlyContain(s => s.AgeGroup == "6-9");
    }

    [Fact]
    public async Task ExecuteAsync_WithMinimumAgeFilter_ReturnsApplicable()
    {
        // Arrange
        await _context.Scenarios.AddRangeAsync(
            CreateScenario("s1", "Young Kids", "3-5", 3),
            CreateScenario("s2", "School Age", "6-9", 6),
            CreateScenario("s3", "Teen", "10-13", 10));
        await _context.SaveChangesAsync();

        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10, MinimumAge = 7 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Scenarios.Should().OnlyContain(s => s.MinimumAge <= 7);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatches_ReturnsEmptyResponse()
    {
        // Arrange
        await SeedScenarios(2);
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10, AgeGroup = "nonexistent" };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Scenarios.Should().BeEmpty();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var older = CreateScenario("s1", "Older", "6-9", 6);
        older.CreatedAt = DateTime.UtcNow.AddDays(-2);
        var newer = CreateScenario("s2", "Newer", "6-9", 6);
        newer.CreatedAt = DateTime.UtcNow.AddDays(-1);
        await _context.Scenarios.AddRangeAsync(older, newer);
        await _context.SaveChangesAsync();

        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Scenarios.First().Title.Should().Be("Newer");
    }

    private async Task SeedScenarios(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await _context.Scenarios.AddAsync(
                CreateScenario($"s{i}", $"Scenario {i}", "6-9", 6));
        }
        await _context.SaveChangesAsync();
    }

    private static Scenario CreateScenario(string id, string title, string ageGroup, int minAge)
    {
        return new Scenario
        {
            Id = id,
            Title = title,
            Description = "Test scenario",
            AgeGroup = ageGroup,
            MinimumAge = minAge,
            Tags = new List<string>(),
            Archetypes = new List<Archetype>(),
            CoreAxes = new List<CoreAxis>(),
            Characters = new List<ScenarioCharacter>(),
            Scenes = new List<Scene>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
