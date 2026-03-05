# RAG System

A Retrieval Augmented Generation system built with .NET 10, Qdrant VectorDB, and Gemini AI. Ask questions about your own documents — the system finds the most relevant information and generates accurate answers without hallucination.

## How It Works

```
Document Indexing:
  Document arrives
    → Split into chunks (500 words, 50 word overlap)
    → Each chunk converted to a 3072-dimensional vector via Gemini Embeddings
    → Vectors stored in Qdrant with metadata

Question Answering:
  Question arrives
    → Question converted to a vector
    → Most similar chunks retrieved from Qdrant
    → "Answer ONLY based on this context" + chunks + question → Gemini
    → Gemini generates a grounded answer
    → Answer + sources returned to user
```

## Architecture

Clean Architecture with 4 layers:

```
rag-system/
├── docker-compose.yml
├── .env
└── src/
    ├── RAG.Domain/
    │   ├── Entities/
    │   │   ├── Document.cs           # Document with status management
    │   │   ├── DocumentChunk.cs      # Chunk with embedding
    │   │   ├── Conversation.cs       # Conversation with message history
    │   │   └── Message.cs            # Message with role and citation
    │   └── Interfaces/
    │       ├── IEmbeddingService.cs
    │       ├── IVectorRepository.cs
    │       ├── IDocumentRepository.cs
    │       ├── IConversationRepository.cs
    │       └── ILanguageModelService.cs
    ├── RAG.Application/
    │   ├── Commands/
    │   │   ├── IndexDocument/        # Index document into Qdrant
    │   │   └── CreateConversation/   # Start a new conversation
    │   ├── Queries/
    │   │   ├── Chat/                 # Core RAG pipeline
    │   │   └── GetConversation/      # Retrieve conversation history
    │   └── Services/
    │       ├── ChunkingService.cs        # Sliding window chunking
    │       └── ContextBuilderService.cs  # Build system prompt from chunks
    ├── RAG.Infrastructure/
    │   ├── Services/
    │   │   ├── GeminiEmbeddingService.cs       # Vector generation
    │   │   └── GeminiLanguageModelService.cs   # Answer generation + streaming
    │   └── Repositories/
    │       ├── QdrantVectorRepository.cs
    │       ├── InMemoryDocumentRepository.cs
    │       └── InMemoryConversationRepository.cs
    └── RAG.API/
        ├── Controllers/
        │   ├── DocumentsController.cs
        │   └── ChatController.cs
        └── Middleware/
            └── ExceptionHandlingMiddleware.cs
```

## Tech Stack

| Component | Technology |
|---|---|
| API | .NET 10 Web API |
| Vector Database | Qdrant |
| Embedding Model | Gemini Embedding 001 (3072 dimensions) |
| Language Model | Gemini 2.5 Flash |
| Mediator | MediatR 11.x |
| Containerization | Docker + Docker Compose |
| Protocol | gRPC (Qdrant), REST (Gemini API), SSE (Streaming) |

## Key Concepts

**RAG (Retrieval Augmented Generation)** — Instead of relying on the LLM's training data, we provide relevant documents as context. The LLM answers based only on what we give it, dramatically reducing hallucination.

**Hallucination Prevention** — The system prompt explicitly instructs Gemini: *"Answer ONLY based on the provided context. If the answer cannot be found, say 'I don't have enough information.'"* This was validated in testing — when asked about the capital of France (not in any document), the system correctly responded with "I don't have enough information."

**Chunking with Overlap** — Documents are split into 500-word chunks with 50-word overlap. Overlap preserves context at chunk boundaries so no information is lost between splits.

**Conversation History** — Previous messages are sent to Gemini on each request (last 10 messages). This allows follow-up questions like "Can you explain more?" to work correctly.

**Citation / Sources** — Every response includes the source documents used to generate the answer, giving full transparency into where the information came from.

**Streaming** — `IAsyncEnumerable<string>` enables token-by-token streaming via SSE (Server-Sent Events), so responses appear word by word rather than waiting for the full response.

## Getting Started

### Prerequisites

- Docker & Docker Compose
- Gemini API Key ([Get one here](https://aistudio.google.com/))

### Setup

1. Clone the repository:
```bash
git clone https://github.com/yourusername/rag-system
cd rag-system
```

2. Create `.env` file in the root directory:
```
GEMINI_API_KEY=your_gemini_api_key_here
```

3. Start the services:
```bash
docker-compose up --build
```

4. Open Swagger UI:
```
http://localhost:5000/swagger
```

## API Endpoints

### Index a Document
```http
POST /api/documents
Content-Type: application/json

{
  "title": "Docker and Containerization",
  "content": "Docker is a platform for developing, shipping, and running applications..."
}
```

Response:
```json
{
  "documentId": "07f4652e-3a49-4617-887c-391cbd2e5775",
  "title": "Docker and Containerization",
  "chunkCount": 1,
  "status": "Completed"
}
```

### Create a Conversation
```http
POST /api/chat/conversations
```

Response:
```json
{
  "id": "c8cba595-9dac-4ff3-8a69-ddd934bae65c",
  "title": "New Conversation",
  "messages": [],
  "createdAt": "2026-03-05T15:08:41Z"
}
```

### Ask a Question
```http
POST /api/chat/conversations/{conversationId}/messages
Content-Type: application/json

{
  "question": "What is the difference between Docker and virtual machines?"
}
```

Response:
```json
{
  "conversationId": "c8cba595-9dac-4ff3-8a69-ddd934bae65c",
  "answer": "Unlike virtual machines, containers share the host operating system kernel, making them lightweight and fast.",
  "sources": [
    {
      "documentId": "07f4652e-3a49-4617-887c-391cbd2e5775",
      "documentTitle": "Docker and Containerization",
      "content": "Docker is a platform for developing...",
      "score": 0
    }
  ]
}
```

### Get Conversation History
```http
GET /api/chat/conversations/{conversationId}
```

## Test Scenarios

| Question | Expected Behavior |
|---|---|
| "What is machine learning?" | Answers from AI document ✅ |
| "What is Docker?" | Answers from Docker document ✅ |
| "What is the capital of France?" | "I don't have enough information" ✅ |
| Follow-up: "Explain more about that" | Remembers conversation context ✅ |

## Services

| Service | Port | Description |
|---|---|---|
| RAG API | 5000 | .NET Web API + Swagger |
| Qdrant Dashboard | 6333 | VectorDB web UI |
| Qdrant gRPC | 6334 | Used internally by the API |

Visit `http://localhost:6333/dashboard` to explore stored vectors visually.