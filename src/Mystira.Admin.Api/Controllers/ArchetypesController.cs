using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mystira.Application.CQRS.Archetypes.Commands;
using Mystira.Application.CQRS.Archetypes.Queries;
using Mystira.Domain.Models;

namespace Mystira.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class ArchetypesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ArchetypesController> _logger;

    public ArchetypesController(IMediator mediator, ILogger<ArchetypesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ArchetypeDefinition>>> GetAllArchetypes()
    {
        _logger.LogInformation("GET: Retrieving all archetypes");
        var archetypes = await _mediator.Send(new GetAllArchetypesQuery());
        return Ok(archetypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArchetypeDefinition>> GetArchetypeById(string id)
    {
        _logger.LogInformation("GET: Retrieving archetype with id: {Id}", id);
        var archetype = await _mediator.Send(new GetArchetypeByIdQuery(id));
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

        var created = await _mediator.Send(new CreateArchetypeCommand(request.Name, request.Description)) as ArchetypeDefinition;
        if (created == null)
        {
            _logger.LogError("Failed to create archetype - mediator returned unexpected type");
            return StatusCode(500, new { message = "Internal server error" });
        }
        return CreatedAtAction(nameof(GetArchetypeById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ArchetypeDefinition>> UpdateArchetype(string id, [FromBody] UpdateArchetypeRequest request)
    {
        _logger.LogInformation("PUT: Updating archetype with id: {Id}", id);

        var updated = await _mediator.Send(new UpdateArchetypeCommand(id, request.Name, request.Description));
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

        var result = await _mediator.Send(new DeleteArchetypeCommand(id));
        var success = (bool)result;
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
