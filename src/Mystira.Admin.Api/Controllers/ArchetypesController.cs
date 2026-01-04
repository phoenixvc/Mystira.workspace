using Microsoft.AspNetCore.Mvc;

using Mystira.Application.CQRS.Archetypes.Commands;
using Mystira.Application.CQRS.Archetypes.Queries;
using Mystira.Domain.Models;

using Wolverine;

namespace Mystira.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class ArchetypesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<ArchetypesController> _logger;

    public ArchetypesController(IMessageBus bus, ILogger<ArchetypesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ArchetypeDefinition>>> GetAllArchetypes()
    {
        _logger.LogInformation("GET: Retrieving all archetypes");
        var archetypes = await _bus.InvokeAsync<List<ArchetypeDefinition>>(new GetAllArchetypesQuery());
        return Ok(archetypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArchetypeDefinition>> GetArchetypeById(string id)
    {
        _logger.LogInformation("GET: Retrieving archetype with id: {Id}", id);
        var archetype = await _bus.InvokeAsync<ArchetypeDefinition?>(new GetArchetypeByIdQuery(id));
        if (archetype == null)
        {
            _logger.LogWarning("Archetype with id {Id} not found", id);
            return NotFound(new { message = "Archetype not found" });
        }
        return Ok(archetype);
    }

    [HttpPost]
    public async Task<ActionResult<ArchetypeDefinition>> CreateArchetype([FromBody] CreateArchetypeRequest request)
    {
        _logger.LogInformation("POST: Creating archetype with name: {Name}", request.Name);

        var created = await _bus.InvokeAsync<ArchetypeDefinition>(new CreateArchetypeCommand(request.Name, request.Description));
        return CreatedAtAction(nameof(GetArchetypeById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ArchetypeDefinition>> UpdateArchetype(string id, [FromBody] UpdateArchetypeRequest request)
    {
        _logger.LogInformation("PUT: Updating archetype with id: {Id}", id);

        var updated = await _bus.InvokeAsync<ArchetypeDefinition?>(new UpdateArchetypeCommand(id, request.Name, request.Description));
        if (updated == null)
        {
            _logger.LogWarning("Archetype with id {Id} not found", id);
            return NotFound(new { message = "Archetype not found" });
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArchetype(string id)
    {
        _logger.LogInformation("DELETE: Deleting archetype with id: {Id}", id);

        var success = await _bus.InvokeAsync<bool>(new DeleteArchetypeCommand(id));
        if (!success)
        {
            _logger.LogWarning("Archetype with id {Id} not found", id);
            return NotFound(new { message = "Archetype not found" });
        }
        return NoContent();
    }
}

public class CreateArchetypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateArchetypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
