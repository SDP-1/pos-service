using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IShopRepository
    {
        Task<Shop?> GetAsync();
        Task<Shop> CreateOrUpdateAsync(Shop shop);
    }
}
