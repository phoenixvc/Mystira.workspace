using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Intent;

public enum IntentCategory
{
    [JsonPropertyName("story_generation")]
    StoryGeneration,

    [JsonPropertyName("validation")]
    Validation,

    [JsonPropertyName("autofix")]
    Autofix,

    [JsonPropertyName("summarization")]
    Summarization,

    [JsonPropertyName("config")]
    Config,

    [JsonPropertyName("safety")]
    Safety,

    [JsonPropertyName("meta")]
    Meta
}

public enum IntentInstructionType
{
    [JsonPropertyName("story_generate_initial")]
    StoryGenerateInitial,

    [JsonPropertyName("story_generate_refine")]
    StoryGenerateRefine,

    [JsonPropertyName("story_validate")]
    StoryValidate,

    [JsonPropertyName("story_autofix")]
    StoryAutofix,

    [JsonPropertyName("story_summarize")]
    StorySummarize,

    [JsonPropertyName("config_view")]
    ConfigView,

    [JsonPropertyName("config_update")]
    ConfigUpdate,

    [JsonPropertyName("help")]
    Help,

    [JsonPropertyName("schema_docs")]
    SchemaDocs,

    [JsonPropertyName("safety_policy")]
    SafetyPolicy,

    [JsonPropertyName("requirements")]
    Requirements,

    [JsonPropertyName("guidelines")]
    Guidelines
}

public class IntentClassification
{
    public string[] Categories { get; set; } = [string.Empty];
    public string[] InstructionTypes { get; set; } = [string.Empty];
}
