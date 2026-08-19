using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository(AppDbContext context)
    : GenericRepository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByIdWithDetailsAsync(long id)
    {
        return await Set
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IReadOnlyList<Order>> GetAllWithDetailsAsync()
    {
        return await Set
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount, int FilteredCount)> GetPagedAsync(
        int skip, int take, string? searchTerm, string? sortColumn, bool sortAscending)
    {
        var query = Set.Include(o => o.Customer).AsNoTracking();

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o =>
                o.Status.Contains(searchTerm) ||
                o.Customer.Name.Contains(searchTerm));
        }

        var filteredCount = await query.CountAsync();

        query = sortColumn switch
        {
            "CustomerName" => sortAscending ? query.OrderBy(o => o.Customer.Name) : query.OrderByDescending(o => o.Customer.Name),
            "OrderDate" => sortAscending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate),
            "TotalAmount" => sortAscending ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount),
            "Status" => sortAscending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            _ => query.OrderByDescending(o => o.OrderDate)
        };

        var items = await query.Skip(skip).Take(take).ToListAsync();

        return (items, totalCount, filteredCount);
    }
}
