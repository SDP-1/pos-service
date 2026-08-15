
using pos_service.Models.DTO.Inventory;
using pos_service.Models;

namespace pos_service.Services
{
    public interface IInventoryService
    {
        /// <summary>
        /// Retrieves all inventory records projected as DTOs.
        /// </summary>
        /// <param name="currentUser">Current user context for potential authorization/audit.</param>
        /// <returns>Collection of InventoryResDto.</returns>
        Task<IEnumerable<InventoryResDto>> GetAllAsync(CurrentUser currentUser);

        /// <summary>
        /// Retrieves inventory details for the specified item UUID.
        /// </summary>
        /// <param name="itemUuid">UUID of the item.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>InventoryResDto when found; otherwise null.</returns>
        Task<InventoryResDto?> GetByItemUuidAsync(string itemUuid, CurrentUser currentUser);

        /// <summary>
        /// Updates the inventory record for the given item UUID.
        /// </summary>
        /// <param name="itemUuid">UUID of the associated item.</param>
        /// <param name="dto">Inventory update DTO containing stock and packaging info.</param>
        /// <param name="currentUser">Current user performing the update.</param>
        /// <returns>The updated InventoryResDto.</returns>
        Task<InventoryResDto> UpdateAsync(string itemUuid, InventoryReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Adjusts the stock quantity for the specified item.
        /// </summary>
        /// <param name="itemUuid">UUID of the item to adjust.</param>
        /// <param name="dto">Adjustment DTO (increase/decrease, unit, reason, expiries, price).</param>
        /// <param name="currentUser">Current user performing the adjustment.</param>
        /// <returns>Updated InventoryResDto when successful; otherwise null if inventory not found.</returns>
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
