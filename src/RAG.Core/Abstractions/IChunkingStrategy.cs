using System.Collections.Generic;
using RAG.Core.Models;

namespace RAG.Core.Abstractions
{
    /// <summary>
    /// Strategy for chunking study materials into smaller pieces.
    /// </summary>
    public interface IChunkingStrategy
    {
        /// <summary>
        /// Split material into chunks
        /// </summary>
        /// <param name="material">Raw text material</param>
        /// <param name="source">Source identifier (filename, chapter name, etc.)</param>
        /// <param name="subject">Subject area</param>
        /// <param name="topic">Topic within subject</param>
        IEnumerable<MaterialChunk> ChunkMaterial(
            string material,
            string source,
            string subject,
            string topic);

        /// <summary>
        /// Get the name of this chunking strategy
        /// </summary>
        string StrategyName { get; }
    }
}
