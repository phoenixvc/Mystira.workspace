using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.ContentBundles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class RemoveScenarioFromBundleUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<RemoveScenarioFromBundleUseCase>> _logger;
    private readonly RemoveScenarioFromBundleUseCase _useCase;

    public RemoveScenarioFromBundleUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<RemoveScenarioFromBundleUseCase>>();
        _useCase = new RemoveScenarioFromBundleUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingScenario_RemovesFromBundle()
    {
        var bundle = new ContentBundle
        {
            Id = "b1",
            ScenarioIds = new List<string> { "scen-1", "scen-2", "scen-3" }
        };
        _repository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("b1", "scen-2");

        result.ScenarioIds.Should().NotContain("scen-2");
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
