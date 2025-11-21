namespace Mystira.StoryGenerator.Api.Services.Instructions;

public class InstructionChunk
{
    public string? Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? InstructionType { get; set; }
    public bool IsMandatory { get; set; }
    // Legacy ordering support (kept for compatibility with existing merge/sort logic)
    public int? Order { get; set; }
    // New priority field from index schema
    public int? Priority { get; set; }
    public double? Score { get; set; }
    // Legacy tags support
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    // New metadata fields from index schema
    public string? Source { get; set; }
    public string? Version { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Section { get; set; }
    public string? Dataset { get; set; }
    public IReadOnlyList<string> Keywords { get; set; } = Array.Empty<string>();
}
