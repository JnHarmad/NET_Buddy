# Ollama Setup Guide for NET Buddy - Offline AI

This guide shows you how to set up **Ollama** for completely offline, local AI-powered study assistance.

---

## ⚡ Quick Start (5 minutes)

### 1. **Install Ollama**

#### Windows
1. Download: https://ollama.ai/download
2. Run the installer
3. Follow the prompts
4. Restart your computer

#### Mac
```bash
brew install ollama
```

#### Linux (Ubuntu/Debian)
```bash
curl https://ollama.ai/install.sh | sh
```

---

### 2. **Pull the Model**

Open PowerShell/Terminal and run:

```bash
ollama pull llama2
```

This downloads the Llama 2 13B model (~7GB). First time only.

**First Pull Output:**
```
pulling manifest
pulling 8934d386d91e
pulling 8c17c2ebb0ea
pulling 5c40d7af7fc5
...
✓ Done
```

---

### 3. **Start Ollama Server**

```bash
ollama serve
```

You'll see:
```
time=2024-01-15T10:30:45.123Z level=INFO msg="Listening on 127.0.0.1:11434"
```

**Leave this terminal open!**

---

### 4. **Run NET Buddy**

In a **new terminal/PowerShell**:

```bash
cd f:\NET_Buddy\src\RAG.CLI
dotnet run
```

You'll see output with demo ingestion and retrieval working offline!

---

## 📊 Model Comparison & Recommendations

### For NET JRF Examination Prep

| Model | Size | RAM | Speed | Quality | Recommendation |
|-------|------|-----|-------|---------|-----------------|
| **Llama 2 7B** | 4GB | 6-8GB | ⚡⚡ Fast | Good | Fastest, great for quick study |
| **Llama 2 13B** ✅ | 7GB | 10-12GB | ⚡ Moderate | **Excellent** | Best balance (RECOMMENDED) |
| **Mistral 7B** | 4GB | 6-8GB | ⚡⚡ Fast | Very Good | Alternative to Llama 7B |
| Llama 2 70B | 39GB | 48GB+ | 🐢 Slow | Outstanding | High-end systems only |

### Recommendation for You
**Llama 2 13B** - You selected this, perfect choice:
- ✅ Excellent quality for study material
- ✅ Good balance of speed and quality
- ✅ Reasonable on modern laptops (10-12GB RAM)
- ✅ Great for exam question generation

---

## 🔧 Configuration

### Default Settings (appsettings.json)

```json
{
  "RAG": {
    "LLMProvider": "Ollama",
    "OllamaEndpoint": "http://localhost:11434",
    "OllamaModelName": "llama2",
    "OllamaRequestTimeoutSeconds": 300
  }
}
```

### Switch Models

**To use Llama 2 7B (faster)**:
```bash
ollama pull llama2:7b
```

Then in `appsettings.json`:
```json
"OllamaModelName": "llama2:7b"
```

**Other models**:
```bash
ollama pull mistral
ollama pull neural-chat
ollama pull dolphin-mixtral
```

---

## 📋 Full Ollama Installation Walkthrough

### Windows Step-by-Step

#### Step 1: Download
- Go to https://ollama.ai/download
- Click "Download for Windows"
- File: `OllamaSetup.exe` (~200MB)

#### Step 2: Install
- Run `OllamaSetup.exe`
- Click "Install"
- Wait 2-3 minutes
- Close installer

#### Step 3: Verify
Try opening PowerShell:
```powershell
ollama --version
```

Should show version number ✅

#### Step 4: Pull Model
```powershell
ollama pull llama2
```

**First time**: Takes 5-10 minutes (7GB download)
**Output progress**:
```
pulling manifest
pulling 8934d386d91e
[=======>          ] 2.1 GB / 7.0 GB
```

#### Step 5: Start Server
```powershell
ollama serve
```

Leave this running. Best to use a dedicated terminal.

---

### Test Ollama Works

In **new PowerShell/Terminal**:

```powershell
curl http://localhost:11434/api/tags
```

Response:
```json
{
  "models": [
    {
      "name": "llama2:latest",
      ...
    }
  ]
}
```

✅ If you see models, everything working!

---

## 🚀 Using NET Buddy with Ollama

### Typical Workflow

```bash
# Terminal 1: Start Ollama (leave running)
ollama serve

# Terminal 2: Run NET Buddy
cd f:\NET_Buddy\src\RAG.CLI
dotnet run
```

### What Happens

1. **Ingestion**: Material is chunked and embedded using Llama 2
2. **Retrieval**: Your query is embedded and most relevant chunks found
3. **Generation**: Llama 2 generates answer using your material

All locally, no API calls, **completely offline**.

---

## 💡 Tips & Tricks

### Faster Performance

**Use Llama 2 7B instead of 13B**:
```bash
ollama pull llama2:7b
# Then set OllamaModelName: "llama2:7b" in appsettings.json
```

**Increase timeout for slow systems** (appsettings.json):
```json
"OllamaRequestTimeoutSeconds": 600
```

---

### Run Multiple Models

Keep both loaded:
```bash
ollama pull llama2:7b
ollama pull mistral
```

Switch between them in `appsettings.json` by changing `OllamaModelName`.

---

### Monitor Performance

Check current resource usage:
```bash
# In separate terminal while running
tasklist | find "ollama"
```

Or use Task Manager → Performance tab

---

## 🐛 Troubleshooting

### Issue: "Cannot connect to Ollama"
```
Error: HttpRequestException: No connection could be made
```

**Solution**:
1. Make sure `ollama serve` is running
2. Check terminal started with `ollama serve` is still open
3. Verify `http://localhost:11434` is correct endpoint
4. Wait a few seconds after starting server

---

### Issue: "Model not found"
```
Ollama API error (400): model not found
```

**Solution**:
1. Pull the model: `ollama pull llama2`
2. Verify it's there: `ollama list`
3. Check spelling in `appsettings.json`

---

### Issue: "Out of memory"
```
Error: CUDA out of memory
```

**Solution**:
1. Close other applications
2. Switch to Llama 2 7B: `ollama pull llama2:7b`
3. Set `OllamaModelName: "llama2:7b"`

---

### Issue: "Very slow responses"
Generation takes >30 seconds

**Solution**:
1. You're likely using CPU-only (no GPU)
2. Try smaller model: `ollama pull mistral`
3. Or just accept slower speed (Llama is still fast on CPU)

---

## 🎓 NET JRF Study Workflow with Ollama

### Example Session

```bash
# 1. Terminal 1: Start Ollama in background
ollama serve

# 2. Terminal 2: Run NET Buddy
cd f:\NET_Buddy\src\RAG.CLI
dotnet run

# 3. Output shows:
# ✅ Ingested 5 chunks
# ✅ Retrieved 3 relevant contexts
# ✅ Generated answer about photosynthesis

# 4. Now ready to use programmatically or extend CLI
```

### Programmatic Usage

```csharp
// In your code
var ingestion = serviceProvider.GetRequiredService<IMaterialIngestionService>();
var retrieval = serviceProvider.GetRequiredService<IRetrievalService>();
var generation = serviceProvider.GetRequiredService<IGenerationService>();

// Ingest your physics notes
await ingestion.IngestTextAsync(physicsContent, "Physics", "Mechanics", "Chapter3.pdf");

// Ask a question
var contexts = await retrieval.RetrieveAsync("What is Newton's second law?");

// Generate answer
var answer = await generation.GenerateAnswerAsync(
    "What is Newton's second law?",
    contexts.Select(c => c.Content)
);

Console.WriteLine(answer);  // Fully offline answer!
```

---

## 🔒 Privacy & Security

✅ **All benefits with Ollama**:
- ✅ No data sent to cloud
- ✅ Study materials stay local
- ✅ No subscriptions needed
- ✅ Works offline (no internet)
- ✅ Completely private
- ✅ Free & open-source

---

## 📚 More Information

- **Ollama Documentation**: https://github.com/ollama/ollama
- **Model Library**: https://ollama.ai/library
- **Supported Models**: llama2, mistral, neural-chat, dolphin-mixtral, orca-mini, and more

---

## ✨ Next Steps

1. ✅ Install Ollama
2. ✅ Pull Llama 2 model
3. ✅ Start `ollama serve`
4. ✅ Run NET Buddy
5. 🚀 Add your study materials to `materials/` folder
6. 📖 Ask questions about your content!

---

**You're now ready for completely offline AI-powered exam prep!** 🎓

Questions? Refer back to [Troubleshooting](#-troubleshooting) section.
