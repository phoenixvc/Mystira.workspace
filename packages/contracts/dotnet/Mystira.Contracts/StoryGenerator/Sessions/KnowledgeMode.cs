using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Sessions;

/// <summary>
/// Specifies the knowledge retrieval mode for story generation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum KnowledgeMode
{
    /// <summary>
    /// No external knowledge retrieval.
    /// </summary>
    None,

    /// <summary>
    /// Use vector search for knowledge retrieval.
    /// </summary>
    VectorSearch,

    /// <summary>
    /// Use RAG (Retrieval Augmented Generation) for knowledge retrieval.
    /// </summary>
    Rag,

    /// <summary>
    /// Hybrid mode combining multiple retrieval strategies.
    /// </summary>
    Hybrid
}
