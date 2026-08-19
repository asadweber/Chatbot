using Application.Dtos;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Entities;

namespace Application.Services;

public class CustomerService(IUnitOfWork uow, IMapper mapper) : ICustomerService
{
    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await uow.Customers.GetAllAsync();
        return mapper.Map<List<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await uow.Customers.GetByIdAsync(id);
        return mapper.Map<CustomerDto?>(customer);
    }

    public async Task<CustomerDto> CreateAsync(CustomerDto dto)
    {
        var customer = mapper.Map<Customer>(dto);
        await uow.Customers.AddAsync(customer);
        await uow.SaveChangesAsync();
        return mapper.Map<CustomerDto>(customer);
    }

    public async Task<bool> UpdateAsync(int id, CustomerDto dto)
    {
        if (id != dto.Id) return false;

        var existing = await uow.Customers.GetByIdAsync(id);
        if (existing is null) return false;

        mapper.Map(dto, existing);
        await uow.Customers.Update(existing);
        await uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await uow.Customers.GetByIdAsync(id);
        if (customer is null) return false;

        uow.Customers.Remove(customer);
        await uow.SaveChangesAsync();
        return true;
    }

    public async Task<DataTableResponseDto<CustomerDto>> GetPagedAsync(DataTableRequestDto request)
    {
        var sortAscending = !string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var (items, totalCount, filteredCount) = await uow.Customers.GetPagedAsync(
            request.Start, request.Length, request.SearchValue, request.SortColumn, sortAscending);

        return new DataTableResponseDto<CustomerDto>
        {
            Draw = request.Draw,
            RecordsTotal = totalCount,
            RecordsFiltered = filteredCount,
            Data = mapper.Map<List<CustomerDto>>(items)
        };
    }
}
