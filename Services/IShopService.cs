using pos_service.Models.DTO.Settings;
using pos_service.Models;

namespace pos_service.Services
{
    public interface IShopService
    {
        Task<ShopResDto?> GetAsync();
        Task<ShopResDto> CreateOrUpdateAsync(ShopReqDto req, CurrentUser currentUser);
    }
}
