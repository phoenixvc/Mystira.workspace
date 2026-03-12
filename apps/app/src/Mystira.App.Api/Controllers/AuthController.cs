using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mystira.App.Api.Services;
using Mystira.Core.CQRS.Auth.Commands;
using Mystira.Core.Ports.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Wolverine;

namespace Mystira.App.Api.Controllers;

/// <summary>
/// Compatibility auth controller.
/// Auth route ownership has moved to Mystira.Identity.Api.
/// </summary>
[Route("api/auth")]
[ApiController]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IIdentityAuthGateway _identityAuthGateway;
    private readonly IMessageBus _bus;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        IIdentityAuthGateway identityAuthGateway,
        IMessageBus bus,
        ICurrentUserService currentUser)
    {
        _identityAuthGateway = identityAuthGateway;
        _bus = bus;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns authentication configuration info for clients.
    /// </summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetAuthConfig(CancellationToken ct)
    {
        var response = await _identityAuthGateway.GetAsync("api/auth/config", ct: ct);
        return await ToProxyActionResult(response);
    }

    [HttpPost("magic/request")]
    [EnableRateLimiting("magicAuth")]
    public async Task<IActionResult> RequestMagicLink([FromBody] MagicLinkRequest request, CancellationToken ct)
    {
        var response = await _identityAuthGateway.PostAsync("api/auth/magic/request", request, ct: ct);
        return await ToProxyActionResult(response);
    }

    [HttpPost("magic/resend")]
    [EnableRateLimiting("magicAuth")]
    public async Task<IActionResult> ResendMagicLink([FromBody] MagicResendRequest request, CancellationToken ct)
    {
        var response = await _identityAuthGateway.PostAsync("api/auth/magic/resend", request, ct: ct);
        return await ToProxyActionResult(response);
    }

    [HttpPost("magic/verify")]
    [EnableRateLimiting("magicAuth")]
    public async Task<IActionResult> VerifyMagicLink([FromBody] MagicVerifyRequest request, CancellationToken ct)
    {
        var response = await _identityAuthGateway.PostAsync("api/auth/magic/verify", request, ct: ct);
        return await ToProxyActionResult(response);
    }

    [HttpPost("magic/consume")]
    [EnableRateLimiting("magicAuth")]
    public async Task<IActionResult> ConsumeMagicLink([FromBody] MagicConsumeRequest request, CancellationToken ct)
    {
        var response = await _identityAuthGateway.PostAsync("api/auth/magic/consume", request, ct: ct);
        return await ToProxyActionResult(response);
    }

    [HttpPost("bootstrap-account")]
    [Authorize]
    public async Task<IActionResult> BootstrapAccount(CancellationToken ct)
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

        var account = await _bus.InvokeAsync<Account?>(new BootstrapAccountCommand(externalUserId, email, displayName), ct);
        if (account == null)
        {
            return Unauthorized(new { message = "Unable to bootstrap account" });
        }

        return Ok(account);
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
