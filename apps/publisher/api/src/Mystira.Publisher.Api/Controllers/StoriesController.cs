using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mystira.Publisher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoriesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        // TODO: Implement story listing
        return Ok(new { message = "Stories list endpoint - TODO: implement" });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        // TODO: Implement get story by ID
        return Ok(new { message = $"Get story {id} - TODO: implement" });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object request)
    {
        // TODO: Implement create story
        return CreatedAtAction(nameof(Get), new { id = "new-story-id" }, new { message = "Story created" });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] object request)
    {
        // TODO: Implement update story
        return Ok(new { message = $"Story {id} updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        // TODO: Implement delete story
        return NoContent();
    }
}
