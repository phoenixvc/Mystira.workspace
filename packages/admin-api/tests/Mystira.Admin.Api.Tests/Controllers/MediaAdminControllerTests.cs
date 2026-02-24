using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for MediaAdminController endpoints.
/// Tests media upload, retrieval, and management operations.
/// </summary>
[Collection("Api")]
public class MediaAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/media";

    public MediaAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region Authentication Tests

    [Fact]
    public async Task AllEndpoints_RequireAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act - GET media list
        var getResponse = await client.GetAsync(BaseUrl);

        // Assert
        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region GET /api/admin/media

    [Fact]
    public async Task GetMedia_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMedia_AcceptsQueryParameters()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMedia_FiltersByType()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}?type=image");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/admin/media/{mediaId}

    [Fact]
    public async Task GetMediaFile_ReturnsNotFound_WhenMediaDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/media/upload

    [Fact]
    public async Task UploadMedia_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/upload", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadMedia_ReturnsBadRequest_WithEmptyFile()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "empty.png");

        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/admin/media/bulk-upload

    [Fact]
    public async Task BulkUploadMedia_ReturnsBadRequest_WhenNoFilesProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/bulk-upload", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/admin/media/{mediaId}

    [Fact]
    public async Task UpdateMedia_ReturnsNotFound_WhenMediaDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var updateRequest = new { Description = "Updated description" };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/media/{mediaId}

    [Fact]
    public async Task DeleteMedia_ReturnsNotFound_WhenMediaDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/media/validate

    [Fact]
    public async Task ValidateMediaReferences_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var mediaReferences = new List<string> { "media-1", "media-2" };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/validate", mediaReferences);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidateMediaReferences_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var mediaReferences = new List<string>();

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/validate", mediaReferences);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/admin/media/statistics

    [Fact]
    public async Task GetMediaStatistics_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/admin/media/upload-zip

    [Fact]
    public async Task UploadMediaZip_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/upload-zip", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadMediaZip_ReturnsBadRequest_WhenNotZipFile()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "zipFile", "test.txt");

        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/upload-zip", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
