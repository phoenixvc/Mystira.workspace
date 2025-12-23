using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mystira.App.Admin.Api.Controllers;

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
/// Authentication controller for Admin API.
/// Uses SHA256 hashed password comparison with timing attack protection.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    // Brute-force protection: minimum delay between login attempts
    private static readonly TimeSpan MinLoginDelay = TimeSpan.FromMilliseconds(500);

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with username and password.
    /// Password is compared against SHA256 hash stored in configuration.
    /// </summary>
    /// <remarks>
    /// Configure credentials in appsettings.json or User Secrets:
    /// <code>
    /// {
    ///   "AdminAuth": {
    ///     "Username": "admin",
    ///     "PasswordHash": "&lt;sha256-hash&gt;"
    ///   }
    /// }
    /// </code>
    /// Generate hash: echo -n "your-password" | sha256sum
    /// </remarks>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            // Get configured credentials
            var configuredUsername = _configuration["AdminAuth:Username"];
            var configuredPasswordHash = _configuration["AdminAuth:PasswordHash"];

            // SECURITY: Fail if credentials are not configured
            if (string.IsNullOrEmpty(configuredUsername) || string.IsNullOrEmpty(configuredPasswordHash))
            {
                _logger.LogError("AdminAuth credentials not configured. Set AdminAuth:Username and AdminAuth:PasswordHash in configuration.");

                // Still apply delay to prevent timing attacks
                await ApplyMinimumDelay(startTime);
                return Unauthorized(new { Message = "Authentication not configured. Contact administrator." });
            }

            // SECURITY: Hash the provided password for comparison
            var providedPasswordHash = ComputeSha256Hash(request.Password);

            // SECURITY: Use constant-time comparison to prevent timing attacks
            var usernameMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(request.Username.ToLowerInvariant()),
                Encoding.UTF8.GetBytes(configuredUsername.ToLowerInvariant()));

            var passwordMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(providedPasswordHash.ToLowerInvariant()),
                Encoding.UTF8.GetBytes(configuredPasswordHash.ToLowerInvariant()));

            if (usernameMatch && passwordMatch)
            {
                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, request.Username),
                    new(ClaimTypes.Role, "Admin"),
                    new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

                // Sign in with cookie authentication
                await HttpContext.SignInAsync(
                    "Cookies",
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = expiresAt,
                        IssuedUtc = DateTimeOffset.UtcNow
                    });

                _logger.LogInformation("User {Username} logged in successfully from {IpAddress}",
                    request.Username,
                    HttpContext.Connection.RemoteIpAddress);

                // Apply minimum delay even on success (prevents timing attacks)
                await ApplyMinimumDelay(startTime);

                return Ok(new
                {
                    Message = "Login successful",
                    ExpiresAt = expiresAt
                });
            }

            // Log failed attempt (without revealing which field was wrong)
            _logger.LogWarning("Failed login attempt for user {Username} from {IpAddress}",
                request.Username,
                HttpContext.Connection.RemoteIpAddress);

            // SECURITY: Apply brute-force delay
            await ApplyMinimumDelay(startTime);

            return Unauthorized(new { Message = "Invalid username or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt");
            await ApplyMinimumDelay(startTime);
            return StatusCode(500, new { Message = "An error occurred during authentication" });
        }
    }

    /// <summary>
    /// Log out the current user.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "unknown";
        await HttpContext.SignOutAsync("Cookies");

        _logger.LogInformation("User {Username} logged out", username);

        return Ok(new { Message = "Logged out successfully" });
    }

    /// <summary>
    /// Get the current authentication status.
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetAuthStatus()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var authTime = User.FindFirst("auth_time")?.Value;
            DateTimeOffset? expiresAt = null;

            // Try to get expiration from auth properties
            var authResult = HttpContext.AuthenticateAsync("Cookies").Result;
            if (authResult.Properties?.ExpiresUtc != null)
            {
                expiresAt = authResult.Properties.ExpiresUtc;
            }

            return Ok(new AuthStatusResponse
            {
                IsAuthenticated = true,
                Username = User.Identity.Name,
                Role = User.FindFirst(ClaimTypes.Role)?.Value,
                ExpiresAt = expiresAt
            });
        }

        return Ok(new AuthStatusResponse
        {
            IsAuthenticated = false,
            Username = null,
            Role = null,
            ExpiresAt = null
        });
    }

    /// <summary>
    /// Compute SHA256 hash of a string.
    /// </summary>
    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Apply minimum delay to prevent timing attacks.
    /// Ensures all login attempts take at least MinLoginDelay time.
    /// </summary>
    private static async Task ApplyMinimumDelay(DateTimeOffset startTime)
    {
        var elapsed = DateTimeOffset.UtcNow - startTime;
        if (elapsed < MinLoginDelay)
        {
            await Task.Delay(MinLoginDelay - elapsed);
        }
    }
}
