using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace RagDemo.Functions.Services;

public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<AzureOpenAIService> _logger;
    private readonly string _embeddingModelName;

    public AzureOpenAIService(
        IConfiguration configuration,
        ILogger<AzureOpenAIService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new ArgumentNullException("AzureOpenAI:Endpoint configuration is missing");
        _embeddingModelName = configuration["AzureOpenAI:EmbeddingModel"]
            ?? throw new ArgumentNullException("AzureOpenAI:EmbeddingModel configuration is missing");

        // Use managed identity
        var credential = new DefaultAzureCredential();
        _openAIClient = new OpenAIClient(new Uri(endpoint), credential);

        _logger.LogInformation("AzureOpenAIService initialized with managed identity for endpoint: {Endpoint}", endpoint);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        _logger.LogInformation("Generating embedding for text input");

        try
        {
            // Create embedding options with the input text
            var options = new EmbeddingsOptions(_embeddingModelName, new[] { text });

            // Call the Azure OpenAI service
            Response<Embeddings> response = await _openAIClient.GetEmbeddingsAsync(options);

            // Extract the embedding data from the response
            if (response.Value.Data.Count == 0)
            {
                throw new Exception("No embedding data returned from Azure OpenAI");
            }

            // Return the embedding vector
            return response.Value.Data[0].Embedding.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding: {Message}", ex.Message);
            throw;
        }
    }
}
