using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using RAG.Core.Abstractions;
using RAG.Core.Models;
using RAG.Core.Services;
using RAG.Infrastructure.Ollama;

namespace RAG.Infrastructure
{
    /// <summary>
    /// Dependency injection extensions for RAG system
    /// Ollama-only implementation for offline operation
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add RAG services to the DI container
        /// Configured for local Ollama operation only
        /// </summary>
        public static IServiceCollection AddRAGServices(this IServiceCollection services, RAGSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // Register settings
            services.AddSingleton(settings);

            // Register vector store
            services.AddSingleton<IVectorStore, InMemoryVectorStore>();

            // Register chunking strategy
            services.AddSingleton<IChunkingStrategy>(sp => 
                settings.ChunkingStrategy.ToLower() switch
                {
                    "sentence" => new SentenceChunkingStrategy(),
                    _ => new FixedSizeChunkingStrategy(settings.ChunkSize, settings.ChunkOverlap)
                });

            // Register ingestion service
            services.AddSingleton<IMaterialIngestionService, MaterialIngestionService>();

            // Register Ollama services
            RegisterOllamaServices(services, settings);

            // Register retrieval service
            services.AddSingleton<IRetrievalService, RetrievalService>();

            return services;
        }

        /// <summary>
        /// Register Ollama services for offline operation
        /// </summary>
        private static void RegisterOllamaServices(IServiceCollection services, RAGSettings settings)
        {
            // HttpClient for Ollama
            services.AddHttpClient("ollama", client =>
            {
                client.BaseAddress = new Uri(settings.OllamaEndpoint);
                client.Timeout = TimeSpan.FromSeconds(settings.OllamaRequestTimeoutSeconds);
            });

            // Register Ollama embedding service
            services.AddSingleton<IEmbeddingService>(sp =>
                new OllamaEmbeddingService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama"),
                    settings.OllamaEndpoint,
                    settings.OllamaModelName,
                    sp.GetRequiredService<ILogger<OllamaEmbeddingService>>()));

            // Register Ollama generation service
            services.AddSingleton<IGenerationService>(sp =>
                new OllamaGenerationService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama"),
                    settings.OllamaEndpoint,
                    settings.OllamaModelName,
                    sp.GetRequiredService<ILogger<OllamaGenerationService>>()));
        }



        /// <summary>
        /// Add RAG services using IOptions pattern for settings
        /// </summary>
        public static IServiceCollection AddRAGServices(this IServiceCollection services, 
            IConfigurationSection configSection)
        {
            var settings = configSection.Get<RAGSettings>() 
                ?? throw new InvalidOperationException("RAG settings not configured");

            return services.AddRAGServices(settings);
        }
    }
}
