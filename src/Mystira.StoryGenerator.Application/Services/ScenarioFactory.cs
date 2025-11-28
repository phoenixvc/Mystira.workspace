using System.Text.Json;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mystira.StoryGenerator.Application.Services;

/// <summary>
/// Application-layer implementation for creating Scenarios from JSON or YAML.
/// </summary>
public class ScenarioFactory : IScenarioFactory
{
    public Task<Scenario> CreateFromContentAsync(string content, ScenarioContentFormat format, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is null or empty", nameof(content));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return Task.FromResult(format switch
            {
                ScenarioContentFormat.Json => FromJson(content),
                ScenarioContentFormat.Yaml => FromYaml(content),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported format")
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Scenario from {format} content: {ex.Message}", ex);
        }
    }

    private static Scenario FromJson(string json)
    {
        var scenario = JsonSerializer.Deserialize<Scenario>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        if (scenario == null)
            throw new JsonException("Deserialized scenario is null");

        return scenario;
    }

    private static Scenario FromYaml(string yaml)
    {
        // YAML uses snake_case keys (e.g., next_scene)
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var scenario = deserializer.Deserialize<Scenario>(yaml);
        if (scenario == null)
            throw new InvalidOperationException("Deserialized YAML scenario is null");

        return scenario;
    }
}
