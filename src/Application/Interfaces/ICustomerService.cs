using Application.Dtos;

namespace Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CustomerDto dto);
    Task<bool> UpdateAsync(int id, CustomerDto dto);
    Task<bool> DeleteAsync(int id);
    Task<DataTableResponseDto<CustomerDto>> GetPagedAsync(DataTableRequestDto request);
}
