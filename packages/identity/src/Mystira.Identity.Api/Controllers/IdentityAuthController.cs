using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mystira.App.Application.CQRS.Auth.Commands;
using Mystira.App.Application.Ports.Services;
using Mystira.App.Domain.Models;
using Mystira.Identity.Api.Services;
using Wolverine;

namespace Mystira.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class IdentityAuthController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityTokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityAuthController> _logger;

    public IdentityAuthController(
        IMessageBus bus,
        ICurrentUserService currentUser,
        IIdentityTokenService tokenService,
        IConfiguration configuration,
        ILogger<IdentityAuthController> logger)
    {
        _bus = bus;
        _currentUser = currentUser;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("config")]
    public ActionResult GetAuthConfig()
    {
        return Ok(new
        {
            provider = "Mystira Identity API",
            message = "Centralized identity authority for app and admin clients"
        });
    }

    [HttpPost("magic/request")]
    public async Task<ActionResult<MagicSignupResult>> RequestMagicLink([FromBody] MagicLinkRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var baseUrl = ResolveClientBaseUrl();
        var result = await _bus.InvokeAsync<MagicSignupResult>(new RequestMagicSignupCommand(request.Email, request.DisplayName, baseUrl));
        return Ok(new MagicSignupResult(result.PendingSignupId, result.Status, "If the email is valid, a magic link has been sent."));
    }

    [HttpPost("magic/resend")]
    public async Task<ActionResult<MagicSignupResult>> ResendMagicLink([FromBody] MagicResendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var baseUrl = ResolveClientBaseUrl();
        var result = await _bus.InvokeAsync<MagicSignupResult>(new ResendMagicSignupCommand(request.Email, baseUrl));
        return Ok(new MagicSignupResult(result.PendingSignupId, result.Status, "If the email is valid, a magic link has been sent."));
    }

    [HttpPost("magic/verify")]
    public async Task<ActionResult<VerifyMagicSignupResult>> VerifyMagicLink([FromBody] MagicVerifyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var result = await _bus.InvokeAsync<VerifyMagicSignupResult>(new VerifyMagicSignupCommand(request.Token));
        return Ok(result);
    }

    [HttpPost("magic/consume")]
    public async Task<ActionResult<MagicConsumeResponse>> ConsumeMagicLink([FromBody] MagicConsumeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var result = await _bus.InvokeAsync<ConsumeMagicSignupResult>(new ConsumeMagicSignupCommand(request.Token));
        if (result.Account == null)
        {
            return BadRequest(new MagicConsumeResponse(null, null, null, result.Status, result.Message));
        }

        var token = _tokenService.CreateAccountToken(result.Account, "email");
        return Ok(new MagicConsumeResponse(token.AccessToken, token.ExpiresAtUtc, result.Account, result.Status, result.Message));
    }

    [HttpPost("bootstrap-account")]
    [Authorize]
    public async Task<ActionResult<Account>> BootstrapAccount()
    {
        var email = _currentUser.GetEmail();
        var displayName = _currentUser.GetDisplayName();
        var externalUserId =
            _currentUser.GetClaim("sub")
            ?? _currentUser.GetClaim("oid")
            ?? _currentUser.GetAccountId();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(externalUserId))
        {
            return Unauthorized(new { message = "Missing required identity claims" });
        }

        var account = await _bus.InvokeAsync<Account?>(new BootstrapAccountCommand(externalUserId, email, displayName));
        if (account == null)
        {
            return Unauthorized(new { message = "Unable to bootstrap account" });
        }

        return Ok(account);
    }

    [HttpPost("admin/login")]
    [AllowAnonymous]
    public ActionResult<AdminLoginResponse> AdminLogin([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var configuredUsername = _configuration["AdminAuth:Username"];
        var configuredPasswordHash = _configuration["AdminAuth:PasswordHash"];

        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPasswordHash))
        {
            _logger.LogError("AdminAuth credentials not configured in identity authority.");
            return Unauthorized(new { message = "Authentication not configured" });
        }

        var providedHash = ComputeSha256Hash(request.Password);

        var usernameMatch = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(request.Username.Trim().ToLowerInvariant()),
            Encoding.UTF8.GetBytes(configuredUsername.Trim().ToLowerInvariant()));

        var passwordMatch = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedHash.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(configuredPasswordHash.Trim().ToLowerInvariant()));

        if (!usernameMatch || !passwordMatch)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var configuredRoles = (_configuration["AdminAuth:Roles"] ?? "Admin")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();

        var token = _tokenService.CreateAdminToken(request.Username.Trim(), configuredRoles);

        return Ok(new AdminLoginResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            configuredRoles));
    }

    [HttpGet("admin/status")]
    [Authorize]
    public ActionResult<AdminAuthStatusResponse> GetAdminStatus()
    {
        var roles = User.FindAll(ClaimTypes.Role)
            .Concat(User.FindAll("role"))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        DateTimeOffset? expiresAt = null;
        var expClaim = User.FindFirst("exp")?.Value;
        if (long.TryParse(expClaim, out var expUnix))
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        }

        return Ok(new AdminAuthStatusResponse(
            true,
            User.Identity?.Name,
            roles,
            expiresAt));
    }

    [HttpPost("admin/logout")]
    [Authorize]
    public ActionResult LogoutAdmin()
    {
        // Stateless JWT logout. Token revocation can be added later using deny-lists/rotation.
        return Ok(new { message = "Logged out" });
    }

    private string ResolveClientBaseUrl()
    {
        var configured = _configuration["MagicAuth:PwaBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var origin = Request.Headers.Origin.ToString();
        if (!string.IsNullOrWhiteSpace(origin))
        {
            return origin;
        }

        return $"{Request.Scheme}://{Request.Host}";
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public record MagicLinkRequest(string Email, string? DisplayName);
public record MagicResendRequest(string Email);
public record MagicVerifyRequest(string Token);
public record MagicConsumeRequest(string Token);

public record MagicConsumeResponse(
    string? AccessToken,
    DateTime? ExpiresAtUtc,
    Account? Account,
    string Status,
    string Message
);

public record AdminLoginRequest(string Username, string Password);

public record AdminLoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    IReadOnlyCollection<string> Roles
);

public record AdminAuthStatusResponse(
    bool IsAuthenticated,
    string? Username,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset? ExpiresAt
);
