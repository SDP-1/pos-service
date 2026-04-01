using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByItemUuidAsync(string itemUuid);
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory> AddAsync(Inventory inventory);
        Task<Inventory> UpdateAsync(Inventory inventory);
    }
}
