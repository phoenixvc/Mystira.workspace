using Wolverine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mystira.Core.CQRS.MediaAssets.Queries;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using ErrorResponse = Mystira.Contracts.App.Responses.Common.ErrorResponse;

namespace Mystira.App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MediaController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMessageBus bus, ILogger<MediaController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Gets a specific media asset metadata by ID
    /// </summary>
    [HttpGet("{mediaId}/info")]
    [AllowAnonymous]
    public async Task<ActionResult<MediaAsset>> GetMediaById(string mediaId)
    {
        var query = new GetMediaAssetQuery(mediaId);
        var media = await _bus.InvokeAsync<MediaAsset?>(query);
        if (media == null)
        {
            return NotFound(new ErrorResponse
            {
                Message = $"Media not found: {mediaId}",
                TraceId = HttpContext.TraceIdentifier
            });
        }
        return Ok(media);
    }

    /// <summary>
    /// Serves the actual media file content by ID
    /// </summary>
    [HttpGet("{mediaId}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetMediaFile(string mediaId)
    {
        var query = new GetMediaFileQuery(mediaId);
        var result = await _bus.InvokeAsync<(Stream, string, string)?>(query);

        if (result == null)
        {
            return NotFound(new ErrorResponse
            {
                Message = $"Media file not found: {mediaId}",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var (stream, contentType, fileName) = result.Value;
        return File(stream, contentType, fileName);
    }
}
