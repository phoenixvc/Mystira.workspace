using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

public class GetScenarioUseCaseTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<ILogger<GetScenarioUseCase>> _logger;
    private readonly GetScenarioUseCase _useCase;

    public GetScenarioUseCaseTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _logger = new Mock<ILogger<GetScenarioUseCase>>();
        _useCase = new GetScenarioUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsScenario()
    {
        var scenario = new Scenario { Id = "scen-1", Title = "Test Scenario" };
        _repository.Setup(r => r.GetByIdAsync("scen-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var result = await _useCase.ExecuteAsync("scen-1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("scen-1");
        result.Title.Should().Be("Test Scenario");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? scenarioId)
    {
        var act = () => _useCase.ExecuteAsync(scenarioId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryOnce()
    {
        _repository.Setup(r => r.GetByIdAsync("scen-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Scenario { Id = "scen-1" });

        await _useCase.ExecuteAsync("scen-1");

        _repository.Verify(r => r.GetByIdAsync("scen-1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
