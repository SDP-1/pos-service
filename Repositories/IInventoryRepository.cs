using pos_service.Models;
using pos_service.Models.DTO.Inventory;

namespace pos_service.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByItemUuidAsync(string itemUuid);
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory> AddAsync(Inventory inventory);
        Task<Inventory> UpdateAsync(Inventory inventory);
        Task SaveStockAdjustmentAsync(Inventory inventory, Item? item = null, bool itemNeedsUpdate = false);

        /// <summary>
        /// Query inventory adjustment audit history using stored procedure.
        /// </summary>
        /// <param name="itemUuid">Item UUID to query (required)</param>
        /// <param name="startDate">Start date filter (optional)</param>
        /// <param name="endDate">End date filter (optional)</param>
        /// <param name="maxRecords">Maximum records to return (optional, default 100)</param>
        Task<IEnumerable<InventoryAdjustAuditResDto>> GetAuditHistoryAsync(
            string itemUuid,
            DateTime? startDate = null,
            DateTime? endDate   = null,
            int? maxRecords     = null);
    }
}
