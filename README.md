# NET Buddy - RAG Study Assistant for NET JRF Exam Preparation

A complete Retrieval-Augmented Generation (RAG) system built with .NET, designed to help NET JRF exam candidates study more effectively using their own study materials.

## 📋 Project Overview

**NET Buddy** is a generative AI system that:
- Ingests your study materials (PDFs, notes, chapters)
- Chunks material intelligently for better context
- Generates embeddings for semantic search
- Retrieves relevant context for any question
- Generates exam-focused answers and quizzes
- Helps with active recall and spaced repetition

## 🏗️ Architecture

### Three-Tier Design

```
┌─────────────────────────────────────────────────────┐
│         RAG.CLI (Console Application)               │
│  - Material ingestion interface                      │
│  - Query interface                                   │
│  - Results display                                   │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│  RAG.Core (Business Logic & Services)               │
│  ├─ Abstractions (Interfaces)                       │
│  └─ Services                                        │
│     ├─ ChunkingStrategies                           │
│     ├─ RetrievalService                             │
│     ├─ MaterialIngestionService                     │
│     └─ InMemoryVectorStore                          │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│  RAG.Infrastructure (External Integrations)         │
│  ├─ AzureOpenAIEmbeddingService                     │
│  ├─ AzureOpenAIGenerationService                    │
│  └─ ServiceCollectionExtensions (DI Setup)          │
└─────────────────────────────────────────────────────┘
```

### Data Flow

```
Study Material
    ↓
[Chunking] → FixedSize/Sentence chunking
    ↓
[Embedding] → Azure OpenAI embeddings
    ↓
[Storage] → Vector Store (In-Memory)
    ↓
User Query
    ↓
[Embed Query] → Create embedding
    ↓
[Retrieve] → Find similar chunks (cosine similarity)
    ↓
[Generate] → Use Azure OpenAI with context
    ↓
Answer/Quiz/Summary
```

## 📦 Project Structure

```
NET_Buddy/
├── src/
│   ├── RAG.Core/
│   │   ├── Models/
│   │   │   ├── MaterialChunk.cs           # Study material chunk representation
│   │   │   ├── RetrievedContext.cs       # Retrieved chunk with score
│   │   │   ├── QueryResult.cs             # Final RAG output
│   │   │   └── RAGSettings.cs             # Configuration model
│   │   ├── Abstractions/
│   │   │   ├── IEmbeddingService.cs      # Embedding abstraction
│   │   │   ├── IVectorStore.cs            # Vector store abstraction
│   │   │   ├── IRetrievalService.cs      # Retrieval abstraction
│   │   │   ├── IGenerationService.cs     # Generation abstraction
│   │   │   ├── IChunkingStrategy.cs      # Chunking strategy abstraction
│   │   │   └── IMaterialIngestionService.cs # Ingestion abstraction
│   │   └── Services/
│   │       ├── ChunkingStrategies.cs     # Fixed & Sentence chunking
│   │       ├── InMemoryVectorStore.cs    # In-memory vector store
│   │       ├── RetrievalService.cs       # Retrieval implementation
│   │       └── MaterialIngestionService.cs # Ingestion implementation
│   │
│   ├── RAG.Infrastructure/
│   │   ├── AzureOpenAI/
│   │   │   ├── AzureOpenAIEmbeddingService.cs  # Embedding via Azure
│   │   │   └── AzureOpenAIGenerationService.cs # Generation via Azure
│   │   └── ServiceCollectionExtensions.cs      # DI setup
│   │
│   └── RAG.CLI/
│       ├── Program.cs                   # Entry point & demos
│       └── RAG.CLI.csproj
│
├── config/
│   └── appsettings.json               # Configuration
│
├── materials/                          # Your study materials (gitignored)
│   └── (place .txt or .md files here)
│
├── .agent.md                          # Custom Copilot agent config
├── .instructions.md                   # Coding standards
└── README.md                          # This file
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Azure OpenAI account (for embeddings and completion)
- VS Code or Visual Studio

### 1. Clone/Setup
```bash
cd f:\NET_Buddy\src\RAG.CLI
```

### 2. Configure Azure OpenAI
Edit `config/appsettings.json`:
```json
{
  "RAG": {
    "AzureOpenAIEndpoint": "https://your-resource.openai.azure.com/",
    "AzureOpenAIApiKey": "your-api-key",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ChatDeploymentName": "gpt-4-turbo"
  }
}
```

### 3. Prepare Study Materials
Place your study materials in `materials/` folder:
```
materials/
├── physics_notes.txt
├── chemistry_chapter5.md
└── previous_papers.txt
```

### 4. Run the CLI
```bash
dotnet run
```

### 5. Interact with the System
The CLI will:
1. Ingest sample material (demo)
2. Retrieve relevant context for a query
3. Generate an answer using the context

## 🔧 Usage Examples

### Programmatic Usage

```csharp
// Setup
var services = new ServiceCollection();
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

services.AddLogging(b => b.AddConsole());
services.AddRAGServices(config.GetSection("RAG"));
var sp = services.BuildServiceProvider();

// Get services
var ingestion = sp.GetRequiredService<IMaterialIngestionService>();
var retrieval = sp.GetRequiredService<IRetrievalService>();
var generation = sp.GetRequiredService<IGenerationService>();

// Ingest material
var chunkCount = await ingestion.IngestTextAsync(
    materialContent,
    subject: "Biology",
    topic: "Photosynthesis",
    source: "chapter5.pdf"
);

// Retrieve context
var context = await retrieval.RetrieveAsync("How does photosynthesis work?");

// Generate answer
var answer = await generation.GenerateAnswerAsync(
    question: "Explain the light-dependent reactions",
    contextChunks: context.Select(c => c.Content)
);
```

### Quiz Generation
```csharp
var quiz = await generation.GenerateQuizAsync(
    material: studyMaterial,
    questionCount: 10
);
```

### Revision Summaries
```csharp
var summary = await generation.GenerateRevisionSummaryAsync(
    topic: "Photosynthesis",
    materialChunks: contextChunks,
    pointsCount: 5
);
```

### Concept Explanation
```csharp
var explanation = await generation.ExplainConceptAsync(
    concept: "ATP Synthesis",
    materialChunks: contextChunks
);
```

## ⚙️ Configuration

### RAG Settings (appsettings.json)

| Setting | Default | Description |
|---------|---------|-------------|
| `RetrievalTopK` | 5 | Number of context chunks to retrieve |
| `ChunkSize` | 800 | Words per chunk |
| `ChunkOverlap` | 100 | Overlap between chunks |
| `ChunkingStrategy` | Fixed | Chunking approach: Fixed or Sentence |
| `RelevanceThreshold` | 0.5 | Minimum similarity score (0-1) |
| `EnableCaching` | true | Cache query results |
| `CacheDurationMinutes` | 60 | Cache expiration time |

### Chunking Strategies

**FixedSizeChunking** (default)
- Splits into fixed-length chunks with overlap
- Better for dense/technical material
- Configurable via `ChunkSize` and `ChunkOverlap`

**SentenceChunkingStrategy**
- Respects sentence boundaries
- Better for narrative material
- Configurable via `sentencesPerChunk`

## 🔍 Vector Search Details

### Similarity Metrics
- **Distance Metric**: Euclidean distance
- **Similarity Score**: Cosine similarity (converted to 0-1 range)
- **Search**: KNN (K-Nearest Neighbors)

### Performance
- **In-Memory Store**: ~O(n) per search (suitable for <100k chunks)
- **Embedding Dimension**: ~1536 (text-embedding-3-small)
- **Query Embedding Time**: ~100-500ms per query

## 🧪 Testing

### Unit Tests (Work in Progress)
```bash
dotnet test src/RAG.Tests
```

### Manual Testing
```bash
cd src/RAG.CLI
dotnet run
```

## 🌱 For NET JRF Preparation

### Recommended Material Organization
```
materials/
├── Physics/
│   ├── mechanics.txt
│   ├── thermodynamics.txt
│   └── optics.txt
├── Chemistry/
│   ├── organic.txt
│   ├── inorganic.txt
│   └── physical_chemistry.txt
└── Mathematics/
    ├── calculus.txt
    └── algebra.txt
```

### Study Features
1. **Active Recall**: System generates quiz questions
2. **Spaced Repetition**: (Future) Schedule reviews based on performance
3. **Concept Clarification**: Ask for simple explanations
4. **Quick Revisions**: Get 5-point summaries before exams
5. **Previous Papers**: Ingest past exam questions and model answers

## 📈 Roadmap

- [ ] **SQLite Vector Store**: Replace in-memory with persistent storage
- [ ] **PDF/Text Parsing**: Auto-extract text from various formats
- [ ] **Semantic Chunking**: Use embeddings for intelligent chunking
- [ ] **Re-ranking**: Improve retrieval with cross-encoder models
- [ ] **Chat Interface**: Web UI for interactive study sessions
- [ ] **Spaced Repetition**: Track progress and schedule reviews
- [ ] **Multi-Modal**: Support images and diagrams
- [ ] **Performance Analytics**: Track which topics need more study

## 🔐 Security & Privacy

- API keys stored in `appsettings.json` (local only)
- Use Azure Key Vault for production
- Study materials remain local by default
- No data sent to third parties (except embeddings to Azure OpenAI)

## 🤝 Contributing

This is a personal project for NET JRF prep. Feel free to extend:
- Add new chunking strategies
- Implement different vector stores (Pinecone, Qdrant, etc.)
- Build a web UI
- Add spaced repetition logic
- Create flashcards feature

## 📚 References

- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [Azure OpenAI API](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
- [RAG Patterns](https://aka.ms/rag-patterns)
- [NET Exam Guidelines](https://cstchyd.ac.in/net-exam-guidelines)

## 📝 License

Personal educational project - feel free to use and modify for learning.

---

**Last Updated**: March 28, 2026
**Status**: Core RAG architecture complete, ready for extension
