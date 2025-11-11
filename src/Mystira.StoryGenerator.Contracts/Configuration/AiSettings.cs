using System.ComponentModel.DataAnnotations;

namespace Mystira.StoryGenerator.Contracts.Configuration;

public class AiSettings
{
    public const string SectionName = "Ai";

    [Required]
    public string DefaultProvider { get; set; } = string.Empty;

    [Range(0, 2)]
    public double DefaultTemperature { get; set; } = 0.7;

    [Range(1, 4096)]
    public int DefaultMaxTokens { get; set; } = 1000;

    public AzureOpenAISettings AzureOpenAI { get; set; } = new();
    public GoogleGeminiSettings GoogleGemini { get; set; } = new();
}

public class AzureOpenAISettings
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = string.Empty;
}

public class GoogleGeminiSettings
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = "gemini-pro";
}
