using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.ContentBundles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.ContentBundles;

public class GetContentBundlesUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<ILogger<GetContentBundlesUseCase>> _logger;
    private readonly GetContentBundlesUseCase _useCase;

    public GetContentBundlesUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _logger = new Mock<ILogger<GetContentBundlesUseCase>>();
        _useCase = new GetContentBundlesUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBundles()
    {
        var bundles = new List<ContentBundle>
        {
            new() { Id = "b1", Title = "Bundle 1" },
            new() { Id = "b2", Title = "Bundle 2" }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundles);

        var result = await _useCase.ExecuteAsync();

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Bundle 1");
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmpty_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentBundle>());

        var result = await _useCase.ExecuteAsync();

        result.Should().BeEmpty();
    }
}
