namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Represents the knowledge retrieval mode for the story generation agent.
/// </summary>
public enum KnowledgeMode
{
    /// <summary>
    /// Uses File Search with vector stores for knowledge retrieval.
    /// </summary>
    FileSearch,

    /// <summary>
    /// Uses Azure AI Search for knowledge retrieval.
    /// </summary>
    AISearch
}

/// <summary>
/// Represents the current stage of a story generation session.
/// </summary>
public enum StorySessionStage
{
    /// <summary>
    /// Initial story generation in progress.
    /// </summary>
    Generating,

    /// <summary>
    /// Validating the generated story against schema and rules.
    /// </summary>
    Validating,

    /// <summary>
    /// Evaluating story consistency and quality.
    /// </summary>
    Evaluating,

    /// <summary>
    /// User has requested refinements to the story.
    /// </summary>
    RefinementRequested,

    /// <summary>
    /// Story has been refined based on feedback.
    /// </summary>
    Refined,

    /// <summary>
    /// Story generation is complete.
    /// </summary>
    Complete
}

/// <summary>
/// Overall evaluation status for a story.
/// </summary>
public enum EvaluationStatus
{
    /// <summary>
    /// The story passed all evaluations.
    /// </summary>
    Pass,

    /// <summary>
    /// The story failed one or more evaluations.
    /// </summary>
    Fail,

    /// <summary>
    /// The story requires human review.
    /// </summary>
    ReviewRequired
}
