# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```
dotnet build Chatbot.slnx                      # build whole solution
dotnet run --project src/Chatbot                # run app
dotnet ef database update -c AppDbContext -p src/Infrastructure -s src/Chatbot     # apply SQL Server migrations (business data)
dotnet ef migrations add <Name> -c AppDbContext -p src/Infrastructure -s src/Chatbot
```

No test project currently exists in the solution.

Two separate databases, two separate `DbContext`s — migrations must target the right one via `-c`:
- `AppDbContext` (SQL Server) — Customers/Orders/Products business data.
- `VectorDbContext` (PostgreSQL + pgvector) — documents, chunks, embeddings, order semantic-search index.

Requires local Ollama running with `llama3.1:8b` (chat) and `nomic-embed-text` (embeddings) pulled; see `src/Chatbot/README.md` for full setup/config/troubleshooting.

## Architecture

Layered solution (`Chatbot.slnx`), Clean-Architecture style, .NET 10:

- **Domain** — entities (`Customer`, `Order`, `OrderDetail`, `OrderDocument`, `Product`) and repository interfaces (`IOrderRepository`, etc.), `IUnitOfWork`. No dependencies on other layers.
- **Application** — business services + interfaces, AutoMapper profiles. References `Domain` and `Application.Dtos` only.
- **Application.Dtos** — plain DTOs shared between Application and Chatbot (e.g. `OrderDto`, `CustomerDto`).
- **Infrastructure** — EF Core `DbContext`s, repository implementations, Semantic Kernel/Ollama wiring. `DependencyInjection.cs` (`AddInfrastructure`) registers both DbContexts.
- **Chatbot** — ASP.NET Core MVC entry point: Controllers, Views, `Program.cs`, `wwwroot`.

**Why services split across Infrastructure vs. Application for the RAG path:** `VectorDbContext` and the Semantic Kernel are registered in `Infrastructure.AddInfrastructure`, but the services that consume them (`OllamaEmbeddingService`, `OrderIngestionService`, `OrderSemanticSearchService`, `OrderSupportChatService`) live in `Application.Services` and are registered by `Application.AddApplication`. This avoids Infrastructure needing a project reference back to Application. `Application.AddApplication` must be called after `Infrastructure.AddInfrastructure` in `Program.cs` composition.

### Two RAG pipelines in this app

1. **Document RAG** (original, see `src/Chatbot/README.md`) — upload PDF/TXT/MD → chunk → embed → cosine-similarity retrieval → grounded chat. Entities: `Document`, `DocumentChunk` (in `VectorDbContext`).
2. **Order semantic-search RAG** (`OrderIngestionService` / `OrderSemanticSearchService` / `OrderSupportChatService`) — each `Order` row is rendered to text (`OrderDocumentTextBuilder`), embedded, and stored as an `OrderDocument` row alongside the document-RAG data in the same `VectorDbContext`/pgvector store. Powers the internal Support Desk chat (`SupportDeskController`), which grounds staff Q&A in semantically retrieved orders.
   - `OrderSupportChatService.AskAsync` first tries a canned-answer shortcut (`SupportFaqKnowledge.MatchCannedAnswer`) for fresh, order-id-free questions, then falls back to semantic search + LLM. It also regex-extracts explicit order ids (`#123`, `order id 123`) from the current question *and* recent history so follow-up questions keep prior order context, and reports ids that don't resolve to a real order as "missing" so the LLM doesn't fabricate an answer for them.

### Controllers

`HomeController`, `CustomersController`, `OrdersController`, `ProductsController` are standard CRUD-over-EF MVC controllers backed by `IUnitOfWork`/repositories against `AppDbContext`. `SupportDeskController` is the RAG-grounded internal chat UI backed by `IOrderSupportChatService`.
