using System.Text.Json.Serialization;

namespace RagDemo.WebApp.Models;

// Search result item from the Functions API (without UI-specific fields)
public class FunctionSearchResultItem
{
    [JsonPropertyName("chunk_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("chunk")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("@search.score")]
    public double Score { get; set; }

    [JsonPropertyName("containsSensitiveContent")]
    public bool ContainsSensitiveContent { get; set; }
}
