namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// User-facing rubric summary for a story iteration.
/// </summary>
public class RubricSummary
{
    /// <summary>
    /// Combined narrative summary of the story's quality (150 words max).
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// List of story strengths.
    /// </summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// List of concerns or areas needing improvement.
    /// </summary>
    public List<string> Concerns { get; set; } = new();

    /// <summary>
    /// Suggested areas to focus on for refinement.
    /// </summary>
    public List<string> SuggestedFocus { get; set; } = new();

    /// <summary>
    /// Whether the story is ready for publication.
    /// </summary>
    public bool ReadyForPublish { get; set; }
}
