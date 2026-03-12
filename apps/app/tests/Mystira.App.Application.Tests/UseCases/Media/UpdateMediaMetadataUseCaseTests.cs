using Mystira.App.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.Media;

namespace Mystira.App.Application.Tests.UseCases.Media;

public class UpdateMediaMetadataUseCaseTests
{
    private readonly Mock<IMediaAssetRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateMediaMetadataUseCase>> _logger;
    private readonly UpdateMediaMetadataUseCase _useCase;

    public UpdateMediaMetadataUseCaseTests()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateMediaMetadataUseCase>>();
        _useCase = new UpdateMediaMetadataUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesMetadata()
    {
        // Arrange
        var existing = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Description = "Old description",
            Tags = new List<string> { "old-tag" },
            MediaType = "image"
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var updateRequest = new MediaUpdateRequest
        {
            Description = "New description",
            Tags = new List<string> { "new-tag-1", "new-tag-2" }
        };

        // Act
        var result = await _useCase.ExecuteAsync("media-1", updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("New description");
        result.Tags.Should().HaveCount(2);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<MediaAsset>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingMedia_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repository.Setup(r => r.GetByMediaIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaAsset));

        var updateRequest = new MediaUpdateRequest { Description = "Test" };

        // Act
        var act = () => _useCase.ExecuteAsync("missing", updateRequest);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? mediaId)
    {
        var act = () => _useCase.ExecuteAsync(mediaId!, new MediaUpdateRequest());
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsValidationException()
    {
        var act = () => _useCase.ExecuteAsync("media-1", null!);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_PartialUpdate_OnlyChangesProvidedFields()
    {
        // Arrange
        var existing = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Description = "Original",
            Tags = new List<string> { "original-tag" },
            MediaType = "image"
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var updateRequest = new MediaUpdateRequest { Description = "Updated" };

        // Act
        var result = await _useCase.ExecuteAsync("media-1", updateRequest);

        // Assert
        result.Description.Should().Be("Updated");
        result.Tags.Should().Contain("original-tag");
        result.MediaType.Should().Be("image");
    }
}
