using Mystira.StoryGenerator.Contracts.Configuration;
using Microsoft.Extensions.Options;

namespace Mystira.StoryGenerator.Api.Services;

public interface IStorySchemaProvider
{
    Task<string?> GetSchemaJsonAsync(CancellationToken cancellationToken = default);
    bool IsStrict { get; }
}

/// <summary>
/// Default file-based implementation that reads the schema from the configured path.
/// </summary>
public class FileStorySchemaProvider : IStorySchemaProvider
{
    private readonly AiSettings _settings;
    private readonly ILogger<FileStorySchemaProvider> _logger;

    public FileStorySchemaProvider(IOptions<AiSettings> options, ILogger<FileStorySchemaProvider> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public bool IsStrict => _settings.AzureOpenAI.SchemaValidation.IsSchemaValidationStrict;

    public string? GetSchemaPath()
    {
        var configuredPath = _settings.AzureOpenAI.SchemaValidation.SchemaPath;
        var schemaPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, "config", "story-schema.json")
            : (Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(AppContext.BaseDirectory, configuredPath));
        return schemaPath;
    }

    public async Task<string?> GetSchemaJsonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schemaPath = GetSchemaPath();
            if (string.IsNullOrWhiteSpace(schemaPath) || !File.Exists(schemaPath))
            {
                _logger.LogWarning("Story schema file not found at: {SchemaPath}", schemaPath);
                return null;
            }
            return await File.ReadAllTextAsync(schemaPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read story schema file");
            return null;
        }
    }
}
