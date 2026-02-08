using pos_service.Models;
using pos_service.Models.DTO.Customers;

namespace pos_service.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResDto>> GetAllCustomersAsync(CurrentUser currentUser);
        Task<CustomerResDto?> GetCustomerByIdAsync(int id, CurrentUser currentUser);
        Task<CustomerResDto> CreateCustomerAsync(CustomerReqDto dto, CurrentUser currentUser);
        Task<bool> UpdateCustomerAsync(int id, CustomerReqDto dto, CurrentUser currentUser);
        Task<bool> DeleteCustomerAsync(int id, CurrentUser currentUser);
    }
}
