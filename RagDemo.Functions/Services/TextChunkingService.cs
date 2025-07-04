using System.Collections.Generic;

namespace RagDemo.Functions.Services;

/// <summary>
/// Implementation of text chunking service that splits text into overlapping chunks
/// </summary>
public class TextChunkingService : ITextChunkingService
{
    public List<string> ChunkText(string text, int maxChunkSize = 2000, int overlapSize = 500)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var chunks = new List<string>();

        // If text is shorter than max chunk size, return as single chunk
        if (text.Length <= maxChunkSize)
        {
            chunks.Add(text);
            return chunks;
        }

        int start = 0;
        while (start < text.Length)
        {
            int end = start + maxChunkSize;

            // If this would be the last chunk, take everything remaining
            if (end >= text.Length)
            {
                chunks.Add(text.Substring(start));
                break;
            }

            // Try to break at a natural boundary (sentence, paragraph, or word)
            int chunkEnd = FindBestBreakPoint(text, start, end);

            chunks.Add(text.Substring(start, chunkEnd - start));

            // Move start position, accounting for overlap
            start = chunkEnd - overlapSize;
            if (start < 0) start = 0;
        }

        return chunks;
    }

    private int FindBestBreakPoint(string text, int start, int maxEnd)
    {
        // Look for sentence endings first
        for (int i = maxEnd - 1; i > start + (maxEnd - start) / 2; i--)
        {
            if (text[i] == '.' || text[i] == '!' || text[i] == '?')
            {
                return i + 1;
            }
        }

        // Look for paragraph breaks
        for (int i = maxEnd - 1; i > start + (maxEnd - start) / 2; i--)
        {
            if (text[i] == '\n' && i > 0 && text[i - 1] == '\n')
            {
                return i + 1;
            }
        }

        // Look for line breaks
        for (int i = maxEnd - 1; i > start + (maxEnd - start) / 2; i--)
        {
            if (text[i] == '\n')
            {
                return i + 1;
            }
        }

        // Look for word boundaries
        for (int i = maxEnd - 1; i > start + (maxEnd - start) / 2; i--)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                return i + 1;
            }
        }

        // If no good break point found, just break at max length
        return maxEnd;
    }
}
