using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RAG.Core.Models;
using RAG.Core.Abstractions;
using RAG.Infrastructure;

namespace RAG.CLI
{
    /// <summary>
    /// Console application for NET Buddy RAG system.
    /// Ollama-based offline study assistant.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("   NET Buddy - RAG Study Assistant");
            Console.WriteLine("═══════════════════════════════════════════════════════════\n");

            try
            {
                // Setup dependency injection
                var services = new ServiceCollection();

                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                services.AddLogging(builder => 
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                services.AddHttpClient();  // Required for Ollama integration

                var ragSettings = config.GetSection("RAG").Get<RAGSettings>() 
                    ?? throw new InvalidOperationException("RAG settings not found in configuration");
                Console.WriteLine($"🔧 Ollama Endpoint: {ragSettings.OllamaEndpoint}");
                Console.WriteLine($"🤖 Model: {ragSettings.OllamaModelName}");
                Console.WriteLine();
                services.AddRAGServices(ragSettings);
                var serviceProvider = services.BuildServiceProvider();

                // Get services
                var ingestionService = serviceProvider.GetRequiredService<IMaterialIngestionService>();
                var retrievalService = serviceProvider.GetRequiredService<IRetrievalService>();
                var generationService = serviceProvider.GetRequiredService<IGenerationService>();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Demo 1: Ingest sample material
                await DemoIngestMaterial(ingestionService, logger);

                // Demo 2: Retrieve context
                await DemoRetrieveContext(retrievalService, logger);

                // Demo 3: Generate answer
                await DemoGenerateAnswer(retrievalService, generationService, logger);

                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("   Demo Complete!");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("localhost"))
            {
                Console.WriteLine($"❌ Error: Cannot connect to Ollama");
                Console.WriteLine($"\nOllama is not running. To use offline mode:");
                Console.WriteLine($"1. Install Ollama: https://ollama.ai");
                Console.WriteLine($"2. Start Ollama: ollama serve");
                Console.WriteLine($"3. Pull model: ollama pull llama2");
                Console.WriteLine($"4. Run this application again\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"   Details: {ex.InnerException.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        static async Task DemoIngestMaterial(IMaterialIngestionService ingestionService, ILogger<Program> logger)
        {
            Console.WriteLine("📚 DEMO 1: INGESTING STUDY MATERIAL");
            Console.WriteLine("────────────────────────────────────\n");

            var sampleMaterial = @"
Photosynthesis is the process by which plants convert light energy into chemical energy. 
It occurs primarily in the leaves of green plants. The process has two main stages: the light-dependent reactions and the light-independent reactions (Calvin cycle).

Light-dependent reactions occur in the thylakoid membranes and involve the absorption of photons by chlorophyll molecules. 
Water molecules are split, releasing oxygen, electrons, and protons. The electrons move through the electron transport chain, 
generating ATP and NADPH which are used in the Calvin cycle.

The Calvin cycle occurs in the stroma and uses ATP and NADPH to convert carbon dioxide into glucose. 
This cycle involves three main steps: carbon fixation, reduction, and regeneration of RuBP.

Key molecules involved include chlorophyll, ATP synthase, and the enzyme RuBisCO which is the most abundant protein on Earth.
";

            Console.WriteLine("Ingesting sample material on Photosynthesis...\n");

            var stopwatch = Stopwatch.StartNew();
            var chunkCount = await ingestionService.IngestTextAsync(
                sampleMaterial,
                subject: "Biology",
                topic: "Photosynthesis",
                source: "Biology_Ch05_Photosynthesis");

            stopwatch.Stop();

            Console.WriteLine($"✅ Ingested {chunkCount} chunks in {stopwatch.ElapsedMilliseconds}ms");
            
            var stats = await ingestionService.GetStatsAsync();
            Console.WriteLine($"📊 Knowledge Base Stats:");
            Console.WriteLine($"   - Total Chunks: {stats.TotalChunks}");
            Console.WriteLine($"   - Subjects: {stats.TotalSubjects}\n");
        }

        static async Task DemoRetrieveContext(IRetrievalService retrievalService, ILogger<Program> logger)
        {
            Console.WriteLine("🔍 DEMO 2: RETRIEVING CONTEXT");
            Console.WriteLine("─────────────────────────────────\n");

            var query = "What are the light-dependent reactions?";
            Console.WriteLine($"Query: \"{query}\"\n");

            var stopwatch = Stopwatch.StartNew();
            var results = await retrievalService.RetrieveAsync(query, topK: 3);
            stopwatch.Stop();

            Console.WriteLine($"Retrieved in {stopwatch.ElapsedMilliseconds}ms:\n");

            var resultsList = results.ToList();
            for (int i = 0; i < resultsList.Count; i++)
            {
                var result = resultsList[i];
                Console.WriteLine($"Result {i + 1}:");
                Console.WriteLine($"  Subject: {result.Subject}");
                Console.WriteLine($"  Topic: {result.Topic}");
                Console.WriteLine($"  Relevance: {result.RelevanceScore:P}");
                Console.WriteLine($"  Content (preview): {result.Content[..Math.Min(100, result.Content.Length)]}...\n");
            }
        }

        static async Task DemoGenerateAnswer(
            IRetrievalService retrievalService,
            IGenerationService generationService,
            ILogger<Program> logger)
        {
            Console.WriteLine("🤖 DEMO 3: GENERATING ANSWER");
            Console.WriteLine("──────────────────────────────\n");

            var question = "Explain the role of ATP in photosynthesis.";
            Console.WriteLine($"Question: \"{question}\"\n");

            try
            {
                // Step 1: Retrieve relevant context
                Console.WriteLine("Retrieving context...");
                var contexts = await retrievalService.RetrieveAsync(question, topK: 2);
                var contextTexts = contexts.Select(c => c.Content).ToList();

                if (!contextTexts.Any())
                {
                    Console.WriteLine("⚠️ No context found for generation.\n");
                    return;
                }

                // Step 2: Generate answer
                Console.WriteLine("Generating answer (this may take a moment with Ollama)...\n");
                var stopwatch = Stopwatch.StartNew();
                var answer = await generationService.GenerateAnswerAsync(question, contextTexts);
                stopwatch.Stop();

                Console.WriteLine($"✅ Answer generated in {stopwatch.ElapsedMilliseconds}ms:\n");
                Console.WriteLine($"Answer:\n{answer}\n");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"⚠️ Connection issue: {ex.Message}");
                Console.WriteLine($"Make sure Ollama is running: ollama serve\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during generation: {ex.Message}\n");
            }
        }
    }
}
