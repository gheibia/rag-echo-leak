using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using System.IO;
using RagDemo.Functions.Services;
using RagDemo.Functions.Models;

namespace RagDemo.Functions;

public class QueryRag
{
    private readonly ILogger<QueryRag> _logger;
    private readonly IAzureOpenAIService _openAIService;
    private readonly IAzureSearchService _searchService;

    public QueryRag(ILogger<QueryRag> logger, IAzureOpenAIService openAIService, IAzureSearchService searchService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _searchService = searchService;
    }

    [Function("QueryRag")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function received a request");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<QueryRequest>(requestBody);

        if (string.IsNullOrEmpty(data?.Text))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Please provide a 'text' property in the request body");
            return badResponse;
        }

        try
        {
            // Generate embedding using Azure OpenAI Service
            var embedding = await _openAIService.GenerateEmbeddingAsync(data.Text);

            if (embedding == null || embedding.Length == 0)
            {
                var badEmbeddingResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await badEmbeddingResponse.WriteStringAsync("Failed to generate embedding");
                return badEmbeddingResponse;
            }

            // Search for similar documents using the embedding
            var searchResults = await _searchService.SearchAsync(embedding, topK: 5);

            // Create the response
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Check if any results contain sensitive content
            bool foundSensitiveContent = searchResults.Any(r => r.ContainsSensitiveContent);

            // Serialize the response
            var responseBody = new QueryRagResponse
            {
                Query = data.Text,
                FoundSensitiveContent = foundSensitiveContent,
                Results = searchResults
            };

            await response.WriteAsJsonAsync(responseBody);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing embedding request");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    private class QueryRequest
    {
        public string? Text { get; set; }
    }
}
