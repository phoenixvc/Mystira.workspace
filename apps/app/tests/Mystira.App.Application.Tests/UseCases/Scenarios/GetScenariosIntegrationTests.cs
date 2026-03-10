using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Tests.TestUtilities;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.Scenarios;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

/// <summary>
/// Tests for GetScenariosUseCase using mock repository with async queryable support.
/// Uses TestAsyncEnumerable to avoid EF InMemory provider limitations with value object projections.
/// </summary>
public class GetScenariosIntegrationTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly GetScenariosUseCase _useCase;

    public GetScenariosIntegrationTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _useCase = new GetScenariosUseCase(_repository.Object, new Mock<ILogger<GetScenariosUseCase>>().Object);
    }

    private void SetupScenarios(params Scenario[] scenarios)
    {
        var queryable = new TestAsyncEnumerable<Scenario>(scenarios.AsEnumerable());
        _repository.Setup(r => r.GetQueryable()).Returns(queryable);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoFilters_ReturnsAllScenarios()
    {
        SetupScenarios(
            CreateScenario("s0", "Scenario 0", "6-9", 6),
            CreateScenario("s1", "Scenario 1", "6-9", 6),
            CreateScenario("s2", "Scenario 2", "6-9", 6));
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10 };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Scenarios.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsCorrectPage()
    {
        SetupScenarios(
            CreateScenario("s0", "Scenario 0", "6-9", 6),
            CreateScenario("s1", "Scenario 1", "6-9", 6),
            CreateScenario("s2", "Scenario 2", "6-9", 6),
            CreateScenario("s3", "Scenario 3", "6-9", 6),
            CreateScenario("s4", "Scenario 4", "6-9", 6));
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 2 };

        var result = await _useCase.ExecuteAsync(request);

        result.TotalCount.Should().Be(5);
        result.Scenarios.Should().HaveCount(2);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithAgeGroupFilter_ReturnsFiltered()
    {
        SetupScenarios(
            CreateScenario("s1", "Kids Quest", "6-9", 6),
            CreateScenario("s2", "Teen Adventure", "10-13", 10),
            CreateScenario("s3", "Little Story", "6-9", 6));
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10, AgeGroup = "6-9" };

        var result = await _useCase.ExecuteAsync(request);

        result.TotalCount.Should().Be(2);
        result.Scenarios.Should().OnlyContain(s => s.AgeGroup == "6-9");
    }

    [Fact]
    public async Task ExecuteAsync_WithMinimumAgeFilter_ReturnsApplicable()
    {
        SetupScenarios(
            CreateScenario("s1", "Young Kids", "3-5", 3),
            CreateScenario("s2", "School Age", "6-9", 6),
            CreateScenario("s3", "Teen", "10-13", 10));
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10, MinimumAge = 7 };

        var result = await _useCase.ExecuteAsync(request);

        result.TotalCount.Should().Be(2);
        result.Scenarios.Should().OnlyContain(s => s.MinimumAge <= 7);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatches_ReturnsEmptyResponse()
    {
        SetupScenarios(
            CreateScenario("s0", "Scenario 0", "6-9", 6),
            CreateScenario("s1", "Scenario 1", "6-9", 6));
        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10, AgeGroup = "nonexistent" };

        var result = await _useCase.ExecuteAsync(request);

        result.TotalCount.Should().Be(0);
        result.Scenarios.Should().BeEmpty();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_OrdersByCreatedAtDescending()
    {
        var older = CreateScenario("s1", "Older", "6-9", 6);
        older.CreatedAt = DateTime.UtcNow.AddDays(-2);
        var newer = CreateScenario("s2", "Newer", "6-9", 6);
        newer.CreatedAt = DateTime.UtcNow.AddDays(-1);
        SetupScenarios(older, newer);

        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10 };

        var result = await _useCase.ExecuteAsync(request);

        result.Scenarios.First().Title.Should().Be("Newer");
    }

    private static Scenario CreateScenario(string id, string title, string ageGroup, int minAge)
    {
        return new Scenario
        {
            Id = id,
            Title = title,
            Description = "Test scenario",
            AgeGroupId = ageGroup,
            MinimumAge = minAge,
            Tags = new List<string>(),
            Archetypes = new List<string>(),
            CoreAxes = new List<string>(),
            Characters = new List<ScenarioCharacter>(),
            Scenes = new List<Scene>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
