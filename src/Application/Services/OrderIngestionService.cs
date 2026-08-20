using Application.Dtos;
using Application.Interfaces;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <inheritdoc cref="IOrderIngestionService" />
public class OrderIngestionService(
    IOrderDocumentTextBuilder textBuilder,
    IEmbeddingService embeddingService,
    IDocumentRepository documentRepository,
    IOrderService orderService,
    ILogger<OrderIngestionService> logger) : IOrderIngestionService
{
    /// <inheritdoc />
    public async Task IndexOrderAsync(OrderDto order, CancellationToken ct = default)
    {
        var content = textBuilder.Build(order);
        var embedding = await embeddingService.EmbedAsync(content, ct);
        await documentRepository.UpsertOrderDocumentAsync(order.Id, content, embedding, ct);
        logger.LogInformation("Indexed order {OrderId} for semantic search", order.Id);
    }

    /// <inheritdoc />
    public async Task ReindexAllAsync(CancellationToken ct = default)
    {
        var orders = await orderService.GetAllAsync();
        foreach (var order in orders)
            await IndexOrderAsync(order, ct);
    }

    /// <inheritdoc />
    public Task DeleteOrderDocumentAsync(long orderId, CancellationToken ct = default) =>
        documentRepository.DeleteOrderDocumentAsync(orderId, ct);
}
