using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Customers;

namespace pos_service.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<CustomerResDto>> GetAllAsync()
        {
            var query = _context.Customers.AsQueryable();
            return await makeCustomreResponceDto(_context, query);
        }

        public async Task<CustomerResDto?> GetByIdAsync(int id)
        {
            var query = _context.Customers.Where(c => c.Id == id);
            var result = await makeCustomreResponceDto(_context, query);
            return result.FirstOrDefault();
        }

        public async Task<CustomerResDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var query = _context.Customers.Where(c => c.Email == email);
            var result = await makeCustomreResponceDto(_context, query);
            return result.FirstOrDefault();
        }

        public async Task<CustomerResDto?> GetByPhoneNumberAsync(string phoneNumber)
        {
            var query = _context.Customers.Where(c => c.PhoneNumber == phoneNumber);
            var result = await makeCustomreResponceDto(_context, query);
            return result.FirstOrDefault();
        }

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

        public async Task<Customer> AddAsync(Customer customer)
        {
            customer.Uuid = Guid.NewGuid().ToString();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

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
