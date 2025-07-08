using System.Threading.Tasks;
using RagDemo.Functions.Models;

namespace RagDemo.Functions.Services;

/// <summary>
/// Interface for document search operations (Read operations)
/// </summary>
public interface IDocumentSearchService
{
    /// <summary>
    /// Searches for documents using vector similarity
    /// </summary>
    /// <param name="queryVector">The query embedding vector</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>A list of search results</returns>
    Task<List<SearchResultItem>> SearchAsync(float[] queryVector, int topK = 5);
}
