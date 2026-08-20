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
    IOllamaEmbeddingService embeddingService,
    IDocumentRepository documentRepository,
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<OrderIngestionService> logger) : IOrderIngestionService
{
    /// <inheritdoc />
    public async Task IndexOrderAsync(OrderDto order, CancellationToken ct = default)
    {
        var content = textBuilder.Build(order);
        var embedding = await embeddingService.GenerateEmbeddingAsync(content, ct);
        await documentRepository.UpsertOrderDocumentAsync(order.Id, content, embedding, ct);
        logger.LogInformation("Indexed order {OrderId} for semantic search", order.Id);
    }

    private const int ReindexChunkSize = 100;

    /// <inheritdoc />
    public async Task ReindexAllAsync(CancellationToken ct = default)
    {
        var total = await uow.Orders.CountAsync();

        for (var skip = 0; skip < total; skip += ReindexChunkSize)
        {
            ct.ThrowIfCancellationRequested();

            var chunk = await uow.Orders.GetChunkWithDetailsAsync(skip, ReindexChunkSize);
            foreach (var order in mapper.Map<List<OrderDto>>(chunk))
                await IndexOrderAsync(order, ct);

            logger.LogInformation("Reindexed orders {Skip}-{End} of {Total}", skip, Math.Min(skip + ReindexChunkSize, total), total);
        }
    }

    /// <inheritdoc />
    public Task DeleteOrderDocumentAsync(long orderId, CancellationToken ct = default) =>
        documentRepository.DeleteOrderDocumentAsync(orderId, ct);
}
