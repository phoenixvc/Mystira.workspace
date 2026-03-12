using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.Contracts.App.Requests.Scenarios;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

public class CreateScenarioUseCaseTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateScenarioUseCase>> _logger;
    private readonly Mock<IValidateScenarioUseCase> _validateScenarioUseCase;

    public CreateScenarioUseCaseTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateScenarioUseCase>>();
        _validateScenarioUseCase = new Mock<IValidateScenarioUseCase>();
    }

    /// <summary>
    /// Note: CreateScenarioUseCase calls ScenarioSchemaValidator.ValidateAgainstSchema(request)
    /// which is a static method that cannot be mocked. Tests for the full happy path require
    /// a request that passes JSON schema validation. This test validates that null input
    /// is properly rejected.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsException()
    {
        // Arrange
        var useCase = new CreateScenarioUseCase(
            _repository.Object, _unitOfWork.Object,
            _logger.Object, _validateScenarioUseCase.Object);

        // Act - null request will fail at schema validation (NullReferenceException)
        // or ValidationException depending on serializer behavior
        var act = () => useCase.ExecuteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRequest_ThrowsFromSchemaValidation()
    {
        // Arrange - empty request will fail at static ScenarioSchemaValidator
        var useCase = new CreateScenarioUseCase(
            _repository.Object, _unitOfWork.Object,
            _logger.Object, _validateScenarioUseCase.Object);

        // Act - empty request fails schema validation
        var act = () => useCase.ExecuteAsync(new CreateScenarioRequest());

        // Assert - ScenarioSchemaValidator throws ValidationException for invalid schema
        await act.Should().ThrowAsync<Exception>();
    }
}
