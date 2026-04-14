using pos_service.Models;
using pos_service.Models.DTO.Customers;

namespace pos_service.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerResDto>> GetAllAsync();
        Task<CustomerResDto?> GetByIdAsync(int id);
        Task<CustomerResDto?> GetByEmailAsync(string email);
        Task<CustomerResDto?> GetByPhoneNumberAsync(string phoneNumber);
        Task<IEnumerable<CustomerResDto>> GetBySearchAsync(string searchTerm);
        Task<Customer> AddAsync(Customer customer);
        Task<Customer?> UpdateAsync(int id, CustomerReqDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
