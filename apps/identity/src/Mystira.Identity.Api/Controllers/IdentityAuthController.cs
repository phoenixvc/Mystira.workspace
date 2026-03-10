using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Mystira.App.Application.CQRS.Auth.Commands;
using Mystira.Application.Ports.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
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
    private readonly IEntraProvisioningService _entraProvisioningService;
    private readonly IProvisioningQueue _provisioningQueue;
    private readonly HttpClient _httpClient;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public IdentityAuthController(
        IMessageBus bus,
        ICurrentUserService currentUser,
        IIdentityTokenService tokenService,
        IConfiguration configuration,
        ILogger<IdentityAuthController> logger,
        IEntraProvisioningService entraProvisioningService,
        IProvisioningQueue provisioningQueue,
        IHttpClientFactory httpClientFactory)
    {
        _bus = bus;
        _currentUser = currentUser;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
        _entraProvisioningService = entraProvisioningService;
        _provisioningQueue = provisioningQueue;
        _httpClient = httpClientFactory.CreateClient();

        // Setup token validation parameters
        var tenantId = _configuration["EntraProvisioning:TenantId"];
        var clientId = _configuration["EntraProvisioning:ClientId"];

        if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId))
        {
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0",
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        }
        else
        {
            _tokenValidationParameters = new TokenValidationParameters(); // Fallback
        }
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

        // Trigger background Entra provisioning for magic-link users
        _ = Task.Run(async () =>
        {
            try
            {
                await TriggerEntraProvisioningAsync(result.Account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger Entra provisioning for account {AccountId}", result.Account.Id);
            }
        });

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

    [HttpGet("entra/config")]
    public ActionResult GetEntraConfig()
    {
        var tenantId = _configuration["EntraProvisioning:TenantId"];
        var clientId = _configuration["EntraProvisioning:ClientId"];
        var entraAuthority = _configuration["EntraProvisioning:Authority"];

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            return Ok(new { enabled = false, message = "Entra authentication is not configured" });
        }

        return Ok(new
        {
            enabled = true,
            authority = entraAuthority ?? $"https://login.microsoftonline.com/{tenantId}",
            clientId = clientId,
            tenantId = tenantId,
            scopes = new[] { "openid", "profile", "email", "offline_access" }
        });
    }

    [HttpPost("entra/login")]
    public async Task<ActionResult<EntraLoginResponse>> EntraLogin([FromBody] EntraLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return BadRequest(new { message = "ID token is required" });
        }

        try
        {
            // Validate the Entra ID token with proper signature and claim validation
            var claimsPrincipal = await ValidateIdTokenAsync(request.IdToken);
            if (claimsPrincipal == null)
            {
                return BadRequest(new { message = "Invalid ID token: validation failed" });
            }

            // Extract required claims from validated token
            var email = claimsPrincipal.FindFirst(c => c.Type == "email" || c.Type == "upn")?.Value;
            var displayName = claimsPrincipal.FindFirst(c => c.Type == "name")?.Value;
            var objectId = claimsPrincipal.FindFirst(c => c.Type == "oid")?.Value;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(objectId))
            {
                return BadRequest(new { message = "Invalid token: missing required claims" });
            }

            // Bootstrap or create account
            var account = await _bus.InvokeAsync<Account?>(new BootstrapAccountCommand(objectId, email, displayName ?? email));
            if (account == null)
            {
                return Unauthorized(new { message = "Unable to bootstrap account" });
            }

            // Create token
            var authToken = _tokenService.CreateAccountToken(account, "entra");

            return Ok(new EntraLoginResponse(
                authToken.AccessToken,
                authToken.ExpiresAtUtc,
                account
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Entra login");
            return BadRequest(new { message = "Failed to process Entra login" });
        }
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

    private async Task<ClaimsPrincipal?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _configuration["EntraProvisioning:TenantId"];
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogError("Entra tenant ID not configured");
                return null;
            }

            // Fetch OIDC configuration and signing keys
            var ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var openIdConfig = await ConfigurationManager.GetConfigurationAsync(cancellationToken);
            _tokenValidationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var result = await tokenHandler.ValidateTokenAsync(idToken, _tokenValidationParameters);

            if (!result.IsValid)
            {
                _logger.LogWarning("Token validation failed: {Errors}", string.Join(", ", result.Exception?.Message ?? "Unknown error"));
                return null;
            }

            // Convert the claims dictionary to a ClaimsIdentity
            var claims = result.Claims.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "JwtBearer"));
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning(ex, "Security token validation failed");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ID token");
            return null;
        }
    }

    private async Task TriggerEntraProvisioningAsync(Account account)
    {
        // Check if account already has Entra linkage
        if (!string.IsNullOrWhiteSpace(account.EntraObjectId))
        {
            _logger.LogDebug("Account {AccountId} already linked to Entra user {EntraObjectId}",
                account.Id, account.EntraObjectId);
            return;
        }

        // Check if Entra user already exists
        var existingEntraUser = await _entraProvisioningService.FindUserByEmailAsync(account.Email);
        if (existingEntraUser != null)
        {
            // Link existing Entra user
            var linkResult = await _entraProvisioningService.LinkExistingUserAsync(account.Id, existingEntraUser.ObjectId);
            if (linkResult.IsSuccess)
            {
                _logger.LogInformation("Linked existing account {AccountId} to Entra user {EntraObjectId}",
                    account.Id, existingEntraUser.ObjectId);
                return;
            }
        }

        // Queue provisioning job
        var provisioningJob = new ProvisioningJob(
            JobId: Guid.NewGuid().ToString(),
            Email: account.Email,
            DisplayName: account.DisplayName,
            AccountId: account.Id,
            AttemptCount: 0,
            NextAttemptAt: DateTime.UtcNow,
            CreatedAt: DateTime.UtcNow
        );

        await _provisioningQueue.EnqueueProvisioningJobAsync(provisioningJob);
        _logger.LogInformation("Queued Entra provisioning job {JobId} for account {AccountId}",
            provisioningJob.JobId, account.Id);
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

public record EntraLoginRequest(string IdToken);

public record EntraLoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Account Account
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
