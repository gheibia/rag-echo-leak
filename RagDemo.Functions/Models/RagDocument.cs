using System.Text.Json.Serialization;

namespace RagDemo.Functions.Models;

/// <summary>
/// Represents a document to be indexed in Azure AI Search
/// </summary>
public class RagDocument
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    [JsonPropertyName("chunk_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the document
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content of the document
    /// </summary>
    [JsonPropertyName("chunk")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Source file path or identifier
    /// </summary>
    [JsonPropertyName("parent_id")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Embedding vector for the document content
    /// </summary>
    [JsonPropertyName("text_vector")]
    public float[]? ContentVector { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
