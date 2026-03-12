using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.ContentBundles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class UpdateContentBundleUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateContentBundleUseCase>> _logger;
    private readonly UpdateContentBundleUseCase _useCase;

    public UpdateContentBundleUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateContentBundleUseCase>>();
        _useCase = new UpdateContentBundleUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithTitle_UpdatesTitle()
    {
        var bundle = new ContentBundle { Id = "b1", Title = "Old Title" };
        _repository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("b1", title: "New Title");

        result.Title.Should().Be("New Title");
        _repository.Verify(r => r.UpdateAsync(bundle, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPartialUpdate_OnlyChangesProvidedFields()
    {
        var bundle = new ContentBundle
        {
            Id = "b1",
            Title = "Original",
            Description = "Original Desc",
            AgeGroupId = "middle_childhood",
            PriceCents = 999
        };
        _repository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("b1", description: "Updated Desc");

        result.Title.Should().Be("Original");
        result.Description.Should().Be("Updated Desc");
        result.AgeGroupId.Should().Be("middle_childhood");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingBundle_ThrowsValidationException()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var act = () => _useCase.ExecuteAsync("missing", title: "New Title");

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*not found*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyBundleId_ThrowsValidationException(string? bundleId)
    {
        var act = () => _useCase.ExecuteAsync(bundleId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithScenarioIds_UpdatesScenarioList()
    {
        var bundle = new ContentBundle { Id = "b1", ScenarioIds = new List<string> { "old-1" } };
        _repository.Setup(r => r.GetByIdAsync("b1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var newIds = new List<string> { "new-1", "new-2" };
        var result = await _useCase.ExecuteAsync("b1", scenarioIds: newIds);

        result.ScenarioIds.Should().BeEquivalentTo(newIds);
    }
}
