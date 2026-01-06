using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Infrastructure.Data;

namespace Mystira.Admin.Api.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests with test authentication and in-memory database.
/// </summary>
public class MystiraWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestUsername = "test-admin";
    public const string TestRole = "Admin";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MystiraAppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<MystiraAppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"MystiraTestDb_{Guid.NewGuid()}");
            });

            // Add test authentication scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });

            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Creates an HttpClient with test authentication (authenticated as admin).
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        return client;
    }

    /// <summary>
    /// Creates an HttpClient with custom role authentication.
    /// </summary>
    public HttpClient CreateClientWithRole(string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    /// <summary>
    /// Creates an HttpClient without authentication (anonymous).
    /// </summary>
    public HttpClient CreateAnonymousClient()
    {
        return CreateClient();
    }
}

/// <summary>
/// Test authentication handler that allows easy authentication control in tests.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string AuthenticatedHeader = "X-Test-Authenticated";
    public const string RoleHeader = "X-Test-Role";
    public const string UsernameHeader = "X-Test-Username";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if we should authenticate this request
        if (!Request.Headers.TryGetValue(AuthenticatedHeader, out var authHeader) ||
            authHeader != "true")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Get role from header or use default
        var role = Request.Headers.TryGetValue(RoleHeader, out var roleHeader)
            ? roleHeader.ToString()
            : MystiraWebApplicationFactory.TestRole;

        // Get username from header or use default
        var username = Request.Headers.TryGetValue(UsernameHeader, out var usernameHeader)
            ? usernameHeader.ToString()
            : MystiraWebApplicationFactory.TestUsername;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
