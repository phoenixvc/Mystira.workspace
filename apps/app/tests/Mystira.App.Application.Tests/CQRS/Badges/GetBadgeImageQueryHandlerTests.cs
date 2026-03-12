using FluentAssertions;
using Moq;
using Mystira.App.Application.CQRS.Badges.Queries;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Badges;

public class GetBadgeImageQueryHandlerTests
{
    private readonly Mock<IBadgeImageRepository> _repository;

    public GetBadgeImageQueryHandlerTests()
    {
        _repository = new Mock<IBadgeImageRepository>();
    }

    [Fact]
    public async Task Handle_WithExistingImage_ReturnsBadgeImageResult()
    {
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var image = new BadgeImage { Id = "img-1", ImageId = "badge-courage", ImageData = imageData, ContentType = "image/png" };
        _repository.Setup(r => r.GetByImageIdAsync("badge-courage", It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        var result = await GetBadgeImageQueryHandler.Handle(
            new GetBadgeImageQuery("badge-courage"), _repository.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ImageData.Should().BeEquivalentTo(imageData);
        result.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task Handle_WithEncodedImageId_DecodesAndFindsImage()
    {
        var imageData = new byte[] { 0xFF, 0xD8 }; // JPEG header
        var image = new BadgeImage { Id = "img-1", ImageId = "badge/courage", ImageData = imageData, ContentType = "image/jpeg" };
        _repository.Setup(r => r.GetByImageIdAsync("badge/courage", It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        var result = await GetBadgeImageQueryHandler.Handle(
            new GetBadgeImageQuery("badge%2Fcourage"), _repository.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Handle_WithFallbackToGetById_ReturnsBadgeImageResult()
    {
        var imageData = new byte[] { 0x89, 0x50 };
        var image = new BadgeImage { Id = "img-1", ImageData = imageData, ContentType = "image/png" };
        _repository.Setup(r => r.GetByImageIdAsync("img-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(BadgeImage));
        _repository.Setup(r => r.GetByIdAsync("img-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        var result = await GetBadgeImageQueryHandler.Handle(
            new GetBadgeImageQuery("img-1"), _repository.Object, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNoImageFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByImageIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(BadgeImage));
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(BadgeImage));

        var result = await GetBadgeImageQueryHandler.Handle(
            new GetBadgeImageQuery("missing"), _repository.Object, CancellationToken.None);

        result.Should().BeNull();
    }
}
