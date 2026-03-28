# Getting Started with NET Buddy RAG System

## Prerequisites

1. **.NET 8.0 SDK** or later
   - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
   - Verify: `dotnet --version`

2. **Azure OpenAI Account**
   - Resources needed:
     - Text Embeddings 3 Small (`text-embedding-3-small`)
     - GPT-4 Turbo or GPT-4o (`gpt-4-turbo`)
   - Get your endpoint URL and API key

3. **VS Code** or **Visual Studio 2022+** (optional but recommended)

## Step 1: Project Setup

### Clone or Create Project
```bash
cd f:\NET_Buddy
```

### Build Solution
```bash
dotnet restore
dotnet build
```

### Verify Projects
```bash
# Should work without errors
dotnet --list-psf
```

## Step 2: Configure Azure OpenAI

### Get Azure Credentials
1. Go to [Azure Portal](https://portal.azure.com)
2. Create an Azure OpenAI resource
3. Note:
   - **Endpoint**: `https://your-resource.openai.azure.com/`
   - **API Key**: From "Keys and Endpoint" section

### Update appsettings.json
Edit `src/RAG.CLI/appsettings.json`:
```json
{
  "RAG": {
    "AzureOpenAIEndpoint": "https://your-resource.openai.azure.com/",
    "AzureOpenAIApiKey": "your-api-key-here",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ChatDeploymentName": "gpt-4-turbo"
  }
}
```

**⚠️ IMPORTANT**: Add this file to `.gitignore` to protect credentials!

## Step 3: Prepare Study Materials

### Create Materials Directory
```bash
mkdir materials
mkdir materials\Physics
mkdir materials\Chemistry
mkdir materials\Mathematics
```

### Add Your Materials
Place `.txt` or `.md` files in subdirectories:
```
materials/
├── Physics/
│   ├── mechanics.txt
│   ├── thermodynamics.txt
│   └── waves.md
├── Chemistry/
│   ├── organic.txt
│   └── inorganic.txt
└── Mathematics/
    └── calculus.txt
```

### Format Guidelines
- **Plain text files** (`.txt`): Supported
- **Markdown files** (`.md`): Supported
- **One topic per file** recommended
- **Clear structure** helps chunking

### Sample Content Format
```
# Photosynthesis

Photosynthesis is the process by which plants convert light energy 
into chemical energy. It occurs in two main stages...

## Stage 1: Light-Dependent Reactions

The light-dependent reactions occur in the thylakoid membranes...

## Stage 2: Calvin Cycle

The Calvin cycle uses ATP and NADPH from the light reactions...
```

## Step 4: Run the CLI

### First Run
```bash
cd src/RAG.CLI
dotnet run
```

### Expected Output
```
═══════════════════════════════════════════════════════════
   NET Buddy - RAG Study Assistant
═══════════════════════════════════════════════════════════

📚 DEMO 1: INGESTING STUDY MATERIAL
────────────────────────────────────

Ingesting sample material on Photosynthesis...

✅ Ingested 5 chunks in 123ms
📊 Knowledge Base Stats:
   - Total Chunks: 5
   - Subjects: 1

🔍 DEMO 2: RETRIEVING CONTEXT
─────────────────────────────────

Query: "What are the light-dependent reactions?"

Retrieved in 45ms:
...

🤖 DEMO 3: GENERATING ANSWER
──────────────────────────────
...
```

## Step 5: Programmatic Usage

### Basic RAG Pipeline
```csharp
// You can now write code that uses the RAG system
// See examples in Program.cs

var ingestion = serviceProvider.GetRequiredService<IMaterialIngestionService>();
var retrieval = serviceProvider.GetRequiredService<IRetrievalService>();
var generation = serviceProvider.GetRequiredService<IGenerationService>();

// 1. Ingest material
var chunks = await ingestion.IngestTextAsync(
    content: "Physics notes here...",
    subject: "Physics",
    topic: "Mechanics",
    source: "Chapter3.pdf"
);

// 2. Retrieve context
var results = await retrieval.RetrieveAsync("What is force?");

// 3. Generate answer
var answer = await generation.GenerateAnswerAsync(
    "What is force?",
    results.Select(r => r.Content)
);
```

## Step 6: Extend the System

### Add Custom Features
1. **For quiz generation**: See `GenerateQuizAsync` method
2. **For summaries**: See `GenerateRevisionSummaryAsync` method
3. **For explanations**: See `ExplainConceptAsync` method

### Example: Create a Study Session
```csharp
public class StudySession
{
    private readonly IRetrievalService _retrieval;
    private readonly IGenerationService _generation;
    
    public async Task QuizMeAsync(string topic, int questionCount = 5)
    {
        var context = await _retrieval.RetrieveAsync(topic);
        var quiz = await _generation.GenerateQuizAsync(
            string.Join("\n\n", context.Select(c => c.Content)),
            questionCount
        );
        Console.WriteLine(quiz);
    }
}
```

## Troubleshooting

### Issue: "Missing configuration"
**Solution**: Ensure `appsettings.json` exists in `RAG.CLI` directory with valid Azure credentials

### Issue: "Azure OpenAI API error"
**Steps**:
1. Verify endpoint URL format
2. Check API key validity
3. Ensure deployments exist in Azure
4. Check rate limits haven't been exceeded

### Issue: "File not found" during ingestion
**Solution**: 
- Verify file paths are correct
- Check Windows permissions
- Use absolute paths if needed

### Issue: "OutOfMemoryException" with large materials
**Solution**:
- Use `SentenceChunkingStrategy` instead of `FixedSizeChunkingStrategy`
- Reduce `ChunkSize` parameter
- Process materials in batches

## Configuration Tuning

### For Better Search Results
```json
{
  "RAG": {
    "RetrievalTopK": 8,           // Increase for more context
    "ChunkSize": 1000,             // Larger chunks for more context
    "RelevanceThreshold": 0.6      // Stricter threshold
  }
}
```

### For Faster Ingestion
```json
{
  "RAG": {
    "ChunkSize": 500,              // Smaller chunks = faster
    "ChunkingStrategy": "Sentence" // Simpler strategy
  }
}
```

### For Better Answers (Slower)
```json
{
  "RAG": {
    "RetrievalTopK": 10,           // More context
    "ChunkSize": 1200,             // More detailed chunks
    "ChunkOverlap": 200            // More context continuity
  }
}
```

## Next Steps

1. **Ingest your materials**: Place files in `materials/` and run CLI
2. **Test retrieval**: Try different queries
3. **Generate quizzes**: Use `GenerateQuizAsync` for practice
4. **Build UI**: Create a web interface using Blazor or ASP.NET Core
5. **Implement persistence**: Switch to SQLite/Pinecone for scalability
6. **Add spaced repetition**: Track quiz performance and schedule reviews

## Additional Resources

- [README.md](../README.md) - Project overview and features
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design details
- [.instructions.md](../.instructions.md) - Coding standards
- [Azure OpenAI Docs](https://aka.ms/oai/docs)

## Quick Commands

```bash
# Build
dotnet build

# Run
cd src/RAG.CLI && dotnet run

# Run tests (when available)
dotnet test

# Restore packages
dotnet restore

# Clean
dotnet clean
```

---

**Last Updated**: March 28, 2026
**Ready to start? Begin here →** [Configure Azure OpenAI](#step-2-configure-azure-openai)
