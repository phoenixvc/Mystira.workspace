using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Infrastructure.Data;
using Xunit;

namespace Mystira.Admin.Api.Tests.Infrastructure;

/// <summary>
/// Base fixture for API integration tests providing common utilities.
/// </summary>
public class ApiTestFixture : IClassFixture<MystiraWebApplicationFactory>, IAsyncLifetime
{
    protected readonly MystiraWebApplicationFactory Factory;
    protected HttpClient AuthenticatedClient = null!;
    protected HttpClient AnonymousClient = null!;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiTestFixture(MystiraWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public virtual Task InitializeAsync()
    {
        AuthenticatedClient = Factory.CreateAuthenticatedClient();
        AnonymousClient = Factory.CreateAnonymousClient();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        AuthenticatedClient?.Dispose();
        AnonymousClient?.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a scoped service from the test server's DI container.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        using var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets the database context for seeding test data.
    /// </summary>
    protected MystiraAppDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();
    }

    /// <summary>
    /// Seeds data into the test database.
    /// </summary>
    protected async Task SeedDataAsync<T>(params T[] entities) where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();
        await context.Set<T>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deserializes the response content as JSON.
    /// </summary>
    protected static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Posts JSON content and returns the response.
    /// </summary>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(
        HttpClient client,
        string url,
        T content)
    {
        return await client.PostAsJsonAsync(url, content, JsonOptions);
    }

    /// <summary>
    /// Puts JSON content and returns the response.
    /// </summary>
    protected async Task<HttpResponseMessage> PutJsonAsync<T>(
        HttpClient client,
        string url,
        T content)
    {
        return await client.PutAsJsonAsync(url, content, JsonOptions);
    }
}

/// <summary>
/// Collection definition for tests that share the same WebApplicationFactory.
/// </summary>
[CollectionDefinition("Api")]
public class ApiTestCollection : ICollectionFixture<MystiraWebApplicationFactory>
{
}
