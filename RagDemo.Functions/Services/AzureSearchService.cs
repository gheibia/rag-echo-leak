using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using RagDemo.Functions.Models;

namespace RagDemo.Functions.Services;

public class AzureSearchService : IDocumentIndexService, IDocumentSearchService
{
    private readonly SearchClient _searchClient;
    private readonly ILogger<AzureSearchService> _logger;

    public AzureSearchService(
        IConfiguration configuration,
        ILogger<AzureSearchService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var searchEndpoint = configuration["AzureSearch:Endpoint"]
            ?? throw new ArgumentNullException("AzureSearch:Endpoint configuration is missing");
        var searchApiKey = configuration["AzureSearch:ApiKey"]
            ?? throw new ArgumentNullException("AzureSearch:ApiKey configuration is missing");
        var indexName = configuration["AzureSearch:IndexName"]
            ?? throw new ArgumentNullException("AzureSearch:IndexName configuration is missing");

        // Create the Azure Search client
        var credential = new AzureKeyCredential(searchApiKey);
        _searchClient = new SearchClient(new Uri(searchEndpoint), indexName, credential);
    }

    public async Task IndexDocumentAsync(RagDocument document)
    {
        _logger.LogInformation("Indexing single document with ID: {DocumentId}", document.Id);

        try
        {
            var batch = IndexDocumentsBatch.Upload(new[] { document });
            IndexDocumentsResult result = await _searchClient.IndexDocumentsAsync(batch);

            if (result.Results.Count > 0 && result.Results[0].Succeeded)
            {
                _logger.LogInformation("Successfully indexed document with ID: {DocumentId}", document.Id);
            }
            else
            {
                var errorMessage = result.Results.Count > 0 ? result.Results[0].ErrorMessage : "Unknown error";
                _logger.LogError("Failed to index document with ID: {DocumentId}. Error: {Error}", document.Id, errorMessage);
                throw new Exception($"Failed to index document: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document with ID: {DocumentId}", document.Id);
            throw;
        }
    }

    public async Task IndexDocumentsAsync(IEnumerable<RagDocument> documents)
    {
        var documentList = documents.ToList();
        _logger.LogInformation("Indexing {DocumentCount} documents", documentList.Count);

        try
        {
            var batch = IndexDocumentsBatch.Upload(documentList);
            IndexDocumentsResult result = await _searchClient.IndexDocumentsAsync(batch);

            var successCount = result.Results.Count(r => r.Succeeded);
            var failureCount = result.Results.Count - successCount;

            _logger.LogInformation("Indexing completed. Success: {SuccessCount}, Failures: {FailureCount}",
                successCount, failureCount);

            if (failureCount > 0)
            {
                var errors = result.Results
                    .Where(r => !r.Succeeded)
                    .Select(r => $"ID: {r.Key}, Error: {r.ErrorMessage}")
                    .ToList();

                _logger.LogWarning("Some documents failed to index: {Errors}", string.Join("; ", errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing documents batch");
            throw;
        }
    }

    public async Task<List<SearchResultItem>> SearchAsync(float[] queryVector, int topK = 5)
    {
        _logger.LogInformation("Performing vector search with top {TopK} results", topK);

        try
        {
            var searchOptions = new SearchOptions
            {
                Size = topK,
                Select = { "chunk_id", "title", "chunk" },
                IncludeTotalCount = false
            };

            // Add vector search
            searchOptions.VectorSearch = new VectorSearchOptions();
            searchOptions.VectorSearch.Queries.Add(new VectorizedQuery(queryVector)
            {
                KNearestNeighborsCount = topK,
                Fields = { "text_vector" }
            });

            var searchResults = await _searchClient.SearchAsync<RagDocument>(
                searchText: null,
                searchOptions);

            var results = new List<SearchResultItem>();

            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                var item = new SearchResultItem
                {
                    Id = result.Document.Id,
                    Title = result.Document.Title,
                    Content = result.Document.Content,
                    Score = result.Score ?? 0.0,
                    ContainsSensitiveContent = ContainsSensitiveContent(result.Document.Content)
                };

                results.Add(item);
            }

            _logger.LogInformation("Vector search completed. Found {ResultCount} results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vector search");
            throw;
        }
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
            "webhook\\.site",
            "ghp_",
            "AKIA_"
        };

        return sensitivePatterns.Any(pattern =>
            System.Text.RegularExpressions.Regex.IsMatch(content, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}
