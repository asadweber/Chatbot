using Domain.Entities;

namespace Domain.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<bool> HasSufficientStockAsync(int productId, int qty);

    Task<bool> ReduceStockQtyAsync(int productId, int qty);

    Task<(IReadOnlyList<Product> Items, int TotalCount, int FilteredCount)> GetPagedAsync(
        int skip, int take, string? searchTerm);
}
