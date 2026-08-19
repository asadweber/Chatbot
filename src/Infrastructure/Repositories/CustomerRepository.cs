using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class CustomerRepository(AppDbContext context)
    : GenericRepository<Customer>(context), ICustomerRepository
{
}
