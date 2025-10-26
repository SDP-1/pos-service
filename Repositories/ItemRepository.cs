using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Item?> GetByIdAsync(int id, int subId)
        {
            return await _context.Items.FindAsync(id, subId);
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            //return await _context.Items.ToListAsync();
            return await _context.Items
                .Include(i => i.Suppliers)
                .ToListAsync();
        }

        public async Task<Item> AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Item> UpdateAsync(Item item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task DeleteAsync(Item item)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ItemExistsAsync(int id, int subId)
        {
            return await _context.Items.AnyAsync(e => e.Id == id && e.SubId == subId);
        }

        public async Task<Item?> GetByIdWithSuppliersAsync(int id, int subId)
        {
            // Eagerly loads the related Suppliers data
            return await _context.Items
                .Include(i => i.Suppliers)
                .FirstOrDefaultAsync(i => i.Id == id && i.SubId == subId);
        }

        public async Task<IEnumerable<Item>> GetByMainIdAsync(int id)
        {
            return await _context.Items
                .Where(i => i.Id == id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetByBarCodeAsync(string barCode)
        {
            return await _context.Items
                .Where(i => i.BarCode == barCode)
                .ToListAsync();
        }

        public async Task<Item?> GetByUuidAsync(Guid uuid)
        {
            return await _context.Items
                .FirstOrDefaultAsync(i => i.Uuid == uuid);
        }

        /// <summary>
        /// Gets all items that are supplied by the specified supplier ID.
        /// </summary>
        public async Task<IEnumerable<Item>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.Items
                .Where(i => i.Suppliers.Any(s => s.Id == supplierId))
                .Include(i => i.Suppliers)
                .ToListAsync();
        }
    }
}
