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
    /// Material ingestion service that handles chunking and vectorization of study materials.
    /// </summary>
    public class MaterialIngestionService : IMaterialIngestionService
    {
        private readonly IVectorStore _vectorStore;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChunkingStrategy _chunkingStrategy;
        private readonly ILogger<MaterialIngestionService> _logger;

        public MaterialIngestionService(
            IVectorStore vectorStore,
            IEmbeddingService embeddingService,
            IChunkingStrategy chunkingStrategy,
            ILogger<MaterialIngestionService> logger)
        {
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _chunkingStrategy = chunkingStrategy ?? throw new ArgumentNullException(nameof(chunkingStrategy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> IngestTextAsync(string content, string subject, string topic, string source)
        {
            _logger.LogInformation(
                "Ingesting text from {Source} | Subject: {Subject} | Topic: {Topic}",
                source, subject, topic);

            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Empty content provided for ingestion from {Source}", source);
                    return 0;
                }

                // Step 1: Chunk the material
                var chunks = _chunkingStrategy.ChunkMaterial(content, source, subject, topic).ToList();
                _logger.LogInformation("Created {ChunkCount} chunks using {Strategy} strategy", 
                    chunks.Count, _chunkingStrategy.StrategyName);

                if (chunks.Count == 0)
                    return 0;

                // Step 2: Generate embeddings for all chunks
                var chunkTexts = chunks.Select(c => c.Content).ToList();
                var embeddings = await _embeddingService.EmbedTextBatchAsync(chunkTexts);

                // Step 3: Assign embeddings to chunks
                var embeddingsList = embeddings.ToList();
                for (int i = 0; i < chunks.Count && i < embeddingsList.Count; i++)
                {
                    chunks[i].Embedding = embeddingsList[i];
                }

                // Step 4: Store chunks in vector store
                await _vectorStore.AddChunkBatchAsync(chunks);
                _logger.LogInformation("Successfully ingested {ChunkCount} chunks from {Source}", chunks.Count, source);

                return chunks.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting text from {Source}", source);
                throw;
            }
        }

        public async Task<int> IngestFromFileAsync(string filePath, string subject, string topic)
        {
            _logger.LogInformation("Ingesting file: {FilePath}", filePath);

            try
            {
                if (!System.IO.File.Exists(filePath))
                    throw new System.IO.FileNotFoundException($"File not found: {filePath}");

                var content = await System.IO.File.ReadAllTextAsync(filePath);
                var sourceFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

                return await IngestTextAsync(content, subject, topic, sourceFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<(int totalChunks, int successfulFiles)> IngestFromDirectoryAsync(
            string directoryPath,
            string subject,
            string? topicPattern = null)
        {
            _logger.LogInformation("Ingesting from directory: {DirectoryPath} | Subject: {Subject}", 
                directoryPath, subject);

            try
            {
                if (!System.IO.Directory.Exists(directoryPath))
                    throw new System.IO.DirectoryNotFoundException($"Directory not found: {directoryPath}");

                var files = System.IO.Directory.GetFiles(directoryPath, "*.txt")
                    .Concat(System.IO.Directory.GetFiles(directoryPath, "*.md"))
                    .ToList();

                _logger.LogInformation("Found {FileCount} files to ingest", files.Count);

                int totalChunks = 0;
                int successfulFiles = 0;

                foreach (var file in files)
                {
                    try
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        var topic = topicPattern ?? fileName;
                        
                        var chunkCount = await IngestFromFileAsync(file, subject, topic);
                        totalChunks += chunkCount;
                        successfulFiles++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to ingest file: {File}", file);
                    }
                }

                _logger.LogInformation(
                    "Directory ingestion complete: {SuccessfulFiles}/{TotalFiles} files, {TotalChunks} chunks",
                    successfulFiles, files.Count, totalChunks);

                return (totalChunks, successfulFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting directory: {DirectoryPath}", directoryPath);
                throw;
            }
        }

        public async Task ClearMaterialsAsync(string? subject = null)
        {
            _logger.LogWarning("Clearing materials {Filter}", subject == null ? "ALL" : $"for subject: {subject}");
            await _vectorStore.ClearAsync(subject);
        }

        public async Task<IngestionStats> GetStatsAsync()
        {
            var totalChunks = await _vectorStore.GetChunkCountAsync();
            var subjects = await _vectorStore.GetSubjectsAsync();

            return new IngestionStats
            {
                TotalChunks = totalChunks,
                TotalSubjects = subjects.Count(),
                TotalTopics = subjects.Count() // Simplified; enhance as needed
            };
        }
    }
}
