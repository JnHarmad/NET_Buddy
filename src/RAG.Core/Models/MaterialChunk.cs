using System;
using System.Collections.Generic;

namespace RAG.Core.Models
{
    /// <summary>
    /// Represents a chunk of study material with embeddings and metadata.
    /// </summary>
    public class MaterialChunk
    {
        /// <summary>
        /// Unique identifier for the chunk
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The text content of the chunk
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Vector embedding of the chunk content
        /// </summary>
        public float[]? Embedding { get; set; }

        /// <summary>
        /// Subject area (Physics, Chemistry, Mathematics, etc.)
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Source file or chapter name
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Topic within the subject
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Position/index of this chunk in the original document
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// Difficulty level: Basic, Intermediate, Advanced
        /// </summary>
        public string DifficultyLevel { get; set; } = "Intermediate";

        /// <summary>
        /// When this chunk was ingested
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata for flexibility
        /// </summary>
        public Dictionary<string, object> AdditionalMetadata { get; set; } = new();

        /// <summary>
        /// Calculate content length in tokens (rough estimate)
        /// </summary>
        public int EstimatedTokenCount => Content.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length / 4 + 1;
    }
}
