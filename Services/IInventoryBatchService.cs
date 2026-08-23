using pos_service.Models;
using pos_service.Models.DTO.InventoryBatches;
using pos_service.Models.DTO.StockMovements;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public interface IInventoryBatchService
    {
        Task<IEnumerable<InventoryBatchResDto>> GetBatchesByItemUuidAsync(string itemUuid, bool includeExpired = false, CurrentUser? currentUser = null);
        Task<IEnumerable<InventoryBatchSelectDto>> GetBatchesForPosAsync(string itemUuid, CurrentUser? currentUser = null);
        Task<InventoryBatchResDto> CreateBatchAsync(InventoryBatchReqDto dto, CurrentUser? currentUser = null);
        Task<InventoryBatchResDto> UpdateBatchPricesAsync(string batchUuid, decimal costPrice, decimal markedPrice, decimal retailPrice, decimal wholesalePrice, decimal retailDiscountRatio, decimal wholesaleDiscountRatio, CurrentUser? currentUser = null);
        Task<InventoryBatchResDto> AdjustBatchStockAsync(string batchUuid, decimal quantityDelta, StockMovementType type, string? reason = null, string? comment = null, CurrentUser? currentUser = null);
        Task<InventoryBatchResDto> SetBatchStatusAsync(string batchUuid, bool isActive, BatchStatus? status = null, CurrentUser? currentUser = null);
        Task<IEnumerable<StockMovementResDto>> GetStockMovementsByItemUuidAsync(string itemUuid, CurrentUser? currentUser = null);
        Task<IEnumerable<InventoryBatchLogResDto>> GetBatchLogsByItemUuidAsync(string itemUuid, CurrentUser? currentUser = null);
    }
}
