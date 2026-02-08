using pos_service.Models;

namespace pos_service.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);
        Task<Customer> AddAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task DeleteAsync(Customer customer);
    }
}
