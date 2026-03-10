using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.Application.Ports.Storage;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.Media;

public class DeleteMediaUseCaseTests
{
    private readonly Mock<IMediaAssetRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IBlobService> _blobService;
    private readonly Mock<ILogger<DeleteMediaUseCase>> _logger;
    private readonly DeleteMediaUseCase _useCase;

    public DeleteMediaUseCaseTests()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _blobService = new Mock<IBlobService>();
        _logger = new Mock<ILogger<DeleteMediaUseCase>>();
        _useCase = new DeleteMediaUseCase(_repository.Object, _unitOfWork.Object, _blobService.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingMedia_DeletesAndReturnsTrue()
    {
        // Arrange
        var asset = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Url = "https://storage.example.com/images/test.jpg"
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);
        _blobService.Setup(b => b.DeleteMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync("media-1");

        // Assert
        result.Should().BeTrue();
        _blobService.Verify(b => b.DeleteMediaAsync("test.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.DeleteAsync("id-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingMedia_ReturnsFalse()
    {
        // Arrange
        _repository.Setup(r => r.GetByMediaIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaAsset));

        // Act
        var result = await _useCase.ExecuteAsync("missing");

        // Assert
        result.Should().BeFalse();
        _blobService.Verify(b => b.DeleteMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBlobDeleteFails_StillDeletesFromDatabase()
    {
        // Arrange
        var asset = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Url = "https://storage.example.com/images/test.jpg"
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);
        _blobService.Setup(b => b.DeleteMediaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Blob storage error"));

        // Act
        var result = await _useCase.ExecuteAsync("media-1");

        // Assert
        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("id-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
