using System.Collections.Generic;
using System.Threading.Tasks;
using RAG.Core.Models;

namespace RAG.Core.Abstractions
{
    /// <summary>
    /// Abstract interface for vector store operations.
    /// </summary>
    public interface IVectorStore
    {
        /// <summary>
        /// Add a single chunk to the vector store
        /// </summary>
        Task AddChunkAsync(MaterialChunk chunk);

        /// <summary>
        /// Add multiple chunks in batch
        /// </summary>
        Task AddChunkBatchAsync(IEnumerable<MaterialChunk> chunks);

        /// <summary>
        /// Search for similar chunks by embedding
        /// </summary>
        /// <param name="embedding">Query embedding vector</param>
        /// <param name="topK">Number of results to return</param>
        /// <param name="filters">Optional metadata filters</param>
        Task<IEnumerable<MaterialChunk>> SearchAsync(
            float[] embedding,
            int topK = 5,
            Dictionary<string, object>? filters = null);

        /// <summary>
        /// Get a chunk by ID
        /// </summary>
        Task<MaterialChunk?> GetChunkAsync(string chunkId);

        /// <summary>
        /// Delete a chunk by ID
        /// </summary>
        Task DeleteChunkAsync(string chunkId);

        /// <summary>
        /// Clear all chunks, optionally filtered by subject
        /// </summary>
        Task ClearAsync(string? subject = null);

        /// <summary>
        /// Get total number of chunks in store
        /// </summary>
        Task<int> GetChunkCountAsync(string? subject = null);

        /// <summary>
        /// Get all subjects in the vector store
        /// </summary>
        Task<IEnumerable<string>> GetSubjectsAsync();
    }
}
