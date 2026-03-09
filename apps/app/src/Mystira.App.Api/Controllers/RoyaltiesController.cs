using Wolverine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mystira.App.Application.CQRS.Royalties.Commands;
using Mystira.App.Application.CQRS.Royalties.Queries;
using Mystira.Contracts.App.Requests.Royalties;
using Mystira.App.Api.Models;
using Mystira.App.Domain.Models;

namespace Mystira.App.Api.Controllers;

/// <summary>
/// Controller for Story Protocol royalty operations.
/// Provides endpoints for viewing and claiming royalties.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RoyaltiesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<RoyaltiesController> _logger;

    public RoyaltiesController(IMessageBus bus, ILogger<RoyaltiesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }


    /// Get claimable royalties for an IP Asset
    /// </summary>
    /// <param name="ipAssetId">The Story Protocol IP Asset ID</param>
    /// <returns>Claimable royalty balance information</returns>
    [HttpGet("{ipAssetId}/balance")]
    [ProducesResponseType(typeof(RoyaltyBalance), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoyaltyBalance>> GetClaimableRoyalties(string ipAssetId)
    {
        if (string.IsNullOrWhiteSpace(ipAssetId))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "IP Asset ID is required",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            var query = new GetClaimableRoyaltiesQuery(ipAssetId);
            var balance = await _bus.InvokeAsync<RoyaltyBalance>(query);

            return Ok(balance);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Invalid argument when getting claimable royalties for IP Asset {IpAssetId}", ipAssetId);
            return BadRequest(new ErrorResponse
            {
                Message = "Invalid IP Asset ID",
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (InvalidOperationException invOpEx)
        {
            _logger.LogWarning(invOpEx, "Invalid operation when getting claimable royalties for IP Asset {IpAssetId}", ipAssetId);
            return StatusCode(500, new ErrorResponse
            {
                Message = "Operation failed while fetching royalty balance",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    /// <summary>
    /// Pay royalties to an IP Asset. Requires authentication.
    /// </summary>
    /// <param name="ipAssetId">The Story Protocol IP Asset ID</param>
    /// <param name="request">Payment details</param>
    /// <returns>Payment result including transaction hash</returns>
    [HttpPost("{ipAssetId}/pay")]
    [Authorize]
    [ProducesResponseType(typeof(RoyaltyPaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoyaltyPaymentResult>> PayRoyalty(
        string ipAssetId,
        [FromBody] PayRoyaltyRequest request)
    {
        if (string.IsNullOrWhiteSpace(ipAssetId))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "IP Asset ID is required",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Amount must be greater than zero",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            var command = new PayRoyaltyCommand(ipAssetId, request.Amount, request.PayerReference);
            var result = await _bus.InvokeAsync<RoyaltyPaymentResult>(command);

            if (!result.Success)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = result.ErrorMessage ?? "Payment failed",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(result);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Invalid argument when paying royalty to IP Asset {IpAssetId}", ipAssetId);
            return BadRequest(new ErrorResponse
            {
                Message = argEx.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (InvalidOperationException invOpEx)
        {
            _logger.LogWarning(invOpEx, "Invalid operation when paying royalty to IP Asset {IpAssetId}", ipAssetId);
            return BadRequest(new ErrorResponse
            {
                Message = invOpEx.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }


    /// Claim accumulated royalties for a contributor wallet. Requires authentication.
    /// </summary>
    /// <param name="ipAssetId">The Story Protocol IP Asset ID</param>
    /// <param name="request">Claim details including wallet address</param>
    /// <returns>Transaction hash of the claim</returns>
    [HttpPost("{ipAssetId}/claim")]
    [Authorize]
    [ProducesResponseType(typeof(ClaimRoyaltiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClaimRoyaltiesResponse>> ClaimRoyalties(
        string ipAssetId,
        [FromBody] ClaimRoyaltiesRequest request)
    {
        if (string.IsNullOrWhiteSpace(ipAssetId))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "IP Asset ID is required",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (string.IsNullOrWhiteSpace(request.ContributorWallet))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Contributor wallet address is required",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            var command = new ClaimRoyaltiesCommand(ipAssetId, request.ContributorWallet);
            var txHash = await _bus.InvokeAsync<string>(command);

            return Ok(new ClaimRoyaltiesResponse
            {
                IpAssetId = ipAssetId,
                ContributorWallet = request.ContributorWallet,
                TransactionHash = txHash,
                ClaimedAt = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid claim request for IP Asset {IpAssetId}", ipAssetId);
            return BadRequest(new ErrorResponse
            {
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}

/// <summary>
/// Response for claim royalties operation
/// </summary>
public class ClaimRoyaltiesResponse
{
    /// <summary>
    /// The IP Asset ID
    /// </summary>
    public string IpAssetId { get; set; } = string.Empty;

    /// <summary>
    /// The wallet that claimed the royalties
    /// </summary>
    public string ContributorWallet { get; set; } = string.Empty;

    /// <summary>
    /// Transaction hash of the claim
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;

    /// <summary>
    /// When the claim was processed
    /// </summary>
    public DateTime ClaimedAt { get; set; }
}
