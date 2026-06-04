using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Customers;

namespace pos_service.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        /// <summary>
        /// Initializes a new instance of the CustomerRepository.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        public CustomerRepository(AppDbContext context) { _context = context; }

        /// <summary>
        /// Retrieves all customers as response DTOs.
        /// </summary>
        /// <returns>Collection of CustomerResDto.</returns>
        public async Task<IEnumerable<CustomerResDto>> GetAllAsync()
        {
            var query = _context.Customers.AsQueryable();
            return await makeCustomreResponceDto(_context, query);
        }

        /// <summary>
        /// Retrieves a customer by database id as a response DTO.
        /// </summary>
        /// <param name="id">Database id of the customer.</param>
        /// <returns>CustomerResDto when found; otherwise null.</returns>
        public async Task<CustomerResDto?> GetByIdAsync(int id)
        {
            var query = _context.Customers.Where(c => c.Id == id);
            var result = await makeCustomreResponceDto(_context, query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a customer by email address as a response DTO.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>CustomerResDto when found; otherwise null.</returns>
        public async Task<CustomerResDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var query = _context.Customers.Where(c => c.Email == email);
            var result = await makeCustomreResponceDto(_context, query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a customer by phone number as a response DTO.
        /// </summary>
        /// <param name="phoneNumber">Phone number to search for.</param>
        /// <returns>CustomerResDto when found; otherwise null.</returns>
        public async Task<CustomerResDto?> GetByPhoneNumberAsync(string phoneNumber)
        {
            var query = _context.Customers.Where(c => c.PhoneNumber == phoneNumber);
            var result = await makeCustomreResponceDto(_context, query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Searches customers by a search term across name, phone number and email. Returns up to 10 matches ordered by phone number.
        /// </summary>
        /// <param name="searchTerm">Search term to match against customer fields.</param>
        /// <returns>Collection of matching CustomerResDto.</returns>
        public async Task<IEnumerable<CustomerResDto>> GetBySearchAsync(string searchTerm)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(c =>
                    c.FirstName.Contains(searchTerm) ||
                    (c.LastName != null && c.LastName.Contains(searchTerm)) ||
                    c.PhoneNumber.Contains(searchTerm) ||
                    (c.Email != null && c.Email.Contains(searchTerm))
                );
            }

            query = query
                .OrderBy(c => c.PhoneNumber)
                .Take(10);

            return await makeCustomreResponceDto(_context, query);
        }

        /// <summary>
        /// Adds a new customer to the database and assigns a UUID.
        /// </summary>
        /// <param name="customer">Customer entity to add.</param>
        /// <returns>The added Customer entity.</returns>
        public async Task<Customer> AddAsync(Customer customer)
        {
            customer.Uuid = Guid.NewGuid().ToString();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        /// <summary>
        /// Updates an existing customer with values from the request DTO.
        /// </summary>
        /// <param name="id">Database id of the customer to update.</param>
        /// <param name="dto">Request DTO containing updated customer values.</param>
        /// <returns>The updated Customer when successful; otherwise null if not found.</returns>
        public async Task<Customer?> UpdateAsync(int id, CustomerReqDto dto)
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) return null;

            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.PhoneNumber = dto.PhoneNumber;
            existing.Email = dto.Email;
            existing.Address = dto.Address;
            existing.LoyaltyPoints = dto.LoyaltyPoints;
            existing.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Deletes a customer by id.
        /// </summary>
        /// <param name="id">Database id of the customer to delete.</param>
        /// <returns>True when deleted; false if the customer was not found.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) return false;

            _context.Customers.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<List<CustomerResDto>> makeCustomreResponceDto(AppDbContext db, IQueryable<Customer> query)
        {
            var q = from c in query
                    select new CustomerResDto
                    {
                        Id            = c.Id,
                        FirstName     = c.FirstName,
                        LastName      = c.LastName,
                        FullName      = c.FullName,
                        PhoneNumber   = c.PhoneNumber,
                        Email         = c.Email,
                        Address       = c.Address,
                        LoyaltyPoints = c.LoyaltyPoints,

                        Uuid          = c.Uuid,
                        CreatedAt     = c.CreatedAt,
                        UpdatedAt     = c.UpdatedAt,
                        CreatedBy     = db.Users.Where(user => user.Uuid == c.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? c.CreatedBy,
                        UpdatedBy     = db.Users.Where(user => user.Uuid == c.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? c.UpdatedBy,
                        IsActive      = c.IsActive
                    };

            return await q.ToListAsync();
        }
    }
}
