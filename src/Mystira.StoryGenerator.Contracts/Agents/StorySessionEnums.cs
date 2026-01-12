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
    /// Session has been created but not yet initialized.
    /// </summary>
    Uninitialized,

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
    /// Story has passed evaluation and is complete.
    /// </summary>
    Evaluated,

    /// <summary>
    /// User has requested refinements to the story.
    /// </summary>
    RefinementRequested,

    /// <summary>
    /// Story has been refined based on feedback and is being re-evaluated.
    /// </summary>
    Refined,

    /// <summary>
    /// Story has failed evaluation and requires user refinement.
    /// </summary>
    RequiresRefinement,

    /// <summary>
    /// Story is currently being refined based on feedback.
    /// </summary>
    Refining,

    /// <summary>
    /// Story generation is complete.
    /// </summary>
    Complete,

    /// <summary>
    /// Story generation has failed and needs attention.
    /// </summary>
    Failed,

    /// <summary>
    /// Story is stuck in refinement loop and needs human review.
    /// </summary>
    StuckNeedsReview
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
