using System.ComponentModel.DataAnnotations;

namespace Mystira.Contracts.StoryGenerator.Configuration;

public class IntentRouterSettings
{
    public const string SectionName = "IntentRouter";

    public bool Enabled { get; set; } = false;

    public string? Provider { get; set; }

    public string? DeploymentName { get; set; }

    public string? ModelId { get; set; }

    [Range(0, 1)]
    public double Temperature { get; set; } = 0.1;

    [Range(1, 1000)]
    public int MaxTokens { get; set; } = 200;

    public bool IsConfigured =>
        Enabled &&
        !string.IsNullOrWhiteSpace(Provider) &&
        !string.IsNullOrWhiteSpace(ModelId);
}
