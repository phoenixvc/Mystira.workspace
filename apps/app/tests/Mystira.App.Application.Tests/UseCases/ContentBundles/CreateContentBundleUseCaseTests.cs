using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.ContentBundles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class CreateContentBundleUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateContentBundleUseCase>> _logger;
    private readonly CreateContentBundleUseCase _useCase;

    public CreateContentBundleUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateContentBundleUseCase>>();
        _useCase = new CreateContentBundleUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CreatesBundle()
    {
        var prices = new List<BundlePrice> { new() { Value = 9.99m, Currency = "USD" } };

        var result = await _useCase.ExecuteAsync(
            "Adventure Pack", "A pack of adventures",
            new List<string> { "scen-1", "scen-2" },
            "img-1", prices, false, "6-8");

        result.Should().NotBeNull();
        result.Title.Should().Be("Adventure Pack");
        result.Description.Should().Be("A pack of adventures");
        result.ScenarioIds.Should().HaveCount(2);
        result.ImageId.Should().Be("img-1");
        result.IsFree.Should().BeFalse();
        result.AgeGroupId.Should().Be("6-8");
        result.Id.Should().NotBeNullOrEmpty();

        _repository.Verify(r => r.AddAsync(It.IsAny<ContentBundle>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithFreeBundle_SetIsFreeTrue()
    {
        var result = await _useCase.ExecuteAsync(
            "Free Pack", "Free content",
            new List<string>(), "img-1", new List<BundlePrice>(), true, "3-5");

        result.IsFree.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTitle_ThrowsValidationException()
    {
        var act = () => _useCase.ExecuteAsync(
            "", "Description",
            new List<string>(), "img-1", new List<BundlePrice>(), true, "6-8");

        await act.Should().ThrowAsync<ValidationException>();
    }
}
