using System.Threading.Tasks;

namespace RAG.Core.Abstractions
{
    /// <summary>
    /// Service for ingesting and processing study materials into the knowledge base.
    /// </summary>
    public interface IMaterialIngestionService
    {
        /// <summary>
        /// Ingest text content into the knowledge base
        /// </summary>
        /// <param name="content">Raw text content</param>
        /// <param name="subject">Subject area (Physics, Chemistry, etc.)</param>
        /// <param name="topic">Topic within the subject</param>
        /// <param name="source">Source identifier (file name, chapter name, etc.)</param>
        Task<int> IngestTextAsync(string content, string subject, string topic, string source);

        /// <summary>
        /// Ingest from a text file
        /// </summary>
        Task<int> IngestFromFileAsync(string filePath, string subject, string topic);

        /// <summary>
        /// Ingest from multiple files
        /// </summary>
        Task<(int totalChunks, int successfulFiles)> IngestFromDirectoryAsync(
            string directoryPath,
            string subject,
            string? topicPattern = null);

        /// <summary>
        /// Clear all ingested materials (optionally by subject)
        /// </summary>
        Task ClearMaterialsAsync(string? subject = null);

        /// <summary>
        /// Get ingestion statistics
        /// </summary>
        Task<IngestionStats> GetStatsAsync();
    }

    /// <summary>
    /// Statistics about ingested materials
    /// </summary>
    public class IngestionStats
    {
        public int TotalChunks { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalTopics { get; set; }
        public long TotalContentBytes { get; set; }
    }
}
