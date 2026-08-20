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
