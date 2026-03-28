using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RAG.Core.Abstractions;
using RAG.Core.Models;

namespace RAG.Core.Services
{
    /// <summary>
    /// Retrieval service that combines embedding and vector store operations.
    /// </summary>
    public class RetrievalService : IRetrievalService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly ILogger<RetrievalService> _logger;

        public RetrievalService(
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            ILogger<RetrievalService> logger)
        {
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RetrievedContext>> RetrieveAsync(
            string query,
            int topK = 5,
            string? subject = null)
        {
            _logger.LogInformation("Retrieving context for query: {Query} (Subject: {Subject})", query, subject ?? "Any");

            try
            {
                // Step 1: Embed the query
                var queryEmbedding = await _embeddingService.EmbedTextAsync(query);
                _logger.LogDebug("Query embedding generated with dimension {Dimension}", queryEmbedding.Length);

                // Step 2: Search vector store with optional subject filter
                var filters = subject != null 
                    ? new Dictionary<string, object> { { "subject", subject } }
                    : null;

                var retrievedChunks = await _vectorStore.SearchAsync(queryEmbedding, topK, filters);

                // Step 3: Convert to RetrievedContext with relevance scores
                var contexts = retrievedChunks.Select(chunk => new RetrievedContext
                {
                    ChunkId = chunk.Id,
                    Content = chunk.Content,
                    Subject = chunk.Subject,
                    Topic = chunk.Topic,
                    Source = chunk.Source,
                    DifficultyLevel = chunk.DifficultyLevel,
                    RelevanceScore = CalculateRelevanceScore(queryEmbedding, chunk.Embedding)
                }).ToList();

                _logger.LogInformation("Retrieved {Count} relevant contexts", contexts.Count);
                return contexts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving context for query: {Query}", query);
                throw;
            }
        }

        public async Task<IEnumerable<RetrievedContext>> RetrieveByEmbeddingAsync(
            float[] embedding,
            int topK = 5,
            string? subject = null)
        {
            _logger.LogInformation("Retrieving by embedding vector (Size: {Size})", embedding.Length);

            try
            {
                var filters = subject != null
                    ? new Dictionary<string, object> { { "subject", subject } }
                    : null;

                var retrievedChunks = await _vectorStore.SearchAsync(embedding, topK, filters);

                var contexts = retrievedChunks.Select(chunk => new RetrievedContext
                {
                    ChunkId = chunk.Id,
                    Content = chunk.Content,
                    Subject = chunk.Subject,
                    Topic = chunk.Topic,
                    Source = chunk.Source,
                    DifficultyLevel = chunk.DifficultyLevel,
                    RelevanceScore = CalculateRelevanceScore(embedding, chunk.Embedding)
                }).ToList();

                return contexts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving by embedding");
                throw;
            }
        }

        /// <summary>
        /// Calculate relevance score as cosine similarity (0-1)
        /// </summary>
        private float CalculateRelevanceScore(float[] embedding1, float[]? embedding2)
        {
            if (embedding2 == null || embedding1.Length != embedding2.Length)
                return 0f;

            float dotProduct = 0;
            float magnitude1 = 0;
            float magnitude2 = 0;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                magnitude1 += embedding1[i] * embedding1[i];
                magnitude2 += embedding2[i] * embedding2[i];
            }

            magnitude1 = (float)Math.Sqrt(magnitude1);
            magnitude2 = (float)Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0f;

            // Cosine similarity ranges from -1 to 1; convert to 0-1
            var cosineSimilarity = dotProduct / (magnitude1 * magnitude2);
            return (cosineSimilarity + 1) / 2;
        }
    }
}
