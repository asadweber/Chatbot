using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository(AppDbContext context)
    : GenericRepository<Product>(context), IProductRepository
{
    public async Task<bool> HasSufficientStockAsync(int productId, int qty)
    {
        var product = await Context.Products.FindAsync(productId);
        return product is not null && product.Stock >= qty;
    }

    public async Task<bool> ReduceStockQtyAsync(int productId, int qty)
    {
        var product = await Context.Products.FindAsync(productId);
        if (product is null || product.Stock < qty) return false;

        product.Stock -= qty;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount, int FilteredCount)> GetPagedAsync(
        int skip, int take, string? searchTerm)
    {
        var query = Set.AsNoTracking();

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm));

        var filteredCount = await query.CountAsync();

        var items = await query.OrderBy(p => p.Name).Skip(skip).Take(take).ToListAsync();

        return (items, totalCount, filteredCount);
    }
}
