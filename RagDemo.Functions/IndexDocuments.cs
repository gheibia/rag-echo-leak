using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using System.IO;
using RagDemo.Functions.Services;
using RagDemo.Functions.Models;

namespace RagDemo.Functions;

public class IndexDocuments
{
    private readonly ILogger<IndexDocuments> _logger;
    private readonly IAzureOpenAIService _openAIService;
    private readonly IDocumentIndexService _indexService;
    private readonly ITextChunkingService _chunkingService;

    public IndexDocuments(
        ILogger<IndexDocuments> logger,
        IAzureOpenAIService openAIService,
        IDocumentIndexService indexService,
        ITextChunkingService chunkingService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _indexService = indexService;
        _chunkingService = chunkingService;
    }

    [Function("IndexDocuments")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Starting document indexing process");

        try
        {
            // Get the path to the rag-data folder relative to the function app
            var ragDataPath = Path.Combine(Directory.GetCurrentDirectory(), "rag-data");

            if (!Directory.Exists(ragDataPath))
            {
                _logger.LogError("rag-data directory not found at: {Path}", ragDataPath);
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("rag-data directory not found");
                return notFoundResponse;
            }

            // Get all text files in the rag-data directory
            var textFiles = Directory.GetFiles(ragDataPath, "*.txt");

            if (textFiles.Length == 0)
            {
                _logger.LogWarning("No .txt files found in rag-data directory");
                var noFilesResponse = req.CreateResponse(HttpStatusCode.OK);
                await noFilesResponse.WriteAsJsonAsync(new { message = "No .txt files found to index", filesProcessed = 0 });
                return noFilesResponse;
            }

            _logger.LogInformation("Found {FileCount} text files to process", textFiles.Length);

            var documents = new List<RagDocument>();

            // Process each file
            foreach (var filePath in textFiles)
            {
                try
                {
                    _logger.LogInformation("Processing file: {FileName}", Path.GetFileName(filePath));

                    // Read file content
                    var content = await File.ReadAllTextAsync(filePath);

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning("File {FileName} is empty, skipping", Path.GetFileName(filePath));
                        continue;
                    }

                    // Chunk the content using the same parameters as the Azure skillset
                    var chunks = _chunkingService.ChunkText(content, maxChunkSize: 2000, overlapSize: 500);
                    _logger.LogInformation("File {FileName} split into {ChunkCount} chunks", Path.GetFileName(filePath), chunks.Count);

                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var title = FormatTitle(fileName);

                    // Process each chunk
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var chunk = chunks[i];

                        try
                        {
                            // Generate embedding for this chunk
                            var embedding = await _openAIService.GenerateEmbeddingAsync(chunk);

                            // Create search document for this chunk
                            var document = new RagDocument
                            {
                                Id = GenerateChunkId(fileName, i),
                                Title = title,
                                Content = chunk,
                                Source = fileName, // parent_id in the skillset
                                ContentVector = embedding,
                                CreatedAt = DateTime.UtcNow,
                            };

                            documents.Add(document);
                            _logger.LogInformation("Successfully processed chunk {ChunkIndex} of file {FileName}", i + 1, Path.GetFileName(filePath));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing chunk {ChunkIndex} of file {FileName}", i + 1, Path.GetFileName(filePath));
                            // Continue processing other chunks even if one fails
                        }
                    }

                    _logger.LogInformation("Successfully processed file: {FileName} with {ChunkCount} chunks", Path.GetFileName(filePath), chunks.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file: {FileName}", Path.GetFileName(filePath));
                    // Continue processing other files even if one fails
                }
            }

            if (documents.Count == 0)
            {
                _logger.LogWarning("No documents were successfully processed");
                var noDocsResponse = req.CreateResponse(HttpStatusCode.OK);
                await noDocsResponse.WriteAsJsonAsync(new { message = "No documents were successfully processed", filesProcessed = 0 });
                return noDocsResponse;
            }

            // Index all documents in Azure Search
            await _indexService.IndexDocumentsAsync(documents);

            _logger.LogInformation("Successfully indexed {DocumentCount} documents", documents.Count);

            // Create success response
            var response = req.CreateResponse(HttpStatusCode.OK);
            var responseBody = new
            {
                message = "Document chunks indexed successfully",
                chunksProcessed = documents.Count,
                totalFilesFound = textFiles.Length,
                documents = documents.Select(d => new { d.Id, d.Title, d.Source, ContentLength = d.Content.Length }).ToList()
            };

            await response.WriteAsJsonAsync(responseBody);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document indexing process");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    private static string GenerateChunkId(string fileName, int chunkIndex)
    {
        // Create a unique ID for each chunk based on filename and chunk index
        return $"{fileName}_chunk_{chunkIndex}";
    }

    private static string FormatTitle(string fileName)
    {
        // Convert filename to a more readable title
        return fileName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(' ')
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower())
            .Aggregate((current, next) => current + " " + next);
    }
}
