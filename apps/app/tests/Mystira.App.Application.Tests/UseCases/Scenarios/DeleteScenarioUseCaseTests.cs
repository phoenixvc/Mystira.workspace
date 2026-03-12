using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

public class DeleteScenarioUseCaseTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DeleteScenarioUseCase>> _logger;
    private readonly DeleteScenarioUseCase _useCase;

    public DeleteScenarioUseCaseTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DeleteScenarioUseCase>>();
        _useCase = new DeleteScenarioUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsTrue()
    {
        var scenario = new Scenario { Id = "scen-1", Title = "To Delete" };
        _repository.Setup(r => r.GetByIdAsync("scen-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var result = await _useCase.ExecuteAsync("scen-1");

        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("scen-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ReturnsFalse(string? scenarioId)
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var result = await _useCase.ExecuteAsync(scenarioId!);

        result.Should().BeFalse();
    }
}
