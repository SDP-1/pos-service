using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShopRepository> _logger;

        public ShopRepository(AppDbContext context, ILogger<ShopRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Shop?> GetAsync()
        {
            // For now assume there's only one shop row; return the first active
            return await _context.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive);
        }

        public async Task<Shop> CreateOrUpdateAsync(Shop shop)
        {
            // If shop has Id > 0, treat as update
            if (shop.Id > 0)
            {
                _context.Entry(shop).State = EntityState.Modified;
            }
            else
            {
                shop.Uuid = Guid.NewGuid().ToString();
                await _context.Shops.AddAsync(shop);
            }

            await _context.SaveChangesAsync();
            return shop;
        }
    }
}
