using System.Net.Mail;
using System.Security.Claims;
using Wolverine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mystira.App.Application.CQRS.Coppa.Commands;
using Mystira.App.Application.CQRS.Coppa.Queries;
using Mystira.App.Application.Helpers;
using Mystira.Application.Ports.Data;
using Mystira.Application.Ports.Services;
using Mystira.App.Application.Services;
using Mystira.App.Api.Models;

namespace Mystira.App.Api.Controllers;

/// <summary>
/// Controller for COPPA compliance operations.
/// Provides endpoints for age verification, parental consent, and data management.
/// </summary>
[ApiController]
[Route("api/coppa")]
[Produces("application/json")]
[EnableRateLimiting("coppa")]
public class CoppaController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserProfileRepository _profileRepo;
    private readonly ILogger<CoppaController> _logger;

    public CoppaController(
        IMessageBus bus,
        ICurrentUserService currentUser,
        IUserProfileRepository profileRepo,
        ILogger<CoppaController> logger)
    {
        _bus = bus;
        _currentUser = currentUser;
        _profileRepo = profileRepo;
        _logger = logger;
    }

    /// <summary>
    /// Request parental consent for a child profile.
    /// Sends a verification email to the parent's email address.
    /// </summary>
    [HttpPost("consent/request")]
    [ProducesResponseType(typeof(ParentalConsentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParentalConsentResult>> RequestConsent(
        [FromBody] RequestConsentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ChildProfileId))
            return BadRequest(new ErrorResponse { Message = "Child profile ID is required", TraceId = HttpContext.TraceIdentifier });
        if (string.IsNullOrWhiteSpace(request.ParentEmail))
            return BadRequest(new ErrorResponse { Message = "Parent email is required", TraceId = HttpContext.TraceIdentifier });
        if (!MailAddress.TryCreate(request.ParentEmail, out _))
            return BadRequest(new ErrorResponse { Message = "Parent email format is invalid", TraceId = HttpContext.TraceIdentifier });

        var command = new RequestParentalConsentCommand(
            request.ChildProfileId,
            request.ParentEmail,
            request.ChildDisplayName ?? "Child");

        var result = await _bus.InvokeAsync<ParentalConsentResult>(command);
        return Ok(result);
    }

    /// <summary>
    /// Verify parental consent using a verification token (POST).
    /// Called by the PWA verification page after the parent clicks the email link.
    /// </summary>
    [HttpPost("consent/verify")]
    [ProducesResponseType(typeof(ParentalConsentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParentalConsentResult>> VerifyConsent(
        [FromBody] VerifyConsentDto request)
    {
        return await ProcessVerification(request.VerificationToken, request.VerificationMethod ?? "Email");
    }

    /// <summary>
    /// Verify parental consent using a verification token (GET).
    /// Allows direct verification via email link click. Redirects to a result page.
    /// </summary>
    [HttpGet("consent/verify")]
    [ProducesResponseType(typeof(ParentalConsentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParentalConsentResult>> VerifyConsentViaLink([FromQuery] string token)
    {
        return await ProcessVerification(token, "Email");
    }

    private async Task<ActionResult<ParentalConsentResult>> ProcessVerification(string? token, string method)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new ErrorResponse { Message = "Verification token is required", TraceId = HttpContext.TraceIdentifier });

        var command = new VerifyParentalConsentCommand(token, method);
        var result = await _bus.InvokeAsync<ParentalConsentResult>(command);

        if (result.Status == "NotFound" || result.Status == "Expired")
            return BadRequest(new ErrorResponse { Message = result.Message, TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Get consent status for a child profile. Requires authentication.
    /// Validates that the caller is the parent/guardian who owns the profile.
    /// </summary>
    [Authorize]
    [HttpGet("consent/status/{childProfileId}")]
    [ProducesResponseType(typeof(ConsentStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConsentStatusResult>> GetConsentStatus(string childProfileId)
    {
        if (string.IsNullOrWhiteSpace(childProfileId))
            return BadRequest(new ErrorResponse { Message = "Child profile ID is required", TraceId = HttpContext.TraceIdentifier });

        // Verify the caller owns this child profile
        var accountId = _currentUser.GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized();

        var accountProfiles = await _profileRepo.GetByAccountIdAsync(accountId);
        if (!accountProfiles.Any(p => p.Id == childProfileId))
        {
            _logger.LogWarning(
                "Forbidden: account {AccountIdHash} attempted to access consent status for profile {ProfileIdHash}",
                LogAnonymizer.HashId(accountId), LogAnonymizer.HashId(childProfileId));
            return Forbid();
        }

        var query = new GetConsentStatusQuery(childProfileId);
        var result = await _bus.InvokeAsync<ConsentStatusResult>(query);
        return Ok(result);
    }

    /// <summary>
    /// Revoke parental consent (triggers data deletion workflow).
    /// Requires authentication. Accepts parent email and hashes it server-side.
    /// </summary>
    [Authorize]
    [HttpPost("consent/revoke")]
    [ProducesResponseType(typeof(ParentalConsentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ParentalConsentResult>> RevokeConsent(
        [FromBody] RevokeConsentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ChildProfileId))
            return BadRequest(new ErrorResponse { Message = "Child profile ID is required", TraceId = HttpContext.TraceIdentifier });
        if (string.IsNullOrWhiteSpace(request.ParentEmail))
            return BadRequest(new ErrorResponse { Message = "Parent email is required", TraceId = HttpContext.TraceIdentifier });

        // Verify the caller owns this child profile
        var accountId = _currentUser.GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized();

        var accountProfiles = await _profileRepo.GetByAccountIdAsync(accountId);
        if (!accountProfiles.Any(p => p.Id == request.ChildProfileId))
        {
            _logger.LogWarning(
                "Forbidden: account {AccountIdHash} attempted to revoke consent for profile {ProfileIdHash}",
                LogAnonymizer.HashId(accountId), LogAnonymizer.HashId(request.ChildProfileId));
            return Forbid();
        }

        var emailHash = EmailHasher.Hash(request.ParentEmail);

        var command = new RevokeConsentCommand(request.ChildProfileId, emailHash);
        var result = await _bus.InvokeAsync<ParentalConsentResult>(command);

        if (result.Status == "Unauthorized")
            return Forbid();
        if (result.Status == "NotFound")
            return BadRequest(new ErrorResponse { Message = result.Message, TraceId = HttpContext.TraceIdentifier });

        return Ok(result);
    }

    /// <summary>
    /// Age verification endpoint. Returns whether the user requires parental consent.
    /// </summary>
    [HttpPost("age-check")]
    [ProducesResponseType(typeof(AgeCheckResult), StatusCodes.Status200OK)]
    public ActionResult<AgeCheckResult> CheckAge([FromBody] AgeCheckDto request)
    {
        if (request.Age < 0 || request.Age > 150)
            return BadRequest(new ErrorResponse { Message = "Invalid age", TraceId = HttpContext.TraceIdentifier });

        var requiresConsent = request.Age < 13;
        return Ok(new AgeCheckResult(
            RequiresParentalConsent: requiresConsent,
            AgeGroup: GetAgeGroup(request.Age),
            Message: requiresConsent
                ? "Parental consent is required for users under 13"
                : "No parental consent required"
        ));
    }

    private static string GetAgeGroup(int age) => age switch
    {
        < 3 => "1-2",
        < 6 => "3-5",
        < 10 => "6-9",
        < 13 => "10-12",
        < 19 => "13-18",
        _ => "19-150"
    };
}

// DTOs for COPPA endpoints
public record RequestConsentDto(string ChildProfileId, string ParentEmail, string? ChildDisplayName);
public record VerifyConsentDto(string VerificationToken, string? VerificationMethod);
public record RevokeConsentDto(string ChildProfileId, string ParentEmail);
public record AgeCheckDto(int Age);
public record AgeCheckResult(bool RequiresParentalConsent, string AgeGroup, string Message);
