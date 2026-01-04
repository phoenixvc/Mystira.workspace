using Microsoft.AspNetCore.Mvc;
using Mystira.Application.CQRS.FantasyThemes.Commands;
using Mystira.Application.CQRS.FantasyThemes.Queries;
using Mystira.Domain.Models;
using Wolverine;

namespace Mystira.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class FantasyThemesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<FantasyThemesController> _logger;

    public FantasyThemesController(IMessageBus bus, ILogger<FantasyThemesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<FantasyThemeDefinition>>> GetAllFantasyThemes()
    {
        _logger.LogInformation("GET: Retrieving all fantasy themes");
        var fantasyThemes = await _bus.InvokeAsync<List<FantasyThemeDefinition>>(new GetAllFantasyThemesQuery());
        return Ok(fantasyThemes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FantasyThemeDefinition>> GetFantasyThemeById(string id)
    {
        _logger.LogInformation("GET: Retrieving fantasy theme with id: {Id}", id);
        var fantasyTheme = await _bus.InvokeAsync<FantasyThemeDefinition?>(new GetFantasyThemeByIdQuery(id));
        if (fantasyTheme == null)
        {
            _logger.LogWarning("Fantasy theme with id {Id} not found", id);
            return NotFound(new { message = "Fantasy theme not found" });
        }
        return Ok(fantasyTheme);
    }

    [HttpPost]
    public async Task<ActionResult<FantasyThemeDefinition>> CreateFantasyTheme([FromBody] CreateFantasyThemeRequest request)
    {
        _logger.LogInformation("POST: Creating fantasy theme with name: {Name}", request.Name);

        var created = await _bus.InvokeAsync<FantasyThemeDefinition>(new CreateFantasyThemeCommand(request.Name, request.Description));
        return CreatedAtAction(nameof(GetFantasyThemeById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FantasyThemeDefinition>> UpdateFantasyTheme(string id, [FromBody] UpdateFantasyThemeRequest request)
    {
        _logger.LogInformation("PUT: Updating fantasy theme with id: {Id}", id);

        var updated = await _bus.InvokeAsync<FantasyThemeDefinition?>(new UpdateFantasyThemeCommand(id, request.Name, request.Description));
        if (updated == null)
        {
            _logger.LogWarning("Fantasy theme with id {Id} not found", id);
            return NotFound(new { message = "Fantasy theme not found" });
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFantasyTheme(string id)
    {
        _logger.LogInformation("DELETE: Deleting fantasy theme with id: {Id}", id);

        var success = await _bus.InvokeAsync<bool>(new DeleteFantasyThemeCommand(id));
        if (!success)
        {
            _logger.LogWarning("Fantasy theme with id {Id} not found", id);
            return NotFound(new { message = "Fantasy theme not found" });
        }
        return NoContent();
    }
}

public class CreateFantasyThemeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateFantasyThemeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
