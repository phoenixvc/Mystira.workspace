using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Scenarios.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.Scenarios;

public class GetScenariosByAgeGroupQueryHandlerTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetScenariosByAgeGroupQueryHandlerTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithMatchingScenarios_ReturnsScenarios()
    {
        var scenarios = new List<Scenario>
        {
            new() { Id = "scen-1", Title = "Adventure", AgeGroup = "6-9" },
            new() { Id = "scen-2", Title = "Mystery", AgeGroup = "6-9" }
        };
        _repository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenarios);

        var result = await GetScenariosByAgeGroupQueryHandler.Handle(
            new GetScenariosByAgeGroupQuery("6-9"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatchingScenarios_ReturnsEmpty()
    {
        _repository.Setup(r => r.GetByAgeGroupAsync("99-100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Scenario>());

        var result = await GetScenariosByAgeGroupQueryHandler.Handle(
            new GetScenariosByAgeGroupQuery("99-100"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
