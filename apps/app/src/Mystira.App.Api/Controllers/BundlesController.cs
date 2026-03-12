using Wolverine;
using Microsoft.AspNetCore.Mvc;
using Mystira.Core.CQRS.Attribution.Queries;
using Mystira.Core.CQRS.ContentBundles.Queries;
using Mystira.Contracts.App.Responses.Attribution;
using Mystira.Contracts.App.Responses.Common;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BundlesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<BundlesController> _logger;

    public BundlesController(IMessageBus bus, ILogger<BundlesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Get all content bundles
    /// </summary>
    /// <returns>List of content bundles</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContentBundle>>> GetBundles()
    {
        var query = new GetAllContentBundlesQuery();
        var bundles = await _bus.InvokeAsync<IEnumerable<ContentBundle>>(query);
        return Ok(bundles);
    }

    /// <summary>
    /// Get content bundles by age group
    /// </summary>
    /// <param name="ageGroup">Age group (e.g., "Ages7to9")</param>
    /// <returns>List of content bundles for the specified age group</returns>
    [HttpGet("age-group/{ageGroup}")]
    public async Task<ActionResult<IEnumerable<ContentBundle>>> GetBundlesByAgeGroup(string ageGroup)
    {
        try
        {
            var query = new GetContentBundlesByAgeGroupQuery(ageGroup);
            var bundles = await _bus.InvokeAsync<IEnumerable<ContentBundle>>(query);
            return Ok(bundles);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid age group parameter: {AgeGroup}", ageGroup);
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Get creator attribution/credits for a content bundle.
    /// Returns information about the content creators and Story Protocol registration status.
    /// </summary>
    /// <param name="id">The bundle ID</param>
    /// <returns>Attribution information including creator credits</returns>
    [HttpGet("{id}/attribution")]
    [ProducesResponseType(typeof(ContentAttributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContentAttributionResponse>> GetBundleAttribution(string id)
    {
        var query = new GetBundleAttributionQuery(id);
        var attribution = await _bus.InvokeAsync<ContentAttributionResponse?>(query);

        if (attribution == null)
        {
            return NotFound(new ErrorResponse
            {
                Message = $"Content bundle not found: {id}",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(attribution);
    }

    /// <summary>
    /// Get IP registration status for a content bundle.
    /// Returns Story Protocol blockchain registration verification details.
    /// </summary>
    /// <param name="id">The bundle ID</param>
    /// <returns>IP verification status including blockchain details</returns>
    [HttpGet("{id}/ip-status")]
    [ProducesResponseType(typeof(IpVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IpVerificationResponse>> GetBundleIpStatus(string id)
    {
        var query = new GetBundleIpStatusQuery(id);
        var ipStatus = await _bus.InvokeAsync<IpVerificationResponse?>(query);

        if (ipStatus == null)
        {
            return NotFound(new ErrorResponse
            {
                Message = $"Content bundle not found: {id}",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(ipStatus);
    }
}
