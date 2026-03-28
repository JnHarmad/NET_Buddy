using System;
using System.Collections.Generic;
using System.Linq;
using RAG.Core.Abstractions;
using RAG.Core.Models;

namespace RAG.Core.Services
{
    /// <summary>
    /// Fixed-size chunking strategy that splits material into fixed-length chunks with overlap.
    /// </summary>
    public class FixedSizeChunkingStrategy : IChunkingStrategy
    {
        private readonly int _chunkSize;
        private readonly int _overlap;

        public string StrategyName => "FixedSize";

        public FixedSizeChunkingStrategy(int chunkSize = 800, int overlap = 100)
        {
            _chunkSize = chunkSize;
            _overlap = overlap < chunkSize ? overlap : chunkSize / 2;
        }

        public IEnumerable<MaterialChunk> ChunkMaterial(
            string material,
            string source,
            string subject,
            string topic)
        {
            if (string.IsNullOrWhiteSpace(material))
                yield break;

            var words = material.Split(new[] { ' ', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<MaterialChunk>();
            var chunkIndex = 0;

            for (int i = 0; i < words.Length; i += _chunkSize - _overlap)
            {
                var chunkWords = words.Skip(i).Take(_chunkSize).ToList();
                
                if (chunkWords.Count < 10) // Skip very small chunks
                    break;

                var content = string.Join(" ", chunkWords);
                
                chunks.Add(new MaterialChunk
                {
                    Id = $"{source}_{subject}_{chunkIndex}_{Guid.NewGuid()}",
                    Content = content,
                    Subject = subject,
                    Source = source,
                    Topic = topic,
                    ChunkIndex = chunkIndex,
                    CreatedAt = DateTime.UtcNow,
                    DifficultyLevel = "Intermediate"
                });

                chunkIndex++;
            }

            foreach (var chunk in chunks)
                yield return chunk;
        }
    }

    /// <summary>
    /// Sentence-based chunking strategy that respects sentence boundaries.
    /// </summary>
    public class SentenceChunkingStrategy : IChunkingStrategy
    {
        private readonly int _sentencesPerChunk;

        public string StrategyName => "SentenceBased";

        public SentenceChunkingStrategy(int sentencesPerChunk = 5)
        {
            _sentencesPerChunk = sentencesPerChunk;
        }

        public IEnumerable<MaterialChunk> ChunkMaterial(
            string material,
            string source,
            string subject,
            string topic)
        {
            if (string.IsNullOrWhiteSpace(material))
                yield break;

            // Simple sentence splitting on '.', '!', '?'
            var sentences = material
                .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim() + ".")
                .Where(s => s.Length > 10)
                .ToList();

            var chunkIndex = 0;

            for (int i = 0; i < sentences.Count; i += _sentencesPerChunk)
            {
                var chunkSentences = sentences.Skip(i).Take(_sentencesPerChunk).ToList();
                
                if (chunkSentences.Count == 0)
                    break;

                var content = string.Join(" ", chunkSentences);

                yield return new MaterialChunk
                {
                    Id = $"{source}_{subject}_{chunkIndex}_{Guid.NewGuid()}",
                    Content = content,
                    Subject = subject,
                    Source = source,
                    Topic = topic,
                    ChunkIndex = chunkIndex,
                    CreatedAt = DateTime.UtcNow,
                    DifficultyLevel = "Intermediate"
                };

                chunkIndex++;
            }
        }
    }
}
