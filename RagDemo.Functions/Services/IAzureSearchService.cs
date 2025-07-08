using System.Threading.Tasks;
using RagDemo.Functions.Models;

namespace RagDemo.Functions.Services;

/// <summary>
/// Interface for Azure AI Search operations
/// </summary>
public interface IAzureSearchService
{
    /// <summary>
    /// Indexes a document with its embedding vector
    /// </summary>
    /// <param name="document">The document to index</param>
    /// <returns>A task representing the async operation</returns>
    Task IndexDocumentAsync(RagDocument document);

    /// <summary>
    /// Indexes multiple documents with their embedding vectors
    /// </summary>
    /// <param name="documents">The documents to index</param>
    /// <returns>A task representing the async operation</returns>
    Task IndexDocumentsAsync(IEnumerable<RagDocument> documents);

    /// <summary>
    /// Searches for documents using vector similarity
    /// </summary>
    /// <param name="queryVector">The query embedding vector</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>A list of search results</returns>
    Task<List<SearchResultItem>> SearchAsync(float[] queryVector, int topK = 5);
}
