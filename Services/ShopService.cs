using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using pos_service.Helpers;
using pos_service.Models;
using pos_service.Models.DTO.Settings;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;
using System.IO;

namespace pos_service.Services
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _repo;
        private readonly ILogger<ShopService> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        private const CacheExpiry DefaultExpiry = CacheExpiry.OneDay;

        public ShopService(IShopRepository repo, ILogger<ShopService> logger, IMapper mapper, ICacheService cache)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ShopResDto?> GetAsync()
        {
            // Use cache for shop details. There is only one shop entry expected, so secondary key is null.
            var dto = _cache.Get<ShopResDto?>(ServiceCacheKey.Shops);
            if (dto != null)
                return dto;

            var shop = await _repo.GetAsync();
            if (shop == null) return null;

            var mapped = _mapper.Map<ShopResDto>(shop);
            _cache.Set(ServiceCacheKey.Shops, null, mapped, DefaultExpiry);
            return mapped;
        }

        public async Task<ShopResDto> CreateOrUpdateAsync(ShopReqDto req, CurrentUser currentUser)
        {
            // convert incoming DTO + optional file into entity, persist, and return DTO
            var existing = await _repo.GetAsync();
            Shop shop;
            if (existing == null)
            {
                shop = new Shop
                {
                    Name = req.Name,
                    Address = req.Address,
                    PhoneNumber = req.PhoneNumber,
                    Email = req.Email
                };
            }
            else
            {
                existing.Name = req.Name;
                existing.Address = req.Address;
                existing.PhoneNumber = req.PhoneNumber;
                existing.Email = req.Email;
                shop = existing;
            }

            // Handle logo according to DTO flags similar to user RemoveImage behavior:
            // - If RemoveLogo == true => remove existing logo (set null)
            // - Else if a new file is provided => replace existing logo with uploaded bytes
            // - Else => keep existing logo unchanged
            if (req.RemoveLogo)
            {
                shop.Logo = null;
            }
            else if (req.Logo != null)
            {
                shop.Logo = await FileHelper.ConvertFileToBytesAsync(req.Logo);
            }

            var saved = await _repo.CreateOrUpdateAsync(shop);

            // update cache with new value and remove any stale entries
            var mapped = _mapper.Map<ShopResDto>(saved);
            _cache.Set(ServiceCacheKey.Shops, null, mapped, DefaultExpiry);

            return mapped;
        }
    }
}
