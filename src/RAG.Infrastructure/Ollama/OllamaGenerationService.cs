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
    /// Generation service using local Ollama for offline operation.
    /// Supports answers, summaries, quizzes, and explanations using local LLM.
    /// </summary>
    public class OllamaGenerationService : IGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaEndpoint;
        private readonly string _modelName;
        private readonly ILogger<OllamaGenerationService> _logger;

        public OllamaGenerationService(
            HttpClient httpClient,
            string ollamaEndpoint,
            string modelName,
            ILogger<OllamaGenerationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ollamaEndpoint = ollamaEndpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(ollamaEndpoint));
            _modelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks)
        {
            try
            {
                _logger.LogInformation("Generating answer using Ollama for question: {Question}", question);

                var contextText = string.Join("\n\n", contextChunks);
                var prompt = BuildAnswerPrompt(question, contextText);

                var answer = await CallOllamaAsync(prompt);
                _logger.LogInformation("Answer generated successfully");
                return answer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating answer");
                throw;
            }
        }

        public async Task<string> GenerateRevisionSummaryAsync(string topic, IEnumerable<string> materialChunks, int pointsCount = 5)
        {
            try
            {
                _logger.LogInformation("Generating revision summary for topic: {Topic}", topic);

                var material = string.Join("\n\n", materialChunks);
                var prompt = $@"Based on the following study material, create a quick revision summary for the topic: {topic}

Provide exactly {pointsCount} key points in a bulleted format. Be concise and focus on the most important concepts.

Study Material:
{material}

Format:
- Point 1: 
- Point 2: 
(and so on)";

                var summary = await CallOllamaAsync(prompt);
                _logger.LogInformation("Revision summary generated");
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revision summary");
                throw;
            }
        }

        public async Task<string> GenerateQuizAsync(string material, int questionCount = 5)
        {
            try
            {
                _logger.LogInformation("Generating {QuestionCount} quiz questions", questionCount);

                var prompt = $@"Based on the following study material, generate {questionCount} exam-style questions.
Include a mix of multiple-choice, short-answer, and conceptual questions.
For each question, provide the answer.

Study Material:
{material}

Format your response as:
Question 1: [question text]
Answer 1: [answer]
Question 2: [question text]
Answer 2: [answer]
(and so on)";

                var quiz = await CallOllamaAsync(prompt);
                _logger.LogInformation("Quiz generated successfully");
                return quiz;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quiz");
                throw;
            }
        }

        public async Task<string> ExplainConceptAsync(string concept, IEnumerable<string> materialChunks)
        {
            try
            {
                _logger.LogInformation("Explaining concept: {Concept}", concept);

                var material = string.Join("\n\n", materialChunks);
                var prompt = $@"Explain the following concept in simple, easy-to-understand terms for exam preparation:

Concept: {concept}

Use the provided study material as reference:
{material}

Keep the explanation:
- Concise but comprehensive
- Easy to understand
- Focused on exam-relevant aspects
- Include examples if helpful";

                var explanation = await CallOllamaAsync(prompt);
                _logger.LogInformation("Concept explanation generated");
                return explanation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error explaining concept");
                throw;
            }
        }

        /// <summary>
        /// Call Ollama API with a prompt and get generated response.
        /// </summary>
        private async Task<string> CallOllamaAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = _modelName,
                    prompt = prompt,
                    stream = false,
                    temperature = 0.3f  // Lower for more deterministic answers
                };

                var httpContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");

                _logger.LogDebug("Calling Ollama endpoint: {Endpoint}", _ollamaEndpoint);
                
                var response = await _httpClient.PostAsync(
                    $"{_ollamaEndpoint}/api/generate",
                    httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Ollama API error ({response.StatusCode}): {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (!root.TryGetProperty("response", out var responseElement))
                    throw new InvalidOperationException("No response found in Ollama output");

                var generatedText = responseElement.GetString() ?? string.Empty;
                _logger.LogDebug("Generated text length: {Length}", generatedText.Length);

                return generatedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Ollama API at {Endpoint}", _ollamaEndpoint);
                throw;
            }
        }

        private string BuildAnswerPrompt(string question, string context)
        {
            return $@"You are a knowledgeable exam assistant. Answer this question based on the provided study material.

Question: {question}

Study Material:
{context}

Provide a clear, concise, and accurate answer. Include relevant concepts where applicable.";
        }
    }
}
