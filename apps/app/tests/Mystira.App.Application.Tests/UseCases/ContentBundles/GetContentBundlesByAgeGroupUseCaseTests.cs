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

public class GetContentBundlesByAgeGroupUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<ILogger<GetContentBundlesByAgeGroupUseCase>> _logger;
    private readonly GetContentBundlesByAgeGroupUseCase _useCase;

    public GetContentBundlesByAgeGroupUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _logger = new Mock<ILogger<GetContentBundlesByAgeGroupUseCase>>();
        _useCase = new GetContentBundlesByAgeGroupUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingAgeGroup_ReturnsBundles()
    {
        var bundles = new List<ContentBundle>
        {
            new() { Id = "b1", Title = "Kids Bundle", AgeGroupId = "middle_childhood" }
        };
        _repository.Setup(r => r.GetByAgeGroupAsync("middle_childhood", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundles);

        var result = await _useCase.ExecuteAsync("middle_childhood");

        result.Should().HaveCount(1);
        result[0].AgeGroupId.Should().Be("middle_childhood");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatchingAgeGroup_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetByAgeGroupAsync("99-100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentBundle>());

        var result = await _useCase.ExecuteAsync("99-100");

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyAgeGroup_ThrowsValidationException(string? ageGroup)
    {
        var act = () => _useCase.ExecuteAsync(ageGroup!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
