using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.Scenarios;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

public class UpdateScenarioUseCaseTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateScenarioUseCase>> _logger;
    private readonly Mock<IValidateScenarioUseCase> _validateScenarioUseCase;
    private readonly UpdateScenarioUseCase _useCase;

    public UpdateScenarioUseCaseTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateScenarioUseCase>>();
        _validateScenarioUseCase = new Mock<IValidateScenarioUseCase>();
        _useCase = new UpdateScenarioUseCase(
            _repository.Object, _unitOfWork.Object,
            _logger.Object, _validateScenarioUseCase.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingScenario_ReturnsNull()
    {
        // Arrange - GetByIdAsync returns null BEFORE schema validation runs
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        // Act
        var result = await _useCase.ExecuteAsync("missing", new CreateScenarioRequest());

        // Assert
        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// When scenario exists, the static ScenarioSchemaValidator.ValidateAgainstSchema runs
    /// and will reject an empty request with a ValidationException.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithExistingScenarioAndInvalidRequest_ThrowsFromSchemaValidation()
    {
        // Arrange
        var existing = new Scenario { Id = "scenario-1", Title = "Test Scenario" };
        _repository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act - empty request fails schema validation
        var act = () => _useCase.ExecuteAsync("scenario-1", new CreateScenarioRequest());

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_AndExistingScenario_ThrowsException()
    {
        // Arrange
        var existing = new Scenario { Id = "scenario-1", Title = "Test Scenario" };
        _repository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var act = () => _useCase.ExecuteAsync("scenario-1", null!);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
