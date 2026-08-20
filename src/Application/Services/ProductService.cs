using Application.Dtos;
using Application.Interfaces;
using AutoMapper;
using Domain;

namespace Application.Services;

public class ProductService(IUnitOfWork uow, IMapper mapper) : IProductService
{

    public async Task<bool> HasSufficientStockAsync(long productId, long qty)
    {
        var product = await uow.Products.GetByIdAsync(productId);
        return product is not null && product.Stock >= qty;
    }


    public async Task<bool> ReduceStockQtyAsync(long productId, long qty)
    {
        var product = await uow.Products.GetByIdAsync(productId);
        if (product is null || product.Stock < qty) return false;

        product.Stock -= qty;

        await uow.BeginTransactionAsync();
        await uow.Products.Update(product);
        await uow.SaveChangesAsync();                                              // 1) order.Id assigned by DB
        await uow.CommitAsync();
        return true;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await uow.Products.GetAllAsync();
        return mapper.Map<List<ProductDto>>(products);
    }

    public async Task<DataTableResponseDto<ProductDto>> GetPagedAsync(DataTableRequestDto request)
    {
        var (items, totalCount, filteredCount) = await uow.Products.GetPagedAsync(
            request.Start, request.Length, request.SearchValue);

        return new DataTableResponseDto<ProductDto>
        {
            Draw = request.Draw,
            RecordsTotal = totalCount,
            RecordsFiltered = filteredCount,
            Data = mapper.Map<List<ProductDto>>(items)
        };
    }
}
