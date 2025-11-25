using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Web.Models;

public class AiModelConfiguration
{
    [JsonPropertyName("defaultModelId")]
    public string? DefaultModelId { get; set; }

    [JsonPropertyName("models")]
    public List<AiModelDefinition> Models { get; set; } = new();
}

public class AiModelDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("deployment")]
    public string? Deployment { get; set; }

    [JsonPropertyName("defaultMaxTokens")]
    public int DefaultMaxTokens { get; set; } = 1500;

    [JsonPropertyName("maxTokensLimit")]
    public int? MaxTokensLimit { get; set; }

    [JsonPropertyName("minTokens")]
    public int? MinTokens { get; set; }

    [JsonPropertyName("defaultTemperature")]
    public double DefaultTemperature { get; set; } = 0.7;

    [JsonPropertyName("minTemperature")]
    public double? MinTemperature { get; set; }

    [JsonPropertyName("maxTemperature")]
    public double? MaxTemperature { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class AiModelSelection
{
    public string ModelId { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public double Temperature { get; set; }
}

public class EffectiveAiModelSettings
{
    public string ModelId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Deployment { get; init; }
    public int MaxTokens { get; init; }
    public double Temperature { get; init; }
    public AiModelDefinition Definition { get; init; } = new();
}

public static class AiModelDefaults
{
    public const string DefaultProvider = "azure-openai";

    public static AiModelConfiguration CreateConfiguration()
    {
        return new AiModelConfiguration
        {
            DefaultModelId = "gpt-4.1",
            Models = new List<AiModelDefinition>
            {
                new()
                {
                    Id = "gpt-4.1",
                    DisplayName = "GPT-4.1",
                    Provider = DefaultProvider,
                    Deployment = "gpt-4.1",
                    DefaultMaxTokens = 2000,
                    MaxTokensLimit = 12000,
                    DefaultTemperature = 0.7,
                    MinTemperature = 0.0,
                    MaxTemperature = 1.0
                }
            }
        };
    }

    public static EffectiveAiModelSettings CreateEffectiveSettings()
    {
        var configuration = CreateConfiguration();
        var defaultModel = ResolveDefaultModel(configuration);
        return new EffectiveAiModelSettings
        {
            ModelId = defaultModel.Id,
            Provider = defaultModel.Provider,
            Deployment = defaultModel.Deployment,
            MaxTokens = defaultModel.DefaultMaxTokens,
            Temperature = defaultModel.DefaultTemperature,
            Definition = defaultModel
        };
    }

    public static AiModelDefinition ResolveDefaultModel(AiModelConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.DefaultModelId))
        {
            var match = configuration.Models.FirstOrDefault(m => string.Equals(m.Id, configuration.DefaultModelId, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        return configuration.Models.FirstOrDefault() ?? new AiModelDefinition
        {
            Id = "gpt-4.1",
            DisplayName = "GPT-4.1",
            Provider = DefaultProvider,
            Deployment = "gpt-4.1",
            DefaultMaxTokens = 2000,
            MaxTokensLimit = 4000,
            DefaultTemperature = 0.7,
            MinTemperature = 0.0,
            MaxTemperature = 1.0
        };
    }
}
