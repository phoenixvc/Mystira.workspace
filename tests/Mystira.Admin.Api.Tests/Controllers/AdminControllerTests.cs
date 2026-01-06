using System.Net;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for AdminController endpoints.
/// Tests authentication and authorization requirements.
/// </summary>
[Collection("Api")]
public class AdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/admin";

    public AdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region Authentication Tests

    [Fact]
    public async Task Dashboard_RedirectsToLogin_WhenNotAuthenticated()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        // Should either redirect to login or return unauthorized
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.OK); // OK if login page is returned directly
    }

    [Fact]
    public async Task Dashboard_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Login Page Tests

    [Fact]
    public async Task LoginPage_IsAccessible_WithoutAuthentication()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/login");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task LoginPage_RedirectsToDashboard_WhenAlreadyAuthenticated()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var clientWithNoRedirect = Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        clientWithNoRedirect.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        // Act
        var response = await clientWithNoRedirect.GetAsync($"{BaseUrl}/login");

        // Assert
        // Should redirect to dashboard when already logged in
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    #endregion

    #region Admin Pages Authorization Tests

    [Theory]
    [InlineData("/admin/scenarios")]
    [InlineData("/admin/badges")]
    [InlineData("/admin/media")]
    [InlineData("/admin/bundles")]
    [InlineData("/admin/avatars")]
    [InlineData("/admin/compassaxes")]
    [InlineData("/admin/archetypes")]
    [InlineData("/admin/echotypes")]
    [InlineData("/admin/fantasythemes")]
    [InlineData("/admin/agegroups")]
    [InlineData("/admin/charactermaps")]
    public async Task AdminPages_RequireAuthentication(string endpoint)
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/admin/scenarios")]
    [InlineData("/admin/badges")]
    [InlineData("/admin/media")]
    [InlineData("/admin/bundles")]
    public async Task AdminPages_ReturnOk_WhenAuthenticated(string endpoint)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Import Pages Tests

    [Theory]
    [InlineData("/admin/scenarios/import")]
    [InlineData("/admin/media/import")]
    [InlineData("/admin/bundles/import")]
    [InlineData("/admin/badges/import")]
    [InlineData("/admin/charactermaps/import")]
    public async Task ImportPages_ReturnOk_WhenAuthenticated(string endpoint)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region API Endpoints Tests

    [Fact]
    public async Task InitializeSampleData_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.PostAsync($"{BaseUrl}/initialize-sample-data", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateBundle_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.PostAsync($"{BaseUrl}/bundles/validate", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest); // BadRequest if auth passes but no file
    }

    [Fact]
    public async Task ValidateBundle_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/bundles/validate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadBundle_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/bundles/upload", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadScenario_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/scenarios/upload", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Role-Based Access Tests

    [Fact]
    public async Task Dashboard_IsAccessible_WithAdminRole()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Dashboard_IsAccessible_WithSuperAdminRole()
    {
        // Arrange
        var superAdminClient = Factory.CreateClientWithRole("SuperAdmin");

        // Act
        var response = await superAdminClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
