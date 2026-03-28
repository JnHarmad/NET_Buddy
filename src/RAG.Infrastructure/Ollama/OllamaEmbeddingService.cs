using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RAG.Core.Abstractions;

namespace RAG.Infrastructure.Ollama
{
    /// <summary>
    /// Embedding service using local Ollama for offline operation.
    /// Uses the same model as generation for consistency.
    /// </summary>
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaEndpoint;
        private readonly string _modelName;
        private readonly ILogger<OllamaEmbeddingService> _logger;
        private int? _cachedEmbeddingDimension;

        public OllamaEmbeddingService(
            HttpClient httpClient,
            string ollamaEndpoint,
            string modelName,
            ILogger<OllamaEmbeddingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ollamaEndpoint = ollamaEndpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(ollamaEndpoint));
            _modelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<float[]> EmbedTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty", nameof(text));

            try
            {
                _logger.LogDebug("Generating embedding for text of length {Length} using model {Model}", 
                    text.Length, _modelName);

                var request = new
                {
                    model = _modelName,
                    prompt = text
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_ollamaEndpoint}/api/embeddings",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Ollama API error ({response.StatusCode}): {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (!root.TryGetProperty("embedding", out var embeddingElement))
                    throw new InvalidOperationException("No embedding found in Ollama response");

                var embedding = embeddingElement.EnumerateArray()
                    .Select(e => (float)e.GetDouble())
                    .ToArray();

                _logger.LogDebug("Embedding generated with dimension {Dimension}", embedding.Length);
                return embedding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding");
                throw;
            }
        }

        public async Task<IEnumerable<float[]>> EmbedTextBatchAsync(IEnumerable<string> texts)
        {
            var textList = texts.ToList();

            if (textList.Count == 0)
                return Enumerable.Empty<float[]>();

            try
            {
                _logger.LogInformation("Generating embeddings for {Count} texts using Ollama", textList.Count);

                var embeddings = new List<float[]>();

                // Process sequentially to avoid overwhelming Ollama
                foreach (var text in textList)
                {
                    try
                    {
                        var embedding = await EmbedTextAsync(text);
                        embeddings.Add(embedding);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to embed individual text, skipping");
                    }
                }

                _logger.LogInformation("Successfully generated {Count} embeddings", embeddings.Count);
                return embeddings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating batch embeddings");
                throw;
            }
        }

        public async Task<int> GetEmbeddingDimensionAsync()
        {
            if (_cachedEmbeddingDimension.HasValue)
                return _cachedEmbeddingDimension.Value;

            try
            {
                var testEmbedding = await EmbedTextAsync("test");
                _cachedEmbeddingDimension = testEmbedding.Length;
                _logger.LogInformation("Ollama embedding dimension: {Dimension}", _cachedEmbeddingDimension);
                return _cachedEmbeddingDimension.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining embedding dimension");
                throw;
            }
        }
    }
}
