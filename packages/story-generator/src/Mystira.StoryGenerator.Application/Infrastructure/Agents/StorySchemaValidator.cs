using System.Reflection;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

public sealed class StorySchemaValidator
{
    private const string SchemaResourceName = "Mystira.StoryGenerator.Application.Resources.FoundryStorySchema.json";

    private readonly Lazy<Task<JsonSchema>> _schema;

    public StorySchemaValidator()
    {
        _schema = new Lazy<Task<JsonSchema>>(LoadSchemaAsync);
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidateAsync(string storyJson, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(storyJson))
        {
            errors.Add("Story JSON was empty.");
            return (false, errors);
        }

        JsonSchema schema;
        try
        {
            schema = await _schema.Value.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            errors.Add($"Failed to load schema resource '{SchemaResourceName}': {ex.Message}");
            return (false, errors);
        }

        JToken storyToken;
        try
        {
            storyToken = JToken.Parse(storyJson);
        }
        catch (Exception ex)
        {
            errors.Add($"Invalid JSON: {ex.Message}");
            return (false, errors);
        }

        var validationErrors = schema.Validate(storyToken);
        foreach (var error in validationErrors)
        {
            var path = string.IsNullOrWhiteSpace(error.Path) ? "$" : error.Path;
            errors.Add($"{path}: {error.Kind} - {error}");
        }

        return (errors.Count == 0, errors);
    }

    private static async Task<JsonSchema> LoadSchemaAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream(SchemaResourceName)
            ?? throw new InvalidOperationException($"Embedded schema resource not found: {SchemaResourceName}");

        using var reader = new StreamReader(stream);
        var schemaJson = await reader.ReadToEndAsync().ConfigureAwait(false);

        return await JsonSchema.FromJsonAsync(schemaJson).ConfigureAwait(false);
    }
}
