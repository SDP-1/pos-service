using pos_service.Models;
using pos_service.Models.DTO.Inventory;

namespace pos_service.Repositories
{
    public interface IInventoryRepository
    {
        /// <summary>
        /// Retrieves inventory stock, active unit configurations, and audit details for a specific item by its UUID.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <returns>InventoryResDto if found; otherwise null.</returns>
        Task<InventoryResDto?> GetByItemUuidAsync(string itemUuid);

        /// <summary>
        /// Retrieves all inventory stock records across the system.
        /// </summary>
        /// <returns>Collection of InventoryResDto.</returns>
        Task<IEnumerable<InventoryResDto>> GetAllAsync();

        /// <summary>
        /// Adjusts inventory stock quantity, writes an audit record, and updates associated packaging units.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item to adjust.</param>
        /// <param name="dto">Request DTO containing adjustment quantity, reason, and unit definitions.</param>
        /// <param name="currentUser">Optional current authenticated user context.</param>
        /// <returns>The updated InventoryResDto.</returns>
        Task<InventoryResDto> UpdateItemInventoryAsync(string itemUuid, InventoryReqDto dto, CurrentUser? currentUser = null);

        /// <summary>
        /// Retrieves historical manual adjustment audits for an item with optional date range and record limit filters.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="startDate">Optional start date boundary.</param>
        /// <param name="endDate">Optional end date boundary.</param>
        /// <param name="maxRecords">Optional limit on number of returned audit entries.</param>
        /// <returns>Collection of InventoryAdjustAuditResDto records.</returns>
        Task<IEnumerable<InventoryAdjustAuditResDto>> GetAuditHistoryAsync(
            string itemUuid,
            DateTime? startDate = null,
            DateTime? endDate   = null,
            int? maxRecords     = null);
    }
}
