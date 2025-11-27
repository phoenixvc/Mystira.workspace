using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.RagIndexer.Models;

public class RagIndexRequest
{
    [JsonPropertyName("dataset")]
    public string Dataset { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("ageGroup")]
    public string? AgeGroup { get; set; }

    [JsonPropertyName("chunks")]
    public List<InstructionChunk> Chunks { get; set; } = new();
}

public class InstructionChunk
{
    [JsonPropertyName("chunk_id")]
    public string ChunkId { get; set; } = string.Empty;

    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("subcategory")]
    public string Subcategory { get; set; } = string.Empty;

    [JsonPropertyName("instructionType")]
    public string InstructionType { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; } = false;

    [JsonPropertyName("examples")]
    public string Examples { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}