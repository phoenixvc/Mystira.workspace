using Wolverine;
using Microsoft.AspNetCore.Mvc;
using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.App.Application.CQRS.Accounts.Queries;
using Mystira.App.Application.CQRS.UserProfiles.Queries;
using Mystira.App.Domain.Models;

namespace Mystira.App.Api.Controllers;

/// <summary>
/// Controller for account management.
/// Follows hexagonal architecture - uses only IMessageBus (CQRS pattern).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMessageBus bus, ILogger<AccountsController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Get account by email address
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<Account>> GetAccountByEmail(string email)
    {
        var query = new GetAccountByEmailQuery(email);
        var account = await _bus.InvokeAsync<Account?>(query);
        if (account == null)
        {
            return NotFound($"Account with email {email} not found");
        }

        return Ok(account);
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    [HttpGet("{accountId}")]
    public async Task<ActionResult<Account>> GetAccountById(string accountId)
    {
        var query = new GetAccountQuery(accountId);
        var account = await _bus.InvokeAsync<Account?>(query);
        if (account == null)
        {
            return NotFound($"Account with ID {accountId} not found");
        }

        return Ok(account);
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest("Email is required");
        }

        if (string.IsNullOrEmpty(request.ExternalUserId))
        {
            return BadRequest("External User ID is required");
        }

        var command = new CreateAccountCommand(
            request.ExternalUserId,
            request.Email,
            request.DisplayName,
            request.UserProfileIds,
            request.Subscription,
            request.Settings);

        var createdAccount = await _bus.InvokeAsync<Account?>(command);
        if (createdAccount == null)
        {
            return Conflict("Account with this email already exists");
        }

        return CreatedAtAction(nameof(GetAccountById), new { accountId = createdAccount.Id }, createdAccount);
    }

    /// <summary>
    /// Update an existing account
    /// </summary>
    [HttpPut("{accountId}")]
    public async Task<ActionResult<Account>> UpdateAccount(string accountId, [FromBody] UpdateAccountRequest request)
    {
        var command = new UpdateAccountCommand(
            accountId,
            request.DisplayName,
            request.UserProfileIds,
            request.Subscription,
            request.Settings);

        var updatedAccount = await _bus.InvokeAsync<Account?>(command);
        if (updatedAccount == null)
        {
            return NotFound($"Account with ID {accountId} not found");
        }

        return Ok(updatedAccount);
    }

    /// <summary>
    /// Delete an account
    /// </summary>
    [HttpDelete("{accountId}")]
    public async Task<ActionResult> DeleteAccount(string accountId)
    {
        var command = new DeleteAccountCommand(accountId);
        var success = await _bus.InvokeAsync<bool>(command);
        if (!success)
        {
            return NotFound($"Account with ID {accountId} not found");
        }

        return NoContent();
    }

    /// <summary>
    /// Link user profiles to an account
    /// </summary>
    [HttpPost("{accountId}/profiles")]
    public async Task<ActionResult> LinkProfilesToAccount(string accountId, [FromBody] LinkProfilesRequest request)
    {
        if (request.UserProfileIds == null || !request.UserProfileIds.Any())
        {
            return BadRequest("User profile IDs are required");
        }

        var command = new LinkProfilesToAccountCommand(accountId, request.UserProfileIds);
        var success = await _bus.InvokeAsync<bool>(command);
        if (!success)
        {
            return NotFound($"Account with ID {accountId} not found or no profiles were linked");
        }

        return Ok();
    }

    /// <summary>
    /// Get all user profiles for an account
    /// </summary>
    [HttpGet("{accountId}/profiles")]
    public async Task<ActionResult<List<UserProfile>>> GetAccountProfiles(string accountId)
    {
        var query = new GetProfilesByAccountQuery(accountId);
        var profiles = await _bus.InvokeAsync<List<UserProfile>>(query);
        return Ok(profiles);
    }

    /// <summary>
    /// Validate account exists
    /// </summary>
    [HttpGet("validate/{email}")]
    public async Task<ActionResult<bool>> ValidateAccount(string email)
    {
        try
        {
            var query = new ValidateAccountQuery(email);
            var isValid = await _bus.InvokeAsync<bool>(query);
            return Ok(isValid);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error validating account");
            return BadRequest(ex.Message);
        }
    }
}

/// <summary>
/// Request model for creating a new account
/// </summary>
public class CreateAccountRequest
{
    public string ExternalUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public List<string>? UserProfileIds { get; set; }
    public SubscriptionDetails? Subscription { get; set; }
    public AccountSettings? Settings { get; set; }
}

/// <summary>
/// Request model for updating an account
/// </summary>
public class UpdateAccountRequest
{
    public string? DisplayName { get; set; }
    public List<string>? UserProfileIds { get; set; }
    public SubscriptionDetails? Subscription { get; set; }
    public AccountSettings? Settings { get; set; }
}

/// <summary>
/// Request model for linking profiles to an account
/// </summary>
public class LinkProfilesRequest
{
    public List<string> UserProfileIds { get; set; } = new();
}
