namespace RAG.Core.Models
{
    /// <summary>
    /// Configuration settings for the RAG system.
    /// Ollama-only implementation for offline operation.
    /// </summary>
    public class RAGSettings
    {
        // Ollama Settings (local, offline operation)
        public string OllamaEndpoint { get; set; } = "http://localhost:11434";
        public string OllamaModelName { get; set; } = "llama2"; // llama2, mistral, neural-chat, etc.
        public int OllamaRequestTimeoutSeconds { get; set; } = 300; // Ollama can be slow on CPU

        // Vector Store Settings
        public string VectorStoreType { get; set; } = "InMemory"; // InMemory, SQLite, etc.
        public string? VectorStoreConnectionString { get; set; }

        // RAG Parameters
        public int RetrievalTopK { get; set; } = 5;
        public int ChunkSize { get; set; } = 800;
        public int ChunkOverlap { get; set; } = 100;
        public float RelevanceThreshold { get; set; } = 0.5f;
        
        // Chunking Strategy
        public string ChunkingStrategy { get; set; } = "Fixed"; // Fixed, Semantic, Sentence
        
        // Cache Settings
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 60;

        // Logging
        public string LogLevel { get; set; } = "Information";
        public bool LogEmbeddings { get; set; } = false;
    }
}
