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

public class GetContentBundleUseCaseTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<ILogger<GetContentBundleUseCase>> _logger;
    private readonly GetContentBundleUseCase _useCase;

    public GetContentBundleUseCaseTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _logger = new Mock<ILogger<GetContentBundleUseCase>>();
        _useCase = new GetContentBundleUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsBundle()
    {
        var bundle = new ContentBundle { Id = "bundle-1", Title = "Test Bundle" };
        _repository.Setup(r => r.GetByIdAsync("bundle-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var result = await _useCase.ExecuteAsync("bundle-1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("bundle-1");
        result.Title.Should().Be("Test Bundle");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? bundleId)
    {
        var act = () => _useCase.ExecuteAsync(bundleId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
