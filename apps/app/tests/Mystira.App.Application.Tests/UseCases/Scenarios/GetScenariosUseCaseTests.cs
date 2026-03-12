using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.Scenarios;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

/// <summary>
/// GetScenariosUseCase uses IQueryable with EF Core async extensions (CountAsync, ToListAsync).
/// Full integration tests with InMemory DbContext are recommended for thorough coverage.
/// These tests verify constructor wiring and basic contract expectations.
/// </summary>
public class GetScenariosUseCaseTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<ILogger<GetScenariosUseCase>> _logger;
    private readonly GetScenariosUseCase _useCase;

    public GetScenariosUseCaseTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _logger = new Mock<ILogger<GetScenariosUseCase>>();
        _useCase = new GetScenariosUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        _useCase.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_CallsGetQueryable()
    {
        // Arrange - GetScenariosUseCase internally calls _repository.GetQueryable() then chains
        // EF Core async operations. Without a real DbContext, we verify the queryable is accessed.
        var scenarios = new List<Scenario>
        {
            new() { Id = "s-1", Title = "Scenario 1" },
            new() { Id = "s-2", Title = "Scenario 2" }
        }.AsQueryable();

        _repository.Setup(r => r.GetQueryable()).Returns(scenarios);

        var request = new ScenarioQueryRequest { Page = 1, PageSize = 10 };

        // Note: This will throw because IQueryable from List doesn't support EF Core async.
        var act = () => _useCase.ExecuteAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _repository.Verify(r => r.GetQueryable(), Times.Once);
    }
}
