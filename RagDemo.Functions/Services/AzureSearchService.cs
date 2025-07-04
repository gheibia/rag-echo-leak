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

public class AzureSearchService : IAzureSearchService
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
}
