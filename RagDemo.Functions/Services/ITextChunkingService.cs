using System.Collections.Generic;

namespace RagDemo.Functions.Services;

/// <summary>
/// Interface for text chunking operations
/// </summary>
public interface ITextChunkingService
{
    /// <summary>
    /// Splits text into chunks with overlap
    /// </summary>
    /// <param name="text">The text to chunk</param>
    /// <param name="maxChunkSize">Maximum characters per chunk</param>
    /// <param name="overlapSize">Number of characters to overlap between chunks</param>
    /// <returns>List of text chunks</returns>
    List<string> ChunkText(string text, int maxChunkSize = 2000, int overlapSize = 500);
}
