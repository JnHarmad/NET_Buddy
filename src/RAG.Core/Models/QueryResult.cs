using System;
using System.Collections.Generic;
using System.Linq;

namespace RAG.Core.Models
{
    /// <summary>
    /// Represents the result of a RAG query with generated answer and context.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// The original query posed by the user
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// The generated answer
        /// </summary>
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Retrieved context chunks used to generate the answer
        /// </summary>
        public List<RetrievedContext> RetrievedContexts { get; set; } = new();

        /// <summary>
        /// Average relevance score of retrieved contexts
        /// </summary>
        public float AverageRelevanceScore => 
            RetrievedContexts.Any() ? RetrievedContexts.Average(c => c.RelevanceScore) : 0f;

        /// <summary>
        /// Time taken to generate the result
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Timestamp when the query was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Subject filter used (if any)
        /// </summary>
        public string? FilteredBySubject { get; set; }
    }
}
