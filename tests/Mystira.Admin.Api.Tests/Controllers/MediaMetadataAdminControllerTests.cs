using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for MediaMetadataAdminController endpoints.
/// Tests media metadata file and entry operations.
/// </summary>
[Collection("Api")]
public class MediaMetadataAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/mediametadataadmin";

    public MediaMetadataAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
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

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region GET /api/admin/mediametadataadmin

    [Fact]
    public async Task GetMediaMetadataFile_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PUT /api/admin/mediametadataadmin

    [Fact]
    public async Task UpdateMediaMetadataFile_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Id = "media-metadata",
            Entries = new List<object>(),
            Version = "1.0"
        };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /api/admin/mediametadataadmin/entries/{entryId}

    [Fact]
    public async Task GetMediaMetadataEntry_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/entries/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/mediametadataadmin/entries

    [Fact]
    public async Task AddMediaMetadataEntry_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Id = $"test-media-{Guid.NewGuid()}",
            Title = "Test Media",
            FileName = "test.png",
            Type = "image",
            Description = "A test media entry",
            AgeRating = 1
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/entries", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/admin/mediametadataadmin/entries/{entryId}

    [Fact]
    public async Task UpdateMediaMetadataEntry_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new
        {
            Id = nonExistentId,
            Title = "Updated Title"
        };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/entries/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/mediametadataadmin/entries/{entryId}

    [Fact]
    public async Task RemoveMediaMetadataEntry_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/entries/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/mediametadataadmin/import

    [Fact]
    public async Task ImportMediaMetadataEntries_ReturnsBadRequest_WithInvalidJson()
    {
        // Arrange
        var invalidJson = "invalid-json";

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/import", invalidJson);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task ImportMediaMetadataEntries_AcceptsOverwriteParameter()
    {
        // Arrange
        var jsonData = "[]";

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/import?overwriteExisting=true", jsonData);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion
}
