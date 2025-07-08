using System.Text.Json.Serialization;

namespace RagDemo.WebApp.Models;

// Response model from the Functions API
public class QueryRagResponse
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("foundSensitiveContent")]
    public bool FoundSensitiveContent { get; set; }

    [JsonPropertyName("results")]
    public List<FunctionSearchResultItem> Results { get; set; } = new();

    [JsonPropertyName("resultCount")]
    public int ResultCount => Results?.Count ?? 0;
}
