using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using System.IO;
using RagDemo.Functions.Services;

namespace RagDemo.Functions;

public class QueryRag
{
    private readonly ILogger<QueryRag> _logger;
    private readonly IAzureOpenAIService _openAIService;

    public QueryRag(ILogger<QueryRag> logger, IAzureOpenAIService openAIService)
    {
        _logger = logger;
        _openAIService = openAIService;
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

            // Create the response
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Serialize the embedding response
            var responseBody = new
            {
                vector = embedding,
                dimensions = embedding?.Length ?? 0
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
