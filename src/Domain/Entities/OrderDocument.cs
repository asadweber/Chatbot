using Pgvector;

namespace Domain.Entities;

/// <summary>
/// Semantic-search projection of a relational <c>Order</c> (SQL Server,
/// <c>AppDbContext</c>): rendered text plus its vector embedding, stored in
/// the pgvector-backed <c>VectorDbContext</c>. One row per order, upserted
/// whenever the source order is created/updated.
/// </summary>
public class OrderDocument
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Id of the source <c>Order</c> in <c>AppDbContext</c>. No navigation
    /// property — the order lives in a different database.
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>Rendered semantic text for the order (customer, status, products, ...).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Vector embedding of <see cref="Content"/> (pgvector column,
    /// dimension 1024 — see <c>VectorDbContext.OnModelCreating</c>).
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>When this document was last (re)generated.</summary>
    public DateTime UpdatedAt { get; set; }
}
