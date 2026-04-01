
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
    }
}
