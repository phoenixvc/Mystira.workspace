using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Media;

public class GetMediaUseCaseTests
{
    private readonly Mock<IMediaAssetRepository> _repository;
    private readonly Mock<ILogger<GetMediaUseCase>> _logger;
    private readonly GetMediaUseCase _useCase;

    public GetMediaUseCaseTests()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _logger = new Mock<ILogger<GetMediaUseCase>>();
        _useCase = new GetMediaUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingMediaId_ReturnsMediaAsset()
    {
        // Arrange
        var expected = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Url = "https://storage.example.com/image.jpg",
            MediaType = "image",
            MimeType = "image/jpeg",
            FileSizeBytes = 1024
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _useCase.ExecuteAsync("media-1");

        // Assert
        result.Should().NotBeNull();
        result!.MediaId.Should().Be("media-1");
        result.MediaType.Should().Be("image");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _repository.Setup(r => r.GetByMediaIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaAsset));

        // Act
        var result = await _useCase.ExecuteAsync("missing");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? mediaId)
    {
        var act = () => _useCase.ExecuteAsync(mediaId!);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
