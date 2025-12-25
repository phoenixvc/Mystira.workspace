namespace Mystira.Contracts.StoryGenerator;

/// <summary>
/// Story Generator API Contracts
///
/// These types will be migrated from Mystira.StoryGenerator.Contracts.
/// During migration, types will be added here and deprecated in the old package.
///
/// Migration status: PENDING
/// See: docs/architecture/adr/0020-package-consolidation-strategy.md
/// </summary>

/// <summary>
/// Configuration for story generation
/// </summary>
/// <remarks>
/// Placeholder - will be replaced during migration from Mystira.StoryGenerator.Contracts
/// </remarks>
public record GeneratorConfig
{
    /// <summary>
    /// AI model to use for generation
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Maximum tokens for generation
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Temperature for generation creativity (0-1)
    /// </summary>
    public double? Temperature { get; init; }

    /// <summary>
    /// Additional model parameters
    /// </summary>
    public IDictionary<string, object>? Parameters { get; init; }
}

/// <summary>
/// Story generation request
/// </summary>
/// <remarks>
/// Placeholder - will be replaced during migration from Mystira.StoryGenerator.Contracts
/// </remarks>
public record GeneratorRequest
{
    /// <summary>
    /// Prompt or starting text
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Generator configuration
    /// </summary>
    public required GeneratorConfig Config { get; init; }

    /// <summary>
    /// Context from previous generations
    /// </summary>
    public GeneratorContext? Context { get; init; }
}

/// <summary>
/// Context for multi-turn generation
/// </summary>
public record GeneratorContext
{
    /// <summary>
    /// Previous generation IDs
    /// </summary>
    public IReadOnlyList<string>? PreviousGenerations { get; init; }

    /// <summary>
    /// Character definitions
    /// </summary>
    public IDictionary<string, object>? Characters { get; init; }

    /// <summary>
    /// World/setting context
    /// </summary>
    public string? WorldContext { get; init; }
}

/// <summary>
/// Result of story generation
/// </summary>
/// <remarks>
/// Placeholder - will be replaced during migration from Mystira.StoryGenerator.Contracts
/// </remarks>
public record GeneratorResult
{
    /// <summary>
    /// Unique generation ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Generated content
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Generation metadata
    /// </summary>
    public required GeneratorMetadata Metadata { get; init; }

    /// <summary>
    /// Whether generation completed successfully
    /// </summary>
    public required bool Success { get; init; }
}

/// <summary>
/// Metadata about a generation
/// </summary>
public record GeneratorMetadata
{
    /// <summary>
    /// Tokens used in generation
    /// </summary>
    public required int TokensUsed { get; init; }

    /// <summary>
    /// Model used
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Generation timestamp
    /// </summary>
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public required long DurationMs { get; init; }
}
