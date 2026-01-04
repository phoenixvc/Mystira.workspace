using Microsoft.AspNetCore.Mvc;

using Mystira.Application.CQRS.CompassAxes.Commands;
using Mystira.Application.CQRS.CompassAxes.Queries;
using Mystira.Domain.Models;
using Mystira.Domain.ValueObjects;

using Wolverine;

namespace Mystira.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class CompassAxesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<CompassAxesController> _logger;

    public CompassAxesController(IMessageBus bus, ILogger<CompassAxesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompassAxisDefinition>>> GetAllCompassAxes()
    {
        _logger.LogInformation("GET: Retrieving all compass axes");
        var axes = await _bus.InvokeAsync<List<CompassAxisDefinition>>(new GetAllCompassAxesQuery());
        return Ok(axes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompassAxisDefinition>> GetCompassAxisById(string id)
    {
        _logger.LogInformation("GET: Retrieving compass axis with id: {Id}", id);
        var axis = await _bus.InvokeAsync<CompassAxisDefinition?>(new GetCompassAxisByIdQuery(id));
        if (axis == null)
        {
            _logger.LogWarning("Compass axis with id {Id} not found", id);
            return NotFound(new { message = "Compass axis not found" });
        }
        return Ok(axis);
    }

    [HttpPost]
    public async Task<ActionResult<CompassAxisDefinition>> CreateCompassAxis([FromBody] CreateCompassAxisRequest request)
    {
        _logger.LogInformation("POST: Creating compass axis with name: {Name}", request.Name);

        var created = await _bus.InvokeAsync<CompassAxisDefinition>(new CreateCompassAxisCommand(request.Name, request.Description));
        return CreatedAtAction(nameof(GetCompassAxisById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CompassAxisDefinition>> UpdateCompassAxis(string id, [FromBody] UpdateCompassAxisRequest request)
    {
        _logger.LogInformation("PUT: Updating compass axis with id: {Id}", id);

        var updated = await _bus.InvokeAsync<CompassAxisDefinition?>(new UpdateCompassAxisCommand(id, request.Name, request.Description));
        if (updated == null)
        {
            _logger.LogWarning("Compass axis with id {Id} not found", id);
            return NotFound(new { message = "Compass axis not found" });
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompassAxis(string id)
    {
        _logger.LogInformation("DELETE: Deleting compass axis with id: {Id}", id);

        var success = await _bus.InvokeAsync<bool>(new DeleteCompassAxisCommand(id));
        if (!success)
        {
            _logger.LogWarning("Compass axis with id {Id} not found", id);
            return NotFound(new { message = "Compass axis not found" });
        }
        return NoContent();
    }
}

public class CreateCompassAxisRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateCompassAxisRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
