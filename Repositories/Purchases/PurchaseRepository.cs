using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories.Purchases
{
    public class PurchaseRepository : BaseRepository, IPurchaseRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseRepository"/> class with the database context.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public PurchaseRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all purchase records with supplier details and batch collections.
        /// </summary>
        /// <returns>Collection of Purchase entities.</returns>
        public async Task<IEnumerable<Purchase>> GetAllPurchasesAsync()
        {
            // Eager-load supplier and batch items, filtering only active purchases ordered newest first
            return await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Batches)
                    .ThenInclude(b => b.Item)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PurchaseDate)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a purchase record by its unique identifier (UUID), including supplier and batch details.
        /// </summary>
        /// <param name="purchaseUuid">The unique identifier (UUID) of the purchase.</param>
        /// <returns>Purchase entity when found; otherwise null.</returns>
        public async Task<Purchase?> GetByUuidAsync(string purchaseUuid)
        {
            // Query purchase by UUID with associated supplier and batch hierarchy
            return await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Batches)
                    .ThenInclude(b => b.Item)
                .FirstOrDefaultAsync(p => p.Uuid == purchaseUuid && p.IsActive);
        }

        /// <summary>
        /// Adds a new purchase record to the database and saves changes.
        /// </summary>
        /// <param name="purchase">The purchase entity to insert.</param>
        /// <returns>The created Purchase entity.</returns>
        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
            // Ensure unique external UUID identifier is assigned
            if (string.IsNullOrWhiteSpace(purchase.Uuid))
            {
                purchase.Uuid = Guid.NewGuid().ToString();
            }

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            return purchase;
        }

        /// <summary>
        /// Updates an existing purchase record in the database.
        /// </summary>
        /// <param name="purchase">The purchase entity with modified values.</param>
        /// <returns>The updated Purchase entity.</returns>
        public async Task<Purchase> UpdatePurchaseAsync(Purchase purchase)
        {
            // Check if entity is already tracked locally in DbContext to avoid tracking conflicts
            var tracked = _context.Purchases.Local.FirstOrDefault(p => p.Id == purchase.Id);
            if (tracked != null)
            {
                // Copy current values onto the tracked instance
                _context.Entry(tracked).CurrentValues.SetValues(purchase);
            }
            else
            {
                // Attach and mark entity as modified
                _context.Purchases.Update(purchase);
            }

            await _context.SaveChangesAsync();
            return purchase;
        }

        /// <summary>
        /// Soft-deletes a purchase record by marking its IsActive flag to false.
        /// </summary>
        /// <param name="purchaseUuid">The unique identifier (UUID) of the purchase to delete.</param>
        /// <returns>True if the purchase was found and deleted; otherwise false.</returns>
        public async Task<bool> DeletePurchaseAsync(string purchaseUuid)
        {
            var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.Uuid == purchaseUuid);
            if (purchase == null) return false;

            // Mark inactive for soft-deletion to preserve historical integrity
            purchase.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Generates a sequential, user-friendly purchase order number for the current day (e.g., PUR-YYMMDD-XXX).
        /// </summary>
        /// <returns>A formatted unique purchase number string.</returns>
        public async Task<string> GeneratePurchaseNumberAsync()
        {
            // Count purchases created today to generate sequential zero-padded suffix
            var todayStr = DateTime.UtcNow.ToString("yyMMdd");
            var count = await _context.Purchases.CountAsync(p => p.PurchaseNumber.StartsWith($"PUR-{todayStr}"));
            return $"PUR-{todayStr}-{count + 1:D3}";
        }
    }
}
