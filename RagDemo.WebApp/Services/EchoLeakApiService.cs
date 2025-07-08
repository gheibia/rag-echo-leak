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

            // Make a call to QueryRag function which now handles everything
            var request = new { Text = query };
            var jsonContent = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{azureFunctionsBaseUrl}/api/QueryRag", httpContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw API Response: {Response}", responseContent); // Debug logging

            var queryResult = JsonSerializer.Deserialize<QueryRagResponse>(responseContent);

            if (queryResult == null)
            {
                throw new Exception("Failed to deserialize QueryRag response");
            }

            _logger.LogInformation("Deserialized result - Query: {Query}, ResultCount: {Count}",
                queryResult.Query, queryResult.Results.Count); // Debug logging

            // Convert to EchoLeakTestResult
            var result = new EchoLeakTestResult
            {
                Query = queryResult.Query,
                FoundSensitiveContent = queryResult.FoundSensitiveContent,
                Results = queryResult.Results.Select(r =>
                {
                    _logger.LogInformation("Processing result - Title: {Title}, Content length: {Length}, Score: {Score}",
                        r.Title, r.Content?.Length ?? 0, r.Score);

                    var renderedMarkdown = RenderMarkdown(r.Content ?? string.Empty);
                    _logger.LogInformation("Rendered markdown length: {Length}", renderedMarkdown?.Length ?? 0);

                    return new SearchResultItem
                    {
                        Title = r.Title,
                        Content = r.Content ?? string.Empty,
                        Score = r.Score,
                        ContainsSensitiveContent = r.ContainsSensitiveContent,
                        RenderedMarkdown = renderedMarkdown ?? string.Empty
                    };
                }).ToList()
            };

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

    private string RenderMarkdown(string content)
    {
        // Use Markdig to render markdown to HTML
        return Markdig.Markdown.ToHtml(content);
    }
}
