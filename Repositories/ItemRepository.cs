using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using System.Collections.Immutable;

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
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Inventory)
                    .ThenInclude(u => u.Units)
                .FirstOrDefaultAsync(i => i.Id == id && i.SubId == subId);
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            //return await _context.Items.ToListAsync();
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Inventory)
                    .ThenInclude(u => u.Units)
                .ToListAsync();
        }

        public async Task<Item> AddAsync(Item item)
        {
            if (string.IsNullOrWhiteSpace(item.Uuid))
            {
                item.Uuid = Guid.NewGuid().ToString();
            }
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<int> GetNextMainIdAsync()
        {
            var maxId = await _context.Items.MaxAsync(i => (int?)i.Id) ?? 0;
            return maxId + 1;
        }

        public async Task<int> GetNextSubIdAsync(int mainId)
        {
            var maxSubId = await _context.Items.Where(i => i.Id == mainId).MaxAsync(i => (int?)i.SubId) ?? -1;
            return maxSubId + 1;
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
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Inventory)
                    .ThenInclude(u => u.Units)
                .FirstOrDefaultAsync(i => i.Id == id && i.SubId == subId);
        }

        public async Task<IEnumerable<Item>> GetByMainIdAsync(int id)
        {
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Where(i => i.Id == id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetByBarCodeAsync(string barCode)
        {
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Where(i => i.BarCode == barCode)
                .ToListAsync();
        }

        public async Task<Item?> GetByUuidAsync(string uuid)
        {
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Inventory)
                    .ThenInclude(u => u.Units)
                .FirstOrDefaultAsync(i => i.Uuid == uuid);
        }

        public async Task<IEnumerable<Item>> GetByUuidsAsync(IEnumerable<string> uuids)
        {
            var uuidList = uuids.ToList();
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Where(i => uuidList.Contains(i.Uuid))
                .ToListAsync();
        }

        /// <summary>
        /// Gets all items that are supplied by the specified supplier ID.
        /// </summary>
        public async Task<IEnumerable<Item>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Where(i => i.ItemSuppliers.Any(isu => isu.SuppliersId == supplierId))
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetBySearchAsync(string searchTerm)
        {
            var query = _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(i =>
                    i.Name.Contains(searchTerm) ||
                    i.PrintName.Contains(searchTerm) ||
                    (i.BarCode != null && i.BarCode.Contains(searchTerm)) ||
                    i.Uuid.Contains(searchTerm)
                );
            }

            return await query.ToListAsync();
        }
    }
}
