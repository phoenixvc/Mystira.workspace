using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mystira.Publisher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int limit = 10)
    {
        // TODO: Implement user search
        return Ok(new[] { new { id = "user-1", name = "Example User", email = "user@example.com" } });
    }
}
