using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Api.Controllers;
using Mystira.App.Application.CQRS.Badges.Queries;
using Wolverine;

namespace Mystira.App.Api.Tests.Controllers;

public class BadgeImagesControllerTests
{
    private readonly Mock<IMessageBus> _mockBus;
    private readonly Mock<ILogger<BadgeImagesController>> _mockLogger;
    private readonly BadgeImagesController _controller;

    public BadgeImagesControllerTests()
    {
        _mockBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<BadgeImagesController>>();
        _controller = new BadgeImagesController(_mockBus.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetBadgeImage_WithValidImageId_ReturnsFileResult()
    {
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header bytes
        var result = new BadgeImageResult(imageData, "image/png");

        _mockBus.Setup(x => x.InvokeAsync<BadgeImageResult?>(
                It.IsAny<GetBadgeImageQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(result);

        var actionResult = await _controller.GetBadgeImage("badge-img-1");

        var fileResult = actionResult.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("image/png");
        fileResult.FileContents.Should().BeEquivalentTo(imageData);
    }

    [Fact]
    public async Task GetBadgeImage_WithNonExistentImageId_ReturnsNotFound()
    {
        _mockBus.Setup(x => x.InvokeAsync<BadgeImageResult?>(
                It.IsAny<GetBadgeImageQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(default(BadgeImageResult));

        var result = await _controller.GetBadgeImage("nonexistent");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetBadgeImage_WithNullOrEmptyImageId_ReturnsBadRequest(string? imageId)
    {
        var result = await _controller.GetBadgeImage(imageId!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetBadgeImage_WhenExceptionThrown_ReturnsInternalServerError()
    {
        _mockBus.Setup(x => x.InvokeAsync<BadgeImageResult?>(
                It.IsAny<GetBadgeImageQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new Exception("Storage error"));

        var result = await _controller.GetBadgeImage("badge-img-1");

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetBadgeImage_SetsCacheControlHeader()
    {
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var badgeResult = new BadgeImageResult(imageData, "image/png");

        _mockBus.Setup(x => x.InvokeAsync<BadgeImageResult?>(
                It.IsAny<GetBadgeImageQuery>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(badgeResult);

        await _controller.GetBadgeImage("badge-img-1");

        _controller.Response.Headers.CacheControl.ToString().Should().Contain("public");
    }
}
