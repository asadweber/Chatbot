using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository(AppDbContext context)
    : GenericRepository<Customer>(context), ICustomerRepository
{
    public async Task<(IReadOnlyList<Customer> Items, int TotalCount, int FilteredCount)> GetPagedAsync(
        int skip, int take, string? searchTerm, string? sortColumn, bool sortAscending)
    {
        var query = Set.AsNoTracking();

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                (c.Industry != null && c.Industry.Contains(searchTerm)) ||
                (c.City != null && c.City.Contains(searchTerm)) ||
                (c.State != null && c.State.Contains(searchTerm)));
        }

        var filteredCount = await query.CountAsync();

        query = sortColumn switch
        {
            "Name" => sortAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
            "Industry" => sortAscending ? query.OrderBy(c => c.Industry) : query.OrderByDescending(c => c.Industry),
            "City" => sortAscending ? query.OrderBy(c => c.City) : query.OrderByDescending(c => c.City),
            "State" => sortAscending ? query.OrderBy(c => c.State) : query.OrderByDescending(c => c.State),
            "CreatedAt" => sortAscending ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var items = await query.Skip(skip).Take(take).ToListAsync();

        return (items, totalCount, filteredCount);
    }
}
