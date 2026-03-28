using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAG.Core.Abstractions
{
    /// <summary>
    /// Service for generating answers and quiz questions using Azure OpenAI.
    /// </summary>
    public interface IGenerationService
    {
        /// <summary>
        /// Generate an answer to a question based on retrieved context
        /// </summary>
        /// <param name="question">The question to answer</param>
        /// <param name="contextChunks">Retrieved context chunks</param>
        Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks);

        /// <summary>
        /// Generate a quick revision summary for a topic
        /// </summary>
        Task<string> GenerateRevisionSummaryAsync(string topic, IEnumerable<string> materialChunks, int pointsCount = 5);

        /// <summary>
        /// Generate exam-style quiz questions
        /// </summary>
        /// <param name="material">Study material to generate questions from</param>
        /// <param name="questionCount">Number of questions to generate</param>
        Task<string> GenerateQuizAsync(string material, int questionCount = 5);

        /// <summary>
        /// Explain a concept in simple terms
        /// </summary>
        Task<string> ExplainConceptAsync(string concept, IEnumerable<string> materialChunks);
    }
}
