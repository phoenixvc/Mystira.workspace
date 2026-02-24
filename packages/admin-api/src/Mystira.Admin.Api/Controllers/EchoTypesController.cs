using Microsoft.AspNetCore.Mvc;

using Mystira.Application.CQRS.EchoTypes.Commands;
using Mystira.Application.CQRS.EchoTypes.Queries;
using Mystira.Domain.Models;

using Wolverine;

namespace Mystira.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class EchoTypesController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ILogger<EchoTypesController> _logger;

    public EchoTypesController(IMessageBus bus, ILogger<EchoTypesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<EchoTypeDefinition>>> GetAllEchoTypes()
    {
        _logger.LogInformation("GET: Retrieving all echo types");
        var echoTypes = await _bus.InvokeAsync<List<EchoTypeDefinition>>(new GetAllEchoTypesQuery());
        return Ok(echoTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EchoTypeDefinition>> GetEchoTypeById(string id)
    {
        _logger.LogInformation("GET: Retrieving echo type with id: {Id}", id);
        var echoType = await _bus.InvokeAsync<EchoTypeDefinition?>(new GetEchoTypeByIdQuery(id));
        if (echoType == null)
        {
            _logger.LogWarning("Echo type with id {Id} not found", id);
            return NotFound(new { message = "Echo type not found" });
        }
        return Ok(echoType);
    }

    [HttpPost]
    public async Task<ActionResult<EchoTypeDefinition>> CreateEchoType([FromBody] CreateEchoTypeRequest request)
    {
        _logger.LogInformation("POST: Creating echo type with name: {Name}", request.Name);

        var created = await _bus.InvokeAsync<EchoTypeDefinition>(new CreateEchoTypeCommand(request.Name, request.Description));
        return CreatedAtAction(nameof(GetEchoTypeById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EchoTypeDefinition>> UpdateEchoType(string id, [FromBody] UpdateEchoTypeRequest request)
    {
        _logger.LogInformation("PUT: Updating echo type with id: {Id}", id);

        var updated = await _bus.InvokeAsync<EchoTypeDefinition?>(new UpdateEchoTypeCommand(id, request.Name, request.Description));
        if (updated == null)
        {
            _logger.LogWarning("Echo type with id {Id} not found", id);
            return NotFound(new { message = "Echo type not found" });
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEchoType(string id)
    {
        _logger.LogInformation("DELETE: Deleting echo type with id: {Id}", id);

        var success = await _bus.InvokeAsync<bool>(new DeleteEchoTypeCommand(id));
        if (!success)
        {
            _logger.LogWarning("Echo type with id {Id} not found", id);
            return NotFound(new { message = "Echo type not found" });
        }
        return NoContent();
    }
}

public class CreateEchoTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateEchoTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
