using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NJsonSchema.Generation;
using Newtonsoft.Json.Linq;
using Mystira.StoryGenerator.Domain.Stories;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;

namespace Mystira.StoryGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchemaController : ControllerBase
{
    private readonly ILogger<SchemaController> _logger;
    private readonly AiSettings _settings;

    public SchemaController(ILogger<SchemaController> logger, IOptions<AiSettings> aiOptions)
    {
        _logger = logger;
        _settings = aiOptions.Value;
    }

    [HttpGet("story")] // returns the file-based schema used for validating payloads
    public async Task<IActionResult> GetFileSchema()
    {
        try
        {
            var configuredPath = _settings.AzureOpenAI.SchemaValidation.SchemaPath;
            var schemaPath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(AppContext.BaseDirectory, "config", "story-schema.json")
                : (Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(AppContext.BaseDirectory, configuredPath));
            if (!System.IO.File.Exists(schemaPath))
            {
                return NotFound(new { error = "Schema file not found." });
            }

            var json = await System.IO.File.ReadAllTextAsync(schemaPath);
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading story schema file");
            return StatusCode(500, new { error = "Failed to load schema." });
        }
    }

    [HttpGet("generated")] // returns the schema generated from our C# Scenario class
    public async Task<IActionResult> GetGeneratedSchema()
    {
        try
        {
            // NJsonSchema v11 supports static generation via FromType<T>
            var schema = JsonSchema.FromType<Scenario>();
            var json = schema.ToJson();
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schema from Scenario type");
            return StatusCode(500, new { error = "Failed to generate schema." });
        }
    }

    [HttpGet("compare")] // compares file schema with generated class schema
    public async Task<IActionResult> CompareSchemas()
    {
        try
        {
            var configuredPath = _settings.AzureOpenAI.SchemaValidation.SchemaPath;
            var schemaPath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(AppContext.BaseDirectory, "config", "story-schema.json")
                : (Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(AppContext.BaseDirectory, configuredPath));
            if (!System.IO.File.Exists(schemaPath))
            {
                return NotFound(new { error = "Schema file not found." });
            }

            var fileJson = await System.IO.File.ReadAllTextAsync(schemaPath);
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
