using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.ContentBundles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class AddScenarioToBundleUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AddScenarioToBundleUseCase>> _logger;
    private readonly AddScenarioToBundleUseCase _useCase;

    public AddScenarioToBundleUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<AddScenarioToBundleUseCase>>();
        _useCase = new AddScenarioToBundleUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidIds_AddsScenarioToBundle()
    {
        var bundle = new ContentBundle { Id = "b1", ScenarioIds = new List<string> { "scen-1" } };
        _repository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("b1", "scen-2");

        result.ScenarioIds.Should().Contain("scen-2");
        result.ScenarioIds.Should().HaveCount(2);
        _repository.Verify(r => r.UpdateAsync(bundle, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingBundle_ThrowsValidationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var act = () => _useCase.ExecuteAsync("missing", "scen-1");

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*not found*");
    }

    [Theory]
    [InlineData(null, "scen-1")]
    [InlineData("", "scen-1")]
    [InlineData("b1", null)]
    [InlineData("b1", "")]
    public async Task ExecuteAsync_WithNullOrEmptyIds_ThrowsValidationException(string? bundleId, string? scenarioId)
    {
        var act = () => _useCase.ExecuteAsync(bundleId!, scenarioId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
