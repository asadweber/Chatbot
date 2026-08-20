using Application.Dtos;
using Application.Interfaces;
using Domain.Repositories;

namespace Application.Services;

/// <inheritdoc cref="IOrderSemanticSearchService" />
public class OrderSemanticSearchService(
    IDocumentRepository documentRepository,
    IEmbeddingService embeddingService,
    IOrderService orderService) : IOrderSemanticSearchService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderDto>> SearchAsync(string query, int topK = 10, CancellationToken ct = default)
    {
        var queryEmbedding = await embeddingService.EmbedAsync(query, ct);
        var orderIds = await documentRepository.SearchSimilarOrdersAsync(queryEmbedding, topK, ct);

        var results = new List<OrderDto>(orderIds.Count);
        foreach (var orderId in orderIds)
        {
            var order = await orderService.GetByIdAsync(orderId);
            if (order is not null) results.Add(order);
        }

        return results;
    }
}
