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
}
