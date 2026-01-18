namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for managing and retrieving instruction blocks for LLM prompts.
/// </summary>
public interface IInstructionBlockService
{
    /// <summary>
    /// Gets instruction blocks for the specified context.
    /// </summary>
    /// <param name="context">The search context for retrieving instructions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of instruction blocks.</returns>
    Task<IReadOnlyList<InstructionBlock>> GetInstructionsAsync(
        InstructionSearchContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific instruction block by ID.
    /// </summary>
    /// <param name="blockId">The instruction block ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The instruction block, or null if not found.</returns>
    Task<InstructionBlock?> GetByIdAsync(
        string blockId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all instruction blocks of a specific type.
    /// </summary>
    /// <param name="blockType">The block type to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of matching instruction blocks.</returns>
    Task<IReadOnlyList<InstructionBlock>> GetByTypeAsync(
        string blockType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes a full prompt from multiple instruction blocks.
    /// </summary>
    /// <param name="blockIds">The IDs of blocks to compose.</param>
    /// <param name="variables">Variables to interpolate into the prompt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The composed prompt.</returns>
    Task<string> ComposePromptAsync(
        IEnumerable<string> blockIds,
        Dictionary<string, string>? variables = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a reusable instruction block for LLM prompts.
/// </summary>
public class InstructionBlock
{
    /// <summary>
    /// Unique identifier for the block.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of instruction block.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Name of the block.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The instruction content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Priority for ordering (higher = earlier in prompt).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Tags for filtering and categorization.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Variables that can be interpolated into the content.
    /// </summary>
    public List<string> Variables { get; set; } = new();
}

/// <summary>
/// Context for searching instruction blocks.
/// </summary>
public class InstructionSearchContext
{
    /// <summary>
    /// Intent category for filtering.
    /// </summary>
    public string? IntentCategory { get; set; }

    /// <summary>
    /// Specific instruction types to include.
    /// </summary>
    public List<string>? InstructionTypes { get; set; }

    /// <summary>
    /// Tags to filter by.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Maximum number of blocks to return.
    /// </summary>
    public int? MaxBlocks { get; set; }

    /// <summary>
    /// Whether to include optional blocks.
    /// </summary>
    public bool IncludeOptional { get; set; } = true;
}
