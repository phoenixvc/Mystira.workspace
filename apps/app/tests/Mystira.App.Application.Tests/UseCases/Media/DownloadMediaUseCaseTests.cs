using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Net;

namespace Mystira.App.Application.Tests.UseCases.Media;

public class DownloadMediaUseCaseTests
{
    private readonly Mock<IMediaAssetRepository> _repository;
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<ILogger<DownloadMediaUseCase>> _logger;
    private readonly DownloadMediaUseCase _useCase;

    public DownloadMediaUseCaseTests()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _httpClientFactory = new Mock<IHttpClientFactory>();
        _logger = new Mock<ILogger<DownloadMediaUseCase>>();
        _useCase = new DownloadMediaUseCase(_repository.Object, _httpClientFactory.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingMedia_ReturnsNull()
    {
        // Arrange
        _repository.Setup(r => r.GetByMediaIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaAsset));

        // Act
        var result = await _useCase.ExecuteAsync("missing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingMedia_ReturnsStreamData()
    {
        // Arrange
        var asset = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Url = "https://storage.example.com/images/test.jpg",
            MimeType = "image/jpeg"
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        using var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        using var httpClient = new HttpClient(mockHandler.Object);
        _httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Act
        var result = await _useCase.ExecuteAsync("media-1");

        // Assert
        result.Should().NotBeNull();
        var nonNullResult = result!;
        nonNullResult.Value.contentType.Should().Be("image/jpeg");
        nonNullResult.Value.fileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task ExecuteAsync_WhenHttpFails_ReturnsNull()
    {
        // Arrange
        var asset = new MediaAsset
        {
            Id = "id-1",
            MediaId = "media-1",
            Url = "https://storage.example.com/images/test.jpg",
            MimeType = "image/jpeg"
        };
        _repository.Setup(r => r.GetByMediaIdAsync("media-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);

        using var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        using var httpClient = new HttpClient(mockHandler.Object);
        _httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Act
        var result = await _useCase.ExecuteAsync("media-1");

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
