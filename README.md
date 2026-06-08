# Chatbot

A self-hosted Retrieval-Augmented Generation (RAG) chat application built on
ASP.NET Core. Upload your own documents (PDF, TXT, Markdown), then chat with
an LLM that grounds its answers in relevant excerpts retrieved from those
documents — running entirely on local infrastructure via Ollama, with no data
leaving your machine.

## Purpose

Plain LLM chat answers from the model's training data alone, which can be
outdated, generic, or simply wrong about your private documents. This project
implements a practical RAG pipeline: ingest documents, split them into chunks,
embed those chunks as vectors, and at chat time retrieve the most semantically
relevant chunks to feed back into the LLM as context — producing answers
grounded in your own material instead of hallucinated from general knowledge.

## How it works

1. **Ingestion** — user uploads a document (`.pdf`, `.txt`, `.md`). Text is
   extracted (PdfPig for PDFs, plain read for text formats), then recursively
   split into ~500-character overlapping chunks along paragraph/sentence/word
   boundaries to keep each chunk semantically coherent
   (`DocumentIngestionService`).
2. **Embedding** — each chunk is sent to a local Ollama embedding model
   (`nomic-embed-text`), returning a 768-dimension vector. Chunks and vectors
   are stored in PostgreSQL via the `pgvector` extension
   (`OllamaEmbeddingService`, `ApplicationDbContext`).
3. **Retrieval** — the user's question is embedded, and PostgreSQL performs a
   cosine-distance nearest-neighbor search (`<=>` operator via
   Pgvector.EntityFrameworkCore) to find the most relevant chunks
   (`RetrievalService`).
4. **Generation** — retrieved chunks, conversation history, and the question
   are assembled into a grounded system prompt and sent to a local Ollama chat
   model (`llama3.1:8b`) via Semantic Kernel (`OllamaChatService`).

## Tech stack

| Layer | Technology | Role |
|---|---|---|
| Web framework | ASP.NET Core MVC (.NET 10) | Controllers, Razor views, routing, DI |
| Database | PostgreSQL + `pgvector` extension | Stores documents, chunks, embeddings, chat history |
| Data access | EF Core + Npgsql + Pgvector.EntityFrameworkCore | ORM, vector column mapping, cosine-distance queries |
| LLM orchestration | Microsoft Semantic Kernel | Kernel, chat completion & embedding generation abstractions |
| LLM runtime | Ollama (local) | Hosts chat model (`llama3.1:8b`) and embedding model (`nomic-embed-text`) |
| PDF parsing | UglyToad.PdfPig | Extracts text from uploaded PDF documents |

## Project structure

```
Controllers/   HomeController (landing/error), ChatController (sessions & RAG chat),
               DocumentsController (upload & ingestion)
Services/      IChatService/OllamaChatService, IEmbeddingService/OllamaEmbeddingService,
               IRetrievalService/RetrievalService,
               IDocumentIngestionService/DocumentIngestionService
Models/        EF entities (Document, DocumentChunk, ChatSession, ChatMessage) + view models
Data/          ApplicationDbContext — EF Core context, pgvector config, relationships
Migrations/    EF Core database migrations
Views/         Razor views (Home, Chat, Documents)
```

## Endpoints

| Route | Method | Purpose |
|---|---|---|
| `/` | GET | Landing page (`HomeController.Index`) |
| `/Documents` | GET | List uploaded documents and ingestion status |
| `/Documents/Upload` | POST | Upload a file (`.pdf`/`.txt`/`.md`), triggers ingestion pipeline |
| `/Chat?sessionId={id}` | GET | Open chat UI, optionally loading an existing session |
| `/Chat/NewSession` | POST | Create a new empty chat session |
| `/Chat/RenameSession` | POST (JSON) | Rename a session (`RenameSessionRequest`) |
| `/Chat/DeleteSession` | POST (JSON) | Delete a session and its messages (`DeleteSessionRequest`) |
| `/Chat/Send` | POST (JSON) | Submit a user message, run retrieval + generation, return assistant reply (`SendMessageRequest`) |

## Data model

```
Document 1───* DocumentChunk        Document: Id, FileName, ContentType,
                  │                            UploadedAt, chunk count
                  └─ Embedding (Vector, 768d, pgvector column)

ChatSession 1───* ChatMessage       ChatSession: Id, Title, CreatedAt
                                    ChatMessage: Id, Role (user/assistant),
                                                 Content, CreatedAt
```

Embeddings are stored as `Pgvector.Vector` columns (768 dimensions, matching
`nomic-embed-text` output) and indexed for cosine-distance (`<=>`) similarity
search — see `ApplicationDbContext.OnModelCreating` for the column/index setup
and `RetrievalService` for the query that ranks `DocumentChunk` rows by
distance to the question's embedding.

## Configuration

Set in `appsettings.json` (or environment overrides / user secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=<db>;Username=<user>;Password=<password>"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "ChatModel": "llama3.1:8b",
    "EmbeddingModel": "nomic-embed-text"
  }
}
```

## Running locally

1. Start a PostgreSQL instance with the `vector` extension available.
2. Start Ollama and pull the configured models:
   ```
   ollama pull llama3.1:8b
   ollama pull nomic-embed-text
   ```
3. Update the connection string and Ollama settings in `appsettings.json`.
4. Apply EF Core migrations:
   ```
   dotnet ef database update
   ```
5. Run the app:
   ```
   dotnet run
   ```
   Open the **Documents** page to upload material before chatting.

## Troubleshooting

- **Empty/irrelevant answers** — confirm documents finished ingesting (chunk
  count > 0 on the Documents page) and that `nomic-embed-text` is pulled; a
  missing embedding model causes silent ingestion failures.
- **Connection refused to Ollama** — verify `ollama serve` is running and
  `Ollama:Endpoint` matches its address (default `http://localhost:11434`).
- **`relation "vector" does not exist` / migration errors** — the `pgvector`
  extension must be created in the target database before running
  `dotnet ef database update` (`CREATE EXTENSION IF NOT EXISTS vector;`).
- **Slow retrieval on large corpora** — add an IVFFlat/HNSW index on the
  `Embedding` column (see pgvector docs); the default sequential scan is fine
  for small document sets but degrades as chunk counts grow.

## Limitations & possible extensions

- Single-user, no authentication — all sessions and documents are global.
- Fixed chunk size/overlap and a single embedding model; no support for
  re-ranking or hybrid (keyword + vector) search.
- No streaming of assistant responses — replies are returned in one shot.
- No deletion/re-ingestion workflow for documents (upload-only).
