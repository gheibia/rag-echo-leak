using System.Threading.Tasks;

namespace RagDemo.Functions.Services;

/// <summary>
/// Interface for Azure OpenAI Service operations
/// </summary>
public interface IAzureOpenAIService
{
    /// <summary>
    /// Generates embeddings for the given text input
    /// </summary>
    /// <param name="text">The text to generate embeddings for</param>
    /// <returns>An array of floating point values representing the embedding vector</returns>
    Task<float[]> GenerateEmbeddingAsync(string text);
}
