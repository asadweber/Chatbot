using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// <see cref="IDocumentRepository"/> implementation backed by
/// <see cref="VectorDbContext"/> (PostgreSQL + pgvector).
/// </summary>
public class DocumentRepository(VectorDbContext context) : IDocumentRepository
{
    /// <inheritdoc />
    public async Task AddDocumentWithChunksAsync(Document document, IReadOnlyList<DocumentChunk> chunks, CancellationToken ct = default)
    {
        // Persist the Document first so its DB-generated Id is available
        // when attaching the chunk rows below.
        context.Documents.Add(document);
        await context.SaveChangesAsync(ct);

        foreach (var chunk in chunks)
        {
            chunk.DocumentId = document.Id;
            context.DocumentChunks.Add(chunk);
        }

        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> GetAllWithChunksAsync(CancellationToken ct = default)
    {
        return await context.Documents
            .Include(d => d.Chunks)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> SearchSimilarChunksAsync(Vector queryEmbedding, int topK, CancellationToken ct = default)
    {
        // CosineDistance is translated by Pgvector.EntityFrameworkCore into a
        // pgvector "<=>" SQL operator, letting Postgres do the nearest-
        // neighbor ranking (and use an index) instead of pulling all rows.
        return await context.DocumentChunks
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => c.Content)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpsertOrderDocumentAsync(long orderId, string content, Vector embedding, CancellationToken ct = default)
    {
        var existing = await context.OrderDocuments.FirstOrDefaultAsync(d => d.OrderId == orderId, ct);
        if (existing is null)
        {
            context.OrderDocuments.Add(new OrderDocument
            {
                OrderId = orderId,
                Content = content,
                Embedding = embedding,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Content = content;
            existing.Embedding = embedding;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<long>> SearchSimilarOrdersAsync(Vector queryEmbedding, int topK, CancellationToken ct = default)
    {
        return await context.OrderDocuments
            .OrderBy(d => d.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(d => d.OrderId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteOrderDocumentAsync(long orderId, CancellationToken ct = default)
    {
        var existing = await context.OrderDocuments.FirstOrDefaultAsync(d => d.OrderId == orderId, ct);
        if (existing is null) return;

        context.OrderDocuments.Remove(existing);
        await context.SaveChangesAsync(ct);
    }
}
