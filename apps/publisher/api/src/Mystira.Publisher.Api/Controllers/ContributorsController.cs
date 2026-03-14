using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mystira.Publisher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContributorsController : ControllerBase
{
    [HttpGet("story/{storyId}")]
    public async Task<IActionResult> GetByStory(string storyId)
    {
        // TODO: Implement get contributors by story
        return Ok(new { message = $"Get contributors for story {storyId} - TODO: implement" });
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] object request)
    {
        // TODO: Implement add contributor
        return CreatedAtAction(nameof(GetByStory), new { storyId = "story-id" }, new { message = "Contributor added" });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] object request)
    {
        // TODO: Implement update contributor
        return Ok(new { message = $"Contributor {id} updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(string id)
    {
        // TODO: Implement remove contributor
        return NoContent();
    }

    [HttpPost("approve")]
    public async Task<IActionResult> SubmitApproval([FromBody] object request)
    {
        // TODO: Implement submit approval
        return Ok(new { message = "Approval submitted" });
    }

    [HttpPost("override")]
    public async Task<IActionResult> Override([FromBody] object request)
    {
        // TODO: Implement override non-responsive contributor
        return Ok(new { message = "Contributor overridden" });
    }

    [HttpGet("validate/{storyId}")]
    public async Task<IActionResult> ValidateSplits(string storyId)
    {
        // TODO: Implement validate royalty splits
        return Ok(new { valid = true, message = "Royalty splits valid" });
    }
}
