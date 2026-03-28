namespace RAG.Core.Models
{
    /// <summary>
    /// Represents a retrieved context from the knowledge base for RAG.
    /// </summary>
    public class RetrievedContext
    {
        /// <summary>
        /// The chunk ID from the vector store
        /// </summary>
        public string ChunkId { get; set; } = string.Empty;

        /// <summary>
        /// The content of the retrieved chunk
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Subject area of the content
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Topic within the subject
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Source of the material (file, chapter, etc.)
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Relevance score (0-1, higher is better)
        /// </summary>
        public float RelevanceScore { get; set; }

        /// <summary>
        /// Difficulty level
        /// </summary>
        public string DifficultyLevel { get; set; } = "Intermediate";
    }
}
