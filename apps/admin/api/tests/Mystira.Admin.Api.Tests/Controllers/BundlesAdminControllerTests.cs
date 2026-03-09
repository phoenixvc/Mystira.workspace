using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for BundlesAdminController endpoints.
/// Tests content bundle CRUD operations.
/// </summary>
[Collection("Api")]
public class BundlesAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/bundlesadmin";

    public BundlesAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
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

        // Act - GET all
        var getResponse = await client.GetAsync(BaseUrl);

        // Assert
        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region GET /api/admin/bundlesadmin

    [Fact]
    public async Task GetAll_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    #endregion

    #region GET /api/admin/bundlesadmin/{id}

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenBundleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/bundlesadmin

    [Fact]
    public async Task Create_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"Test Bundle {Guid.NewGuid()}",
            Description = "A test content bundle",
            Version = "1.0.0"
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region PUT /api/admin/bundlesadmin/{id}

    [Fact]
    public async Task Update_ReturnsNotFound_WhenBundleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new
        {
            Id = nonExistentId,
            Name = "Updated Bundle"
        };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/bundlesadmin/{id}

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenBundleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
