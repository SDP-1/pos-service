using pos_service.Models.DTO.Settings;
using pos_service.Models;

namespace pos_service.Services
{
    public interface IShopService
    {
        /// <summary>
        /// Retrieves shop configuration (there is expected to be a single shop record).
        /// Uses cache to avoid repeated DB hits.
        /// </summary>
        /// <returns>ShopResDto when found; otherwise null.</returns>
        Task<ShopResDto?> GetAsync();

        /// <summary>
        /// Creates or updates the shop configuration and updates the cached value.
        /// </summary>
        /// <param name="req">Request DTO with shop fields and optional logo file.</param>
        /// <param name="currentUser">Current user performing the operation.</param>
        /// <returns>The persisted ShopResDto.</returns>
        Task<ShopResDto> CreateOrUpdateAsync(ShopReqDto req, CurrentUser currentUser);
    }
}
