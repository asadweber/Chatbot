using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Persistence for ingested documents and their chunks, plus similarity
/// search over chunk embeddings used for retrieval-augmented generation.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Inserts or updates the <see cref="OrderDocument"/> for <paramref name="orderId"/>
    /// with the given rendered <paramref name="content"/> and <paramref name="embedding"/>.
    /// </summary>
    Task UpsertOrderDocumentAsync(long orderId, string content, Pgvector.Vector embedding, CancellationToken ct = default);

    /// <summary>
    /// Returns the ids of the <paramref name="topK"/> orders whose documents are
    /// closest to <paramref name="queryEmbedding"/> by cosine distance, most
    /// relevant first.
    /// </summary>
    Task<IReadOnlyList<long>> SearchSimilarOrdersAsync(Pgvector.Vector queryEmbedding, int topK, CancellationToken ct = default);

    /// <summary>Removes the <see cref="OrderDocument"/> for <paramref name="orderId"/>, if any.</summary>
    Task DeleteOrderDocumentAsync(long orderId, CancellationToken ct = default);
}
