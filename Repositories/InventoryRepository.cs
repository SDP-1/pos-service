using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByItemUuidAsync(string itemUuid)
        {
            return await _context.Inventories
                .Include(i => i.Units.OrderBy(u => u.QuantityInBaseUnits))
                // Include related Item with its Price and ExpDates so mapping can populate price/expiries
                .Include(i => i.Item)
                    .ThenInclude(it => it.Price)
                .Include(i => i.Item)
                    .ThenInclude(it => it.ExpDates)
                .FirstOrDefaultAsync(i => i.ItemUuid == itemUuid);
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Inventories
                .Include(i => i.Units.OrderBy(u => u.QuantityInBaseUnits))
                .Include(i => i.Item)
                    .ThenInclude(it => it.ExpDates)
                .OrderBy(i => i.Item.Id)   // <-- sort Inventories by Item.Id
                .ToListAsync();
        }

        public async Task<Inventory> AddAsync(Inventory inventory)
        {
            if (string.IsNullOrWhiteSpace(inventory.Uuid))
            {
                inventory.Uuid = Guid.NewGuid().ToString();
            }

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> UpdateAsync(Inventory inventory)
        {
            // If the entity is already tracked by the context, update only its scalar values
            // to avoid EF replacing/clearing navigation collections unintentionally.
            var tracked = _context.Inventories.Local.FirstOrDefault(i => i.Id == inventory.Id);
            if (tracked != null)
            {
                _context.Entry(tracked).CurrentValues.SetValues(inventory);
                // Do not replace tracked navigation properties (Units) here —
                // callers should explicitly modify Units when they intend to.
            }
            else
            {
                // Detached entity: safe to call Update to attach the full graph
                _context.Inventories.Update(inventory);
            }

            await _context.SaveChangesAsync();
            return inventory;
        }
    }
}
