using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;
        public SupplierRepository(AppDbContext context) { _context = context; }

        public async Task<Supplier?> GetByIdAsync(int id) => await _context.Suppliers.FindAsync(id);
        public async Task<IEnumerable<Supplier>> GetAllAsync()
            { 
               //return await _context.Suppliers.ToListAsync();
                return await _context.Suppliers
                    .Include(s => s.Contacts)
                    .ToListAsync();
        }
        public async Task<Supplier?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }
        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return supplier;
        }
        public async Task DeleteAsync(Supplier supplier)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }
    }
}
