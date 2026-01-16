namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Summary from the rubric agent evaluating story readiness for publication.
/// </summary>
public class RubricSummary
{
    /// <summary>
    /// Overall summary of the story evaluation.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Whether the story is ready for publication.
    /// </summary>
    public bool ReadyForPublish { get; set; }

    /// <summary>
    /// List of story strengths identified by the rubric agent.
    /// </summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// List of concerns or issues identified by the rubric agent.
    /// </summary>
    public List<string> Concerns { get; set; } = new();

    /// <summary>
    /// Suggested areas to focus on for improvement.
    /// </summary>
    public List<string> SuggestedFocus { get; set; } = new();
}
