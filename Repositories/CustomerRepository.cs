using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            customer.Uuid = Guid.NewGuid().ToString();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}
