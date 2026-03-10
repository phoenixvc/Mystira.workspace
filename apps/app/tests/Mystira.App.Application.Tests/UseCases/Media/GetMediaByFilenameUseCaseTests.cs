using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.Media;

public class GetMediaByFilenameUseCaseTests
{
    private readonly Mock<IMediaAssetRepository> _repository;
    private readonly Mock<IMediaMetadataService> _mediaMetadataService;
    private readonly Mock<ILogger<GetMediaByFilenameUseCase>> _logger;
    private readonly GetMediaByFilenameUseCase _useCase;

    public GetMediaByFilenameUseCaseTests()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _mediaMetadataService = new Mock<IMediaMetadataService>();
        _logger = new Mock<ILogger<GetMediaByFilenameUseCase>>();
        _useCase = new GetMediaByFilenameUseCase(_repository.Object, _mediaMetadataService.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithMatchingFilename_ReturnsMediaAsset()
    {
        // Arrange
        var metadataFile = new MediaMetadataFile
        {
            Entries = new List<MediaMetadataEntry>
            {
                new() { Id = "media-1", FileName = "elarion.jpg" },
                new() { Id = "media-2", FileName = "goblin.png" }
            }
        };
        _mediaMetadataService.Setup(s => s.GetMediaMetadataFileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadataFile);

        var expected = new MediaAsset { Id = "id-1", MediaId = "media-1", Url = "https://storage/elarion.jpg" };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _useCase.ExecuteAsync("elarion.jpg");

        // Assert
        result.Should().NotBeNull();
        result!.MediaId.Should().Be("media-1");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMetadataFile_ReturnsNull()
    {
        // Arrange
        _mediaMetadataService.Setup(s => s.GetMediaMetadataFileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaMetadataFile));

        // Act
        var result = await _useCase.ExecuteAsync("test.jpg");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithNonMatchingFilename_ReturnsNull()
    {
        // Arrange
        var metadataFile = new MediaMetadataFile
        {
            Entries = new List<MediaMetadataEntry>
            {
                new() { Id = "media-1", FileName = "other.jpg" }
            }
        };
        _mediaMetadataService.Setup(s => s.GetMediaMetadataFileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadataFile);

        // Act
        var result = await _useCase.ExecuteAsync("missing.jpg");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyFilename_ThrowsValidationException(string? fileName)
    {
        var act = () => _useCase.ExecuteAsync(fileName!);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
