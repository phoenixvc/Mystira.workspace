using Mystira.App.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports;
using Mystira.Core.Ports.Data;
using Mystira.Core.Ports.Storage;
using Mystira.App.Application.UseCases.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
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
    public async Task ExecuteAsync_WithNullRequest_ThrowsValidationException()
    {
        // Act
        var act = () => _useCase.ExecuteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullFileStream_ThrowsValidationException()
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
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroFileSize_ThrowsValidationException()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1 });
        var request = new UploadMediaRequest
        {
            FileStream = stream,
            FileName = "test.jpg",
            MediaType = "image",
            FileSizeBytes = 0
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
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

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadMediaRequest
        {
            FileStream = stream,
            FileName = "test.jpg",
            MediaType = "image",
            ContentType = "image/jpeg",
            FileSizeBytes = 3
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMetadataFile_ThrowsInvalidOperationException()
    {
        // Arrange
        _mediaMetadataService.Setup(s => s.GetMediaMetadataFileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaMetadataFile));

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadMediaRequest
        {
            FileStream = stream,
            FileName = "test.jpg",
            MediaType = "image",
            ContentType = "image/jpeg",
            FileSizeBytes = 3
        };

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*metadata*");
    }
}
