# RAG System Architecture - Detailed Design

## System Components

### 1. **Models Layer** (`RAG.Core.Models`)

#### MaterialChunk
- Represents a chunk of study material with embeddings
- Properties: Content, Embedding, Subject, Topic, Source, ChunkIndex, DifficultyLevel
- Estimated token count calculation

#### RetrievedContext  
- Retrieved chunk with relevance score
- Used to pass context to generation services
- Includes metadata (subject, topic, source, difficulty)

#### QueryResult
- Final output of RAG pipeline
- Contains: query, answer, retrieved contexts, execution time
- Average relevance score calculation

### 2. **Core Services** (`RAG.Core.Services`)

#### ChunkingStrategies
- **FixedSizeChunkingStrategy**: Splits into fixed-length chunks with overlap
  - Configurable chunk size (default: 800 words)
  - Configurable overlap (default: 100 words)
  - Ignores very small chunks (<10 words)
  
- **SentenceChunkingStrategy**: Respects sentence boundaries
  - Groups by sentences instead of word count
  - Better for narrative material
  - Configurable sentences per chunk (default: 5)

#### InMemoryVectorStore
- Implements `IVectorStore` interface
- Thread-safe dictionary-based storage
- Features:
  - Add single/batch chunks
  - Semantic search via Euclidean distance
  - Metadata filtering (subject, topic, difficulty)
  - Get chunk by ID, delete, clear, get stats
  
- Similarity calculation:
  ```
  distance = sqrt(sum((e1[i] - e2[i])^2))
  Lower distance = higher similarity
  ```

#### RetrievalService
- Orchestrates embedding + vector store search
- Steps:
  1. Embed query using EmbeddingService
  2. Search vector store with optional subject filter
  3. Convert results to RetrievedContext with relevance scores
  4. Relevance score = (cosine_similarity + 1) / 2 (0-1 range)
  
- Supports:
  - Query string retrieval
  - Direct embedding retrieval

#### MaterialIngestionService
- Orchestrates chunking + embedding + storage
- Steps:
  1. Chunk material using selected strategy
  2. Generate embeddings in batch
  3. Assign embeddings to chunks
  4. Store all chunks in vector store
  
- Features:
  - Ingest from text, files, directories
  - Batch processing optimization
  - Statistics tracking

### 3. **Azure OpenAI Integration** (`RAG.Infrastructure.AzureOpenAI`)

#### AzureOpenAIEmbeddingService
- Uses Azure OpenAI embeddings API
- Caches embedding dimension
- Supports:
  - Single text embedding
  - Batch embedding
  - Dimension detection (typically 1536)

#### AzureOpenAIGenerationService
- Uses Azure OpenAI chat completion API
- Features:
  - **GenerateAnswer**: QA with context (Low temperature: 0.3)
  - **GenerateRevisionSummary**: Summarize in N key points (Temperature: 0.3)
  - **GenerateQuiz**: Create exam-style questions (Temperature: 0.5)
  - **ExplainConcept**: Explain concept using material (Temperature: 0.3)

### 4. **Dependency Injection** (`ServiceCollectionExtensions`)
- Configures all services
- Registers based on settings
- Supports strategy selection via configuration

## Data Flow Diagrams

### Material Ingestion Flow
```
Raw Material
    ↓
Parse/Validate
    ↓
Apply Chunking Strategy
    ├─[FixedSize]     → Fixed-size chunks with overlap
    └─[Sentence]      → Sentence-boundary chunks
    ↓
Extract Chunk Texts
    ↓
Batch Embed via Azure OpenAI
    ├─ Group texts
    ├─ Call API
    └─ Receive embeddings
    ↓
Assign Embeddings to Chunks
    ↓
Store in Vector Store
    ├─ Add to dictionary
    ├─ Index by ID
    └─ Enable metadata filtering
    ↓
Return Stats (Chunks Count)
```

### Retrieval Flow
```
User Query
    ↓
Validate Query
    ↓
Embed Query via Azure OpenAI
    ├─ Single embedding call
    └─ Receive ~1536 dimensions
    ↓
Search Vector Store
    ├─ Optional Subject Filter
    ├─ Calculate distance to all chunks
    ├─ Sort by distance (ascending)
    └─ Take top-K results
    ↓
Convert to RetrievedContext
    ├─ Calculate Relevance Score
    │   (Using Cosine Similarity)
    ├─ Add metadata
    └─ Sort by relevance
    ↓
Return Contexts
```

### Generation Flow
```
Query + Retrieved Contexts
    ↓
Build Prompt
    ├─ System message
    ├─ Context injection
    └─ Question/task
    ↓
Call Azure OpenAI Chat
    ├─ Temperature setting
    ├─ Max tokens
    └─ Model deployment
    ↓
Parse Response
    ↓
Return Generated Text
    ├─ Answer
    ├─ Quiz
    ├─ Summary
    └─ Explanation
```

## Performance Characteristics

### Time Complexity
- **Embedding a query**: O(1) - Single API call
- **Vector search**: O(n) - Linear scan (in-memory)
  - Where n = total chunks
  - ~100-300ms for 1000 chunks
- **Batch embedding**: O(m) - Proportional to batch size
  - Where m = number of chunks
  - API rate limits may apply

### Space Complexity
- **Per chunk**: ~6-8KB
  - Content (variable, ~2-3KB)
  - Embedding (1536 × 4 bytes = 6KB)
  - Metadata (minimal)
- **Example**: 10,000 chunks ≈ 60-80MB RAM

### Latency (Typical)
- Query embedding: 200-500ms
- Vector search (1000 chunks): 50-150ms
- Generation (chat completion): 1-3 seconds
- **Total RAG latency**: 1.5-4 seconds

## Extensibility Points

### 1. Alternative Vector Stores
Replace `InMemoryVectorStore` with:
- SQLite + FAISS
- Pinecone
- Qdrant
- Weaviate
- Milvus

Implement `IVectorStore` interface:
```csharp
public interface IVectorStore
{
    Task AddChunkAsync(MaterialChunk chunk);
    Task<IEnumerable<MaterialChunk>> SearchAsync(float[] embedding, int topK, Dictionary<string, object>? filters);
    // ... etc
}
```

### 2. Alternative Chunking Strategies
Implement `IChunkingStrategy`:
```csharp
public interface IChunkingStrategy
{
    IEnumerable<MaterialChunk> ChunkMaterial(string material, string source, string subject, string topic);
}
```

New strategies:
- **Semantic/Recursive**: Use embeddings to split intelligently
- **Markdown**: Parse markdown structure
- **PDF**: Extract from PDF with structure
- **Sliding Window**: Advanced windowing techniques

### 3. Alternative Embedding Services
Implement `IEmbeddingService` for:
- Hugging Face models
- OpenAI (non-Azure)
- Local models (Ollama)
- Cohere API

### 4. Alternative Generation Services
Implement `IGenerationService` for:
- Local LLMs (Llama, Mistral)
- OpenAI (non-Azure)
- Anthropic Claude
- Google Palm

### 5. Caching Layer
Wrap `IRetrievalService`:
```csharp
public class CachedRetrievalService : IRetrievalService
{
    private readonly IRetrievalService _inner;
    private readonly IMemoryCache _cache;
    // Cache query results
}
```

## Testing Strategy

### Unit Tests (Recommendations)
- `ChunkingStrategies` - Verify chunk sizes and overlap
- `InMemoryVectorStore` - Test CRUD operations, search accuracy
- `RetrievalService` - Mock embedding/vector store
- `Services` - Mock dependencies

### Integration Tests
- End-to-end RAG pipeline with test data
- Mock Azure OpenAI if not available
- Verify prompt construction

### Performance Tests
- Measure latency for various chunk counts
- Memory usage profiling
- Batch embedding throughput

## Security Considerations

1. **API Key Management**
   - Store in Azure Key Vault (production)
   - Use environment variables
   - Never commit credentials

2. **Input Validation**
   - Sanitize user queries for prompt injection
   - Validate material content
   - Size limits on inputs

3. **Data Privacy**
   - Materials stay local (by default)
   - Embeddings sent to Azure OpenAI
   - Consider compliance (GDPR, etc.)

## Future Enhancements

1. **Advanced Retrieval**
   - Hybrid search (keyword + semantic)
   - Re-ranking with cross-encoder
   - Query expansion

2. **Learning Features**
   - Spaced repetition scheduling
   - Performance tracking
   - Weak topic identification

3. **UI/UX**
   - Web interface
   - Chat-based interaction
   - Flashcard system

4. **Optimization**
   - Persistent vector store
   - Caching layer
   - Batch processing pipelines

5. **Multi-modal**
   - Image text extraction
   - Diagram analysis
   - Audio transcription

---

**Document Version**: 1.0
**Last Updated**: March 28, 2026
