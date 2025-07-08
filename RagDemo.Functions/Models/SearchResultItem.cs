using System.Text.Json.Serialization;

namespace RagDemo.Functions.Models;

/// <summary>
/// Represents a search result item returned from Azure AI Search
/// </summary>
public class SearchResultItem
{
    /// <summary>
    /// Document ID
    /// </summary>
    [JsonPropertyName("chunk_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Document content/chunk
    /// </summary>
    [JsonPropertyName("chunk")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Search relevance score
    /// </summary>
    [JsonPropertyName("@search.score")]
    public double Score { get; set; }

    /// <summary>
    /// Whether the content contains sensitive information
    /// </summary>
    [JsonPropertyName("containsSensitiveContent")]
    public bool ContainsSensitiveContent { get; set; }
}
