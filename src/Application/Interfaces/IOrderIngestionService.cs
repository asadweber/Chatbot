using Application.Dtos;

namespace Application.Interfaces;

/// <summary>
/// Keeps the pgvector-backed semantic index of orders (<c>OrderDocument</c>)
/// in sync with the relational Order data.
/// </summary>
public interface IOrderIngestionService
{
    /// <summary>Renders, embeds, and upserts the semantic document for a single order.</summary>
    Task IndexOrderAsync(OrderDto order, CancellationToken ct = default);

    /// <summary>Re-indexes every order (backfill / full rebuild).</summary>
    Task ReindexAllAsync(CancellationToken ct = default);

    /// <summary>Removes the semantic document for a deleted order.</summary>
    Task DeleteOrderDocumentAsync(long orderId, CancellationToken ct = default);
}
