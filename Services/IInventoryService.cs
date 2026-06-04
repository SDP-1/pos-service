
using pos_service.Models.DTO.Inventory;
using pos_service.Models;

namespace pos_service.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryResDto>> GetAllAsync(CurrentUser currentUser);
        Task<InventoryResDto?> GetByItemUuidAsync(string itemUuid, CurrentUser currentUser);
        Task<InventoryResDto> UpsertAsync(string itemUuid, InventoryReqDto dto, CurrentUser currentUser);
        Task<InventoryResDto?> AdjustStockAsync(string itemUuid, InventoryAdjustReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Get inventory adjustment audit history for an item.
        /// </summary>
        /// <param name="itemUuid">Item UUID to query (required)</param>
        /// <param name="startDate">Start date filter (optional)</param>
        /// <param name="endDate">End date filter (optional)</param>
        /// <param name="maxRecords">Maximum records to return (optional, default 100)</param>
        /// <param name="currentUser">Current user context</param>
        Task<IEnumerable<InventoryAdjustAuditResDto>> GetAuditHistoryAsync(
            string itemUuid,
            DateTime? startDate     = null,
            DateTime? endDate       = null,
            int? maxRecords         = null,
            CurrentUser currentUser = null);
    }
}
