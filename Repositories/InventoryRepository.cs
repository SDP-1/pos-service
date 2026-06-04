using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pos_service.Data;
using pos_service.Data.Utilities;
using pos_service.Models;
using pos_service.Models.DTO.Inventory;
using System.Data.Common;

namespace pos_service.Repositories
{
    public class InventoryRepository : BaseOperations, IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context, ILogger<InventoryRepository>? logger = null) 
            : base(logger as ILogger<BaseOperations>)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

        public async Task<IEnumerable<InventoryAdjustAuditResDto>> GetAuditHistoryAsync(
            string itemUuid,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxRecords = null)
        {
            // Build parameters for stored procedure using inherited CreateParameter method
            var parameters = new DbParameter[]
            {
                CreateParameter(_context, "@p_item_uuid", itemUuid ?? (object)DBNull.Value),
                CreateParameter(_context, "@p_start_date", startDate ?? (object)DBNull.Value),
                CreateParameter(_context, "@p_end_date", endDate ?? (object)DBNull.Value),
                CreateParameter(_context, "@p_max_records", maxRecords ?? 100)
            };

            // Execute stored procedure using inherited method
            var results = await ExecuteStoredProcedureAsync<InventoryAdjustAuditResDto>(
                _context,
                "sp_get_inventory_audit_history",
                parameters);

            return results;
        }
    }
}
