using Application.Dtos;

namespace Application.Interfaces;

/// <summary>
/// Natural-language semantic search over orders, backed by the pgvector
/// <c>OrderDocument</c> index.
/// </summary>
public interface IOrderSemanticSearchService
{
    /// <summary>
    /// Returns up to <paramref name="topK"/> orders most semantically
    /// relevant to <paramref name="query"/>, most relevant first.
    /// </summary>
    Task<IReadOnlyList<OrderDto>> SearchAsync(string query, int topK = 10, CancellationToken ct = default);
}
