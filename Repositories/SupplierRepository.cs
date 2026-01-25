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
                    .Include(s => s.ItemSuppliers)
                        .ThenInclude(isu => isu.Item)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetAllBasicAsync()
        {
            // Do not include related navigation properties to keep payload small
            return await _context.Suppliers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.ItemSuppliers)
                    .ThenInclude(isu => isu.Item)
                .Include(s => s.Contacts)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Supplier?> GetSupplierWithItemsAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.ItemSuppliers)
                    .ThenInclude(isu => isu.Item)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Supplier?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name == name);
        }
        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            supplier.Uuid = Guid.NewGuid().ToString();
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
