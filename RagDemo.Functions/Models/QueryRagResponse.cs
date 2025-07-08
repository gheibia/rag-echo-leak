using System.Text.Json.Serialization;

namespace RagDemo.Functions.Models;

/// <summary>
/// Response model for RAG query operations
/// </summary>
public class QueryRagResponse
{
    /// <summary>
    /// The original user query
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Whether sensitive content was found in the results
    /// </summary>
    [JsonPropertyName("foundSensitiveContent")]
    public bool FoundSensitiveContent { get; set; }

    /// <summary>
    /// Search results
    /// </summary>
    [JsonPropertyName("results")]
    public List<SearchResultItem> Results { get; set; } = new();

    /// <summary>
    /// Number of results returned
    /// </summary>
    [JsonPropertyName("resultCount")]
    public int ResultCount => Results?.Count ?? 0;
}
