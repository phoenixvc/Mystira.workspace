using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Ports.Storage;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.Media;

namespace Mystira.App.Application.Tests.UseCases.Media;

public class UploadMediaUseCaseTests
{
    private readonly Mock<IMediaAssetRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IBlobService> _blobService;
    private readonly Mock<IMediaMetadataService> _mediaMetadataService;
    private readonly Mock<ILogger<UploadMediaUseCase>> _logger;
    private readonly UploadMediaUseCase _useCase;

    public UploadMediaUseCaseTests()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _blobService = new Mock<IBlobService>();
        _mediaMetadataService = new Mock<IMediaMetadataService>();
        _logger = new Mock<ILogger<UploadMediaUseCase>>();
        _useCase = new UploadMediaUseCase(
            _repository.Object, _unitOfWork.Object,
            _blobService.Object, _mediaMetadataService.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentException()
    {
        // Act
        var act = () => _useCase.ExecuteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullFileStream_ThrowsArgumentException()
    {
        // Arrange
        var request = new UploadMediaRequest
        {
            FileStream = null,
            FileName = "test.jpg",
            MediaType = "image",
            FileSizeBytes = 1024
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroFileSize_ThrowsArgumentException()
    {
        // Arrange
        var request = new UploadMediaRequest
        {
            FileStream = new MemoryStream(new byte[] { 1 }),
            FileName = "test.jpg",
            MediaType = "image",
            FileSizeBytes = 0
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingMediaId_ThrowsInvalidOperationException()
    {
        // Arrange
        var metadataFile = new MediaMetadataFile
        {
            Entries = new List<MediaMetadataEntry>
            {
                new() { Id = "media-1", FileName = "test.jpg" }
            }
        };
        _mediaMetadataService.Setup(s => s.GetMediaMetadataFileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadataFile);

        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MediaAsset { MediaId = "media-1" });

        var request = new UploadMediaRequest
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "test.jpg",
            MediaType = "image",
            ContentType = "image/jpeg",
            FileSizeBytes = 3
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMetadataFile_ThrowsInvalidOperationException()
    {
        // Arrange
        _mediaMetadataService.Setup(s => s.GetMediaMetadataFileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaMetadataFile));

        var request = new UploadMediaRequest
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "test.jpg",
            MediaType = "image",
            ContentType = "image/jpeg",
            FileSizeBytes = 3
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*metadata*");
    }
}
