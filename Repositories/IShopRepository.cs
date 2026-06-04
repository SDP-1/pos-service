using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IShopRepository
    {
        /// <summary>
        /// Retrieves the active shop configuration. Assumes a single active shop entry.
        /// </summary>
        /// <returns>The Shop when found; otherwise null.</returns>
        Task<Shop?> GetAsync();

        /// <summary>
        /// Creates a new shop or updates an existing one. If shop.Id &gt; 0 it will be updated; otherwise created with a new UUID.
        /// </summary>
        /// <param name="shop">Shop entity to create or update.</param>
        /// <returns>The created or updated Shop.</returns>
        Task<Shop> CreateOrUpdateAsync(Shop shop);
    }
}
