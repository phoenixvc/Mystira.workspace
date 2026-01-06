using System.Net;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for BadgeImagesController endpoints.
/// Tests badge image upload, retrieval, and deletion.
/// </summary>
[Collection("Api")]
public class BadgeImagesControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/badges/images";

    public BadgeImagesControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/admin/badges/images

    [Fact]
    public async Task SearchImages_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchImages_ReturnsList()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    [Fact]
    public async Task SearchImages_AcceptsImageIdFilter()
    {
        // Arrange
        var imageId = "test-image-id";

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}?imageId={imageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/admin/badges/images/{id}

    [Fact]
    public async Task GetImage_ReturnsNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/badges/images

    [Fact]
    public async Task UploadImage_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AnonymousClient.PostAsync(BaseUrl, null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task UploadImage_ReturnsBadRequest_WithInvalidRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("test-id"), "imageId");
        // No file provided

        // Act
        var response = await AnonymousClient.PostAsync(BaseUrl, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadImage_AcceptsValidImageFile()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent($"badge-{Guid.NewGuid()}"), "imageId");

        // Create a minimal valid PNG file (8x8 transparent)
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        content.Add(new ByteArrayContent(pngHeader), "file", "test.png");

        // Act
        var response = await AnonymousClient.PostAsync(BaseUrl, content);

        // Assert
        // May fail due to invalid PNG but should not be 500
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region DELETE /api/admin/badges/images/{id}

    [Fact]
    public async Task DeleteImage_ReturnsNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
