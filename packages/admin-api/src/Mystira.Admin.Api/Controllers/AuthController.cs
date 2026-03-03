using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mystira.Admin.Api.Services;

namespace Mystira.Admin.Api.Controllers;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthStatusResponse
{
    public bool IsAuthenticated { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <summary>
/// Compatibility auth controller for Admin API.
/// Auth route ownership has moved to Mystira.Identity.Api.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IIdentityAuthGateway _identityAuthGateway;

    public AuthController(IIdentityAuthGateway identityAuthGateway, ILogger<AuthController> logger)
    {
        _identityAuthGateway = identityAuthGateway;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate admin user with username and password via Identity API.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _identityAuthGateway.PostAsync("api/auth/admin/login", request, ct: ct);
        return await ToProxyActionResult(response);
    }

    /// <summary>
    /// Log out the current user.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var bearerToken = GetBearerToken();
        var response = await _identityAuthGateway.PostAsync("api/auth/admin/logout", bearerToken: bearerToken, ct: ct);
        return await ToProxyActionResult(response);
    }

    /// <summary>
    /// Get the current authentication status.
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuthStatus(CancellationToken ct)
    {
        var bearerToken = GetBearerToken();
        var response = await _identityAuthGateway.GetAsync("api/auth/admin/status", bearerToken, ct);
        return await ToProxyActionResult(response);
    }

    private string? GetBearerToken()
    {
        var rawHeader = Request.Headers.Authorization.ToString();
        const string bearerPrefix = "Bearer ";
        if (rawHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return rawHeader[bearerPrefix.Length..].Trim();
        }

        _logger.LogDebug("No bearer token found when proxying admin auth request.");
        return null;
    }

    private static async Task<IActionResult> ToProxyActionResult(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = payload,
            ContentType = contentType
        };
    }
}
