using System.Collections.Generic;
using System.Threading.Tasks;
using RAG.Core.Models;

namespace RAG.Core.Abstractions
{
    /// <summary>
    /// Service for retrieving relevant context chunks from the knowledge base.
    /// </summary>
    public interface IRetrievalService
    {
        /// <summary>
        /// Retrieve relevant context chunks for a query
        /// </summary>
        /// <param name="query">The user's question</param>
        /// <param name="topK">Number of results to retrieve</param>
        /// <param name="subject">Optional filter by subject</param>
        Task<IEnumerable<RetrievedContext>> RetrieveAsync(
            string query,
            int topK = 5,
            string? subject = null);

        /// <summary>
        /// Retrieve context with custom embedding (for advanced use cases)
        /// </summary>
        Task<IEnumerable<RetrievedContext>> RetrieveByEmbeddingAsync(
            float[] embedding,
            int topK = 5,
            string? subject = null);
    }
}
