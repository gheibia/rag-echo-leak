using System.Threading.Tasks;
using RagDemo.Functions.Models;

namespace RagDemo.Functions.Services;

/// <summary>
/// Interface for document indexing operations (Write operations)
/// </summary>
public interface IDocumentIndexService
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
}
