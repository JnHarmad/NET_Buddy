using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RAG.Core.Abstractions;
using RAG.Core.Models;

namespace RAG.Core.Services
{
    /// <summary>
    /// In-memory vector store implementation. Suitable for development and smaller datasets.
    /// Uses simple Euclidean distance for similarity search.
    /// </summary>
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly Dictionary<string, MaterialChunk> _chunks = new();
        private readonly object _lockObject = new();

        public async Task AddChunkAsync(MaterialChunk chunk)
        {
            lock (_lockObject)
            {
                _chunks[chunk.Id] = chunk;
            }
            await Task.CompletedTask;
        }

        public async Task AddChunkBatchAsync(IEnumerable<MaterialChunk> chunks)
        {
            lock (_lockObject)
            {
                foreach (var chunk in chunks)
                {
                    _chunks[chunk.Id] = chunk;
                }
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<MaterialChunk>> SearchAsync(
            float[] embedding,
            int topK = 5,
            Dictionary<string, object>? filters = null)
        {
            if (embedding == null || embedding.Length == 0)
                return Enumerable.Empty<MaterialChunk>();

            List<(string id, MaterialChunk chunk, float distance)> scoredChunks;
            
            lock (_lockObject)
            {
                scoredChunks = _chunks
                    .Where(kvp => ShouldIncludeChunk(kvp.Value, filters))
                    .Select(kvp => (
                        id: kvp.Key,
                        chunk: kvp.Value,
                        distance: CalculateDistance(embedding, kvp.Value.Embedding)
                    ))
                    .OrderBy(x => x.distance)
                    .Take(topK)
                    .ToList();
            }

            return await Task.FromResult(scoredChunks.Select(x => x.chunk));
        }

        public async Task<MaterialChunk?> GetChunkAsync(string chunkId)
        {
            lock (_lockObject)
            {
                return _chunks.TryGetValue(chunkId, out var chunk) ? chunk : null;
            }
        }

        public async Task DeleteChunkAsync(string chunkId)
        {
            lock (_lockObject)
            {
                _chunks.Remove(chunkId);
            }
            await Task.CompletedTask;
        }

        public async Task ClearAsync(string? subject = null)
        {
            lock (_lockObject)
            {
                if (subject == null)
                {
                    _chunks.Clear();
                }
                else
                {
                    var keysToRemove = _chunks
                        .Where(kvp => kvp.Value.Subject == subject)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in keysToRemove)
                        _chunks.Remove(key);
                }
            }
            await Task.CompletedTask;
        }

        public async Task<int> GetChunkCountAsync(string? subject = null)
        {
            lock (_lockObject)
            {
                return subject == null
                    ? _chunks.Count
                    : _chunks.Values.Count(c => c.Subject == subject);
            }
        }

        public async Task<IEnumerable<string>> GetSubjectsAsync()
        {
            lock (_lockObject)
            {
                return _chunks.Values.Select(c => c.Subject).Distinct().ToList();
            }
        }

        /// <summary>
        /// Calculate Euclidean distance between two embeddings.
        /// Lower distance = higher similarity.
        /// </summary>
        private float CalculateDistance(float[] embedding1, float[]? embedding2)
        {
            if (embedding2 == null || embedding1.Length != embedding2.Length)
                return float.MaxValue;

            float sumSquaredDiff = 0;
            for (int i = 0; i < embedding1.Length; i++)
            {
                var diff = embedding1[i] - embedding2[i];
                sumSquaredDiff += diff * diff;
            }

            return (float)Math.Sqrt(sumSquaredDiff);
        }

        /// <summary>
        /// Check if chunk matches filter criteria
        /// </summary>
        private bool ShouldIncludeChunk(MaterialChunk chunk, Dictionary<string, object>? filters)
        {
            if (filters == null || filters.Count == 0)
                return chunk.Embedding != null && chunk.Embedding.Length > 0;

            foreach (var filter in filters)
            {
                switch (filter.Key.ToLower())
                {
                    case "subject":
                        if (chunk.Subject != filter.Value.ToString())
                            return false;
                        break;
                    case "topic":
                        if (chunk.Topic != filter.Value.ToString())
                            return false;
                        break;
                    case "difficulty":
                        if (chunk.DifficultyLevel != filter.Value.ToString())
                            return false;
                        break;
                }
            }

            return chunk.Embedding != null && chunk.Embedding.Length > 0;
        }
    }
}
