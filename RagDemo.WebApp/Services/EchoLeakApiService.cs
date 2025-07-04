using RagDemo.WebApp.Models;
using System.Text;
using System.Text.Json;

namespace RagDemo.WebApp.Services;

public interface IEchoLeakApiService
{
    Task<EchoLeakTestResult> TestEchoLeakAsync(string query);
    Task<string> IndexDocumentsAsync();
}

public class EchoLeakApiService : IEchoLeakApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EchoLeakApiService> _logger;

    public EchoLeakApiService(HttpClient httpClient, IConfiguration configuration, ILogger<EchoLeakApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EchoLeakTestResult> TestEchoLeakAsync(string query)
    {
        try
        {
            var azureFunctionsBaseUrl = _configuration["AzureFunctions:BaseUrl"] ?? "http://localhost:7071";

            // First generate embeddings
            var embeddingRequest = new { Text = query };
            var embeddingJson = JsonSerializer.Serialize(embeddingRequest);
            var embeddingContent = new StringContent(embeddingJson, Encoding.UTF8, "application/json");

            var embeddingResponse = await _httpClient.PostAsync($"{azureFunctionsBaseUrl}/api/QueryRag", embeddingContent);
            embeddingResponse.EnsureSuccessStatusCode();

            var embeddingResult = await embeddingResponse.Content.ReadAsStringAsync();
            var embeddingData = JsonSerializer.Deserialize<EmbeddingResponse>(embeddingResult);

            // Now search for documents using the embeddings
            var searchResults = await SearchDocumentsAsync(embeddingData?.vector ?? Array.Empty<float>(), query);

            // Process results
            var result = new EchoLeakTestResult
            {
                Query = query,
                FoundSensitiveContent = false,
                Results = new List<SearchResultItem>()
            };

            foreach (var searchResult in searchResults)
            {
                var item = new SearchResultItem
                {
                    Title = searchResult.Title,
                    Content = searchResult.Content,
                    Score = searchResult.Score,
                    RenderedMarkdown = RenderMarkdown(searchResult.Content)
                };

                // Check if content contains sensitive information
                if (ContainsSensitiveContent(searchResult.Content))
                {
                    result.FoundSensitiveContent = true;
                    item.ContainsSensitiveContent = true;
                }

                result.Results.Add(item);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing EchoLeak vulnerability");
            throw;
        }
    }

    public async Task<string> IndexDocumentsAsync()
    {
        try
        {
            var azureFunctionsBaseUrl = _configuration["AzureFunctions:BaseUrl"] ?? "http://localhost:7071";

            var response = await _httpClient.PostAsync($"{azureFunctionsBaseUrl}/api/IndexDocuments", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing documents");
            throw;
        }
    }

    private async Task<List<SearchResultData>> SearchDocumentsAsync(float[] embeddings, string query)
    {
        var searchEndpoint = _configuration["AzureSearch:Endpoint"];
        var searchApiKey = _configuration["AzureSearch:ApiKey"];
        var indexName = _configuration["AzureSearch:IndexName"];

        // Create search request with vector search
        var searchRequest = new
        {
            vector = new
            {
                value = embeddings,
                fields = "text_vector",
                k = 5
            },
            select = "chunk_id,title,chunk",
            top = 5
        };

        var jsonContent = JsonSerializer.Serialize(searchRequest);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Remove("api-key");
        _httpClient.DefaultRequestHeaders.Add("api-key", searchApiKey);

        var response = await _httpClient.PostAsync(
            $"{searchEndpoint}/indexes/{indexName}/docs/search?api-version=2023-07-01-Preview",
            httpContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Search request failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new Exception($"Search request failed: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(responseContent);

        return searchResponse?.value?.Select(v => new SearchResultData
        {
            Title = v.title ?? "Unknown",
            Content = v.chunk ?? "",
            Score = v.score
        }).ToList() ?? new List<SearchResultData>();
    }

    private string RenderMarkdown(string content)
    {
        // Use Markdig to render markdown to HTML
        return Markdig.Markdown.ToHtml(content);
    }

    private bool ContainsSensitiveContent(string content)
    {
        var sensitivePatterns = new[]
        {
            "password",
            "secret",
            "api.key",
            "token",
            "credential",
            "internal.only",
            "confidential",
            "aws_access_key",
            "aws_secret",
            "database.*connection",
            "admin.*password",
            "webhook\\.site"
        };

        return sensitivePatterns.Any(pattern =>
            System.Text.RegularExpressions.Regex.IsMatch(content, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}

// Supporting classes
public class EmbeddingResponse
{
    public float[]? vector { get; set; }
    public int dimensions { get; set; }
}

public class SearchResultData
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class SearchResponse
{
    public List<SearchValue>? value { get; set; }
}

public class SearchValue
{
    public string? chunk_id { get; set; }
    public string? title { get; set; }
    public string? chunk { get; set; }
    public double score { get; set; }
}
