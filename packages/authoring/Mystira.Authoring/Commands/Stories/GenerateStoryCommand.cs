using Mystira.Authoring.Abstractions.Commands;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Authoring.Commands.Stories;

/// <summary>
/// Command to generate a story based on user input.
/// </summary>
public class GenerateStoryCommand : ICommand<GenerateStoryResponse>
{
    /// <summary>
    /// The generation request parameters.
    /// </summary>
    public GenerateStoryRequest Request { get; set; } = new();

    /// <summary>
    /// The user's query/prompt for story generation.
    /// </summary>
    public string? UserQuery { get; set; }

    /// <summary>
    /// Chat history for context.
    /// </summary>
    public List<MystiraChatMessage> History { get; set; } = new();
}

/// <summary>
/// Request parameters for story generation.
/// </summary>
public class GenerateStoryRequest
{
    /// <summary>
    /// Story title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Story description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Difficulty level (Easy, Medium, Hard).
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// Session length (Short, Medium, Long).
    /// </summary>
    public string? SessionLength { get; set; }

    /// <summary>
    /// Target age group.
    /// </summary>
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Minimum recommended age.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Core developmental axes.
    /// </summary>
    public List<string> CoreAxes { get; set; } = new();

    /// <summary>
    /// Character archetypes.
    /// </summary>
    public List<string> Archetypes { get; set; } = new();

    /// <summary>
    /// Story tags.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Story tone.
    /// </summary>
    public string? Tone { get; set; }

    /// <summary>
    /// Minimum number of scenes.
    /// </summary>
    public int MinScenes { get; set; } = 5;

    /// <summary>
    /// Maximum number of scenes.
    /// </summary>
    public int MaxScenes { get; set; } = 15;

    /// <summary>
    /// Number of characters.
    /// </summary>
    public int CharacterCount { get; set; } = 3;

    /// <summary>
    /// AI provider to use.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Model ID to use.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Model name to use.
    /// </summary>
    public string? Model { get; set; }
}

/// <summary>
/// Response from story generation.
/// </summary>
public class GenerateStoryResponse
{
    /// <summary>
    /// Whether generation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Generated story JSON.
    /// </summary>
    public string? Json { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Provider that was used.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Model that was used.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Model ID that was used.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Whether the response was truncated.
    /// </summary>
    public bool IsIncomplete { get; set; }
}
