using pos_service.Models;
using pos_service.Models.DTO.Customers;

namespace pos_service.Repositories
{
    public interface ICustomerRepository
    {
        /// <summary>
        /// Retrieves all customers as response DTOs.
        /// </summary>
        /// <returns>Collection of CustomerResDto.</returns>
        Task<IEnumerable<CustomerResDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a customer entity by database id.
        /// </summary>
        /// <param name="id">Database id of the customer.</param>
        /// <returns>Customer entity when found; otherwise null.</returns>
        Task<Customer?> GetEntityByIdAsync(int id);

        /// <summary>
        /// Retrieves a customer by database id as a response DTO.
        /// </summary>
        /// <param name="id">Database id of the customer.</param>
        /// <returns>CustomerResDto when found; otherwise null.</returns>
        Task<CustomerResDto?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a customer by email address as a response DTO.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>CustomerResDto when found; otherwise null.</returns>
        Task<CustomerResDto?> GetByEmailAsync(string email);

        /// <summary>
        /// Retrieves a customer by phone number as a response DTO.
        /// </summary>
        /// <param name="phoneNumber">Phone number to search for.</param>
        /// <returns>CustomerResDto when found; otherwise null.</returns>
        Task<CustomerResDto?> GetByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Searches customers by a search term across name, phone number and email. Returns up to 10 matches ordered by phone number.
        /// </summary>
        /// <param name="searchTerm">Search term to match against customer fields.</param>
        /// <returns>Collection of matching CustomerResDto.</returns>
        Task<IEnumerable<CustomerResDto>> GetBySearchAsync(string searchTerm);

        /// <summary>
        /// Adds a new customer to the database and assigns a UUID.
        /// </summary>
        /// <param name="customer">Customer entity to add.</param>
        /// <returns>The added Customer entity.</returns>
        Task<Customer> AddAsync(Customer customer);

        /// <summary>
        /// Updates an existing customer with values from the request DTO.
        /// </summary>
        /// <param name="id">Database id of the customer to update.</param>
        /// <param name="dto">Request DTO containing updated customer values.</param>
        /// <returns>The updated Customer when successful; otherwise null if not found.</returns>
        Task<Customer?> UpdateAsync(int id, CustomerReqDto dto);

        /// <summary>
        /// Updates an existing tracked customer entity in the database.
        /// </summary>
        /// <param name="customer">Tracked Customer entity to update.</param>
        /// <returns>The updated Customer entity.</returns>
        Task<Customer> UpdateAsync(Customer customer);

        /// <summary>
        /// Deletes a customer by id.
        /// </summary>
        /// <param name="id">Database id of the customer to delete.</param>
        /// <returns>True when deleted; false if the customer was not found.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
