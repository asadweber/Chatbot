using Application.Dtos;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <inheritdoc cref="IOrderIngestionService" />
/// <remarks>
/// Depends on <see cref="IUnitOfWork"/> directly (not <see cref="IOrderService"/>)
/// to avoid a circular DI dependency: <c>OrderService</c> depends on this service
/// to index orders after create/update/delete.
/// </remarks>
public class OrderIngestionService(
    IOrderDocumentTextBuilder textBuilder,
    IEmbeddingService embeddingService,
    IDocumentRepository documentRepository,
    IUnitOfWork uow,
    IMapper mapper,
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
        var orders = await uow.Orders.GetAllWithDetailsAsync();
        foreach (var order in mapper.Map<List<OrderDto>>(orders))
            await IndexOrderAsync(order, ct);
    }

    /// <inheritdoc />
    public Task DeleteOrderDocumentAsync(long orderId, CancellationToken ct = default) =>
        documentRepository.DeleteOrderDocumentAsync(orderId, ct);
}
