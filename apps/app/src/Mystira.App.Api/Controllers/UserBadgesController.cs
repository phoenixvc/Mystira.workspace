using Wolverine;
using Microsoft.AspNetCore.Mvc;
using Mystira.Core.CQRS.UserBadges.Commands;
using Mystira.Core.CQRS.UserBadges.Queries;
using Mystira.Core.Helpers;
using Mystira.Contracts.App.Requests.Badges;
using Mystira.App.Api.Models;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Api.Controllers;

/// <summary>
/// Controller for user badge management.
/// Follows hexagonal architecture - uses only IMessageBus (CQRS pattern).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserBadgesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<UserBadgesController> _logger;

    public UserBadgesController(
        IMessageBus bus,
        ILogger<UserBadgesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Award a badge to a user profile
    /// </summary>
    [HttpPost("award")]
    public async Task<ActionResult<UserBadge>> AwardBadge([FromBody] AwardBadgeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validation failed",
                ValidationErrors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                ),
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            var command = new AwardBadgeCommand(request);
            var badge = await _bus.InvokeAsync<UserBadge>(command);
            return CreatedAtAction(nameof(GetUserBadges),
                new { userProfileId = request.UserProfileId }, badge);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error awarding badge");
            return BadRequest(new ErrorResponse
            {
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Get all badges for a user profile
    /// </summary>
    [HttpGet("user/{userProfileId}")]
    public async Task<ActionResult<List<UserBadge>>> GetUserBadges(string userProfileId)
    {
        var query = new GetUserBadgesQuery(userProfileId);
        var badges = await _bus.InvokeAsync<List<UserBadge>>(query);
        return Ok(badges);
    }

    /// <summary>
    /// Get badges for a specific axis for a user profile
    /// </summary>
    [HttpGet("user/{userProfileId}/axis/{axis}")]
    public async Task<ActionResult<List<UserBadge>>> GetUserBadgesForAxis(string userProfileId, string axis)
    {
        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);
        var badges = await _bus.InvokeAsync<List<UserBadge>>(query);
        return Ok(badges);
    }

    /// <summary>
    /// Check if a user has earned a specific badge
    /// </summary>
    [HttpGet("user/{userProfileId}/badge/{badgeConfigurationId}/earned")]
    public async Task<ActionResult<bool>> HasUserEarnedBadge(string userProfileId, string badgeConfigurationId)
    {
        var query = new HasUserEarnedBadgeQuery(userProfileId, badgeConfigurationId);
        var hasEarned = await _bus.InvokeAsync<bool>(query);
        return Ok(new { hasEarned });
    }

    /// <summary>
    /// Get badge statistics for a user profile
    /// </summary>
    [HttpGet("user/{userProfileId}/statistics")]
    public async Task<ActionResult<Dictionary<string, int>>> GetBadgeStatistics(string userProfileId)
    {
        var query = new GetBadgeStatisticsQuery(userProfileId);
        var statistics = await _bus.InvokeAsync<Dictionary<string, int>>(query);
        return Ok(statistics);
    }

    /// <summary>
    /// Get all badges for all profiles belonging to an account (identified by email)
    /// </summary>
    [HttpGet("account/{email}")]
    public async Task<ActionResult<List<UserBadge>>> GetBadgesForAccount(string email)
    {
        var query = new GetBadgesForAccountByEmailQuery(email);
        var badges = await _bus.InvokeAsync<List<UserBadge>>(query);

        if (!badges.Any())
        {
            return NotFound(new ErrorResponse
            {
                Message = "Account not found or has no badges",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(badges);
    }

    /// <summary>
    /// Get badge statistics for all profiles belonging to an account (identified by email)
    /// </summary>
    [HttpGet("account/{email}/statistics")]
    public async Task<ActionResult<Dictionary<string, int>>> GetBadgeStatisticsForAccount(string email)
    {
        var query = new GetBadgeStatisticsForAccountByEmailQuery(email);
        var statistics = await _bus.InvokeAsync<Dictionary<string, int>>(query);

        return Ok(statistics);
    }
}
