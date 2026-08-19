using Domain.Entities;

namespace Domain.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(long id);
    Task<IReadOnlyList<Order>> GetAllWithDetailsAsync();

    Task<(IReadOnlyList<Order> Items, int TotalCount, int FilteredCount)> GetPagedAsync(
        int skip, int take, string? searchTerm, string? sortColumn, bool sortAscending);
}
