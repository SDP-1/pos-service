using pos_service.Models.DTO;

namespace pos_service.Services
{
    public interface IItemService
    {
        Task<ItemResDto?> GetItemByIdAsync(int id, int subId);
        Task<IEnumerable<ItemResDto>> GetAllItemsAsync();
        Task<ItemResDto?> CreateItemAsync(ItemReqDto itemDto);
        Task<bool> UpdateItemAsync(int id, int subId, ItemReqDto itemDto);
        Task<bool> DeleteItemAsync(int id, int subId);
        Task<ItemResDto?> AddStockAsync(int id, int subId, decimal quantity);

        Task<IEnumerable<ItemResDto>> GetItemsByMainIdAsync(int id);
        Task<IEnumerable<ItemResDto>> GetItemByBarCodeAsync(string barCode);
        Task<ItemResDto?> GetItemByUuidAsync(Guid uuid);

        Task<Dictionary<string, decimal>> GetQuantitiesByMainIdAsync(int id);
        Task<decimal?> GetQuantityByUuidAsync(Guid uuid);
        Task<decimal?> GetQuantityByIdAsync(int id, int subId);
    }
}
