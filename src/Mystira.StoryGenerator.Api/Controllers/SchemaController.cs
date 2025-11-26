using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using Newtonsoft.Json.Linq;
using Mystira.StoryGenerator.Domain.Stories;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchemaController : ControllerBase
{
    private readonly ILogger<SchemaController> _logger;
    private readonly AiSettings _settings;
    private readonly IStorySchemaProvider _schemaProvider;

    public SchemaController(ILogger<SchemaController> logger, IOptions<AiSettings> aiOptions, IStorySchemaProvider schemaProvider)
    {
        _logger = logger;
        _settings = aiOptions.Value;
        _schemaProvider = schemaProvider;
    }

    [HttpGet("story")] // returns the file-based schema used for validating payloads
    public async Task<IActionResult> GetFileSchema()
    {
        try
        {
            var json = await _schemaProvider.GetSchemaJsonAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return NotFound(new { error = "Schema file not found." });
            }
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading story schema file");
            return StatusCode(500, new { error = "Failed to load schema." });
        }
    }

    [HttpGet("generated")] // returns the schema generated from our C# Scenario class
    public Task<IActionResult> GetGeneratedSchema()
    {
        try
        {
            // NJsonSchema v11 supports static generation via FromType<T>
            var schema = JsonSchema.FromType<Scenario>();
            var json = schema.ToJson();
            return Task.FromResult<IActionResult>(Content(json, "application/json"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schema from Scenario type");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to generate schema." }));
        }
    }

    [HttpGet("compare")] // compares file schema with generated class schema
    public async Task<IActionResult> CompareSchemas()
    {
        try
        {
            var fileJson = await _schemaProvider.GetSchemaJsonAsync();
            if (string.IsNullOrWhiteSpace(fileJson))
            {
                return NotFound(new { error = "Schema file not found." });
            }
            var generated = JsonSchema.FromType<Scenario>();
            var generatedJson = generated.ToJson();

            var fileToken = JToken.Parse(fileJson);
            var genToken = JToken.Parse(generatedJson);

            var equal = JToken.DeepEquals(fileToken, genToken);

            return Ok(new
            {
                equal,
                fileSchema = fileToken,
                generatedSchema = genToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing schemas");
            return StatusCode(500, new { error = "Failed to compare schemas." });
        }
    }
}
