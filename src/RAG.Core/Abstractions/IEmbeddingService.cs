using System.Collections.Generic;
using System.Threading.Tasks;
using RAG.Core.Models;

namespace RAG.Core.Abstractions
{
    /// <summary>
    /// Service for generating embeddings from text using Azure OpenAI.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Embed a single text string
        /// </summary>
        Task<float[]> EmbedTextAsync(string text);

        /// <summary>
        /// Embed multiple text strings in batch
        /// </summary>
        Task<IEnumerable<float[]>> EmbedTextBatchAsync(IEnumerable<string> texts);

        /// <summary>
        /// Get the dimension of embeddings (typically 1536 for text-embedding-3-large)
        /// </summary>
        Task<int> GetEmbeddingDimensionAsync();
    }
}
