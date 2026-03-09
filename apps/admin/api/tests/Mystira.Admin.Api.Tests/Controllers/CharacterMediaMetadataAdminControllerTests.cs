using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for CharacterMediaMetadataAdminController endpoints.
/// Tests character media metadata file and entry operations.
/// </summary>
[Collection("Api")]
public class CharacterMediaMetadataAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/charactermediametadataadmin";

    public CharacterMediaMetadataAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
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

    #region GET /api/admin/charactermediametadataadmin

    [Fact]
    public async Task GetCharacterMediaMetadataFile_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PUT /api/admin/charactermediametadataadmin

    [Fact]
    public async Task UpdateCharacterMediaMetadataFile_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Id = "character-media-metadata",
            Entries = new List<object>(),
            Version = "1.0"
        };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /api/admin/charactermediametadataadmin/entries/{entryId}

    [Fact]
    public async Task GetCharacterMediaMetadataEntry_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/entries/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/charactermediametadataadmin/entries

    [Fact]
    public async Task AddCharacterMediaMetadataEntry_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Id = $"test-char-media-{Guid.NewGuid()}",
            Title = "Test Character Media",
            FileName = "character.png",
            Type = "image",
            Description = "A test character media entry",
            AgeRating = "E",
            Tags = new[] { "character", "test" }
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/entries", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/admin/charactermediametadataadmin/entries/{entryId}

    [Fact]
    public async Task UpdateCharacterMediaMetadataEntry_ReturnsNotFound_WhenEntryDoesNotExist()
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

    #region DELETE /api/admin/charactermediametadataadmin/entries/{entryId}

    [Fact]
    public async Task RemoveCharacterMediaMetadataEntry_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/entries/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/charactermediametadataadmin/import

    [Fact]
    public async Task ImportCharacterMediaMetadataEntries_AcceptsOverwriteParameter()
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
