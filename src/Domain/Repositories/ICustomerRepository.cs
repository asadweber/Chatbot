using Domain.Entities;

namespace Domain.Repositories;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<(IReadOnlyList<Customer> Items, int TotalCount, int FilteredCount)> GetPagedAsync(
        int skip, int take, string? searchTerm, string? sortColumn, bool sortAscending);
}
