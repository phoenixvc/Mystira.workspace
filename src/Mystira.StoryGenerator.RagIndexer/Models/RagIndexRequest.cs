using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.RagIndexer.Models;

public class RagIndexRequest
{
    [JsonPropertyName("dataset")]
    public string Dataset { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

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
}