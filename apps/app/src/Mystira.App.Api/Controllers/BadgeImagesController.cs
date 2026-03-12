using Wolverine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mystira.Core.CQRS.Badges.Queries;
using Mystira.Contracts.App.Responses.Common;

namespace Mystira.App.Api.Controllers;

[ApiController]
[Route("api/badges/images")]
public class BadgeImagesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<BadgeImagesController> _logger;

    public BadgeImagesController(IMessageBus bus, ILogger<BadgeImagesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    [HttpGet("{imageId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBadgeImage(string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "imageId is required",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var result = await _bus.InvokeAsync<BadgeImageResult?>(new GetBadgeImageQuery(imageId));

        if (result is null)
        {
            return NotFound(new ErrorResponse
            {
                Message = "Badge image not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        Response.Headers.CacheControl = "public, max-age=604800";
        return File(result.ImageData, result.ContentType);
    }
}
