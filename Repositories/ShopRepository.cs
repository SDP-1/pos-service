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

        /// <summary>
        /// Retrieves the active shop configuration. Assumes a single active shop entry.
        /// </summary>
        /// <returns>The Shop when found; otherwise null.</returns>
        public async Task<Shop?> GetAsync()
        {
            // For now assume there's only one shop row; return the first active
            return await _context.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive);
        }

        /// <summary>
        /// Creates a new shop or updates an existing one. If shop.Id &gt; 0 it will be updated; otherwise created with a new UUID.
        /// </summary>
        /// <param name="shop">Shop entity to create or update.</param>
        /// <returns>The created or updated Shop.</returns>
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
