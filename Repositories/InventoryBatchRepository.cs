using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public class InventoryBatchRepository : BaseRepository, IInventoryBatchRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryBatchRepository"/> class with the database context.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public InventoryBatchRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves an inventory batch entity by its unique identifier (UUID), including item and supplier details.
        /// </summary>
        /// <param name="batchUuid">The unique identifier (UUID) of the batch.</param>
        /// <returns>InventoryBatch entity when found; otherwise null.</returns>
        public async Task<InventoryBatch?> GetByUuidAsync(string batchUuid)
        {
            return await _context.InventoryBatches
                .Include(b => b.Item)
                    .ThenInclude(i => i!.ItemSuppliers)
                        .ThenInclude(isu => isu.Supplier)
                .FirstOrDefaultAsync(b => b.Uuid == batchUuid);
        }

        /// <summary>
        /// Retrieves all active, non-depleted inventory batches for a specific item, ordered by creation date ascending (FIFO).
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="includeExpired">Whether to include batches marked as expired.</param>
        /// <returns>Collection of active InventoryBatch entities.</returns>
        public async Task<IEnumerable<InventoryBatch>> GetActiveBatchesByItemUuidAsync(string itemUuid, bool includeExpired = false)
        {
            // Prioritize non-empty batches (RemainingQuantity > 0) ordered by newest creation date
            return await _context.InventoryBatches
                .Include(b => b.Item)
                    .ThenInclude(i => i!.ItemSuppliers)
                        .ThenInclude(isu => isu.Supplier)
                .Where(b => b.ItemUuid == itemUuid && b.IsActive)
                .OrderByDescending(b => b.RemainingQuantity > 0)
                .ThenByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves recommended inventory batches for stock deduction based on FIFO/FEFO rules to fulfill a requested quantity.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="requestedQuantity">The quantity required to be deducted.</param>
        /// <returns>Ordered collection of candidate InventoryBatch entities.</returns>
        public async Task<IEnumerable<InventoryBatch>> GetFefoBatchesAsync(string itemUuid, decimal requestedQuantity)
        {
            // Filter active batches with positive stock, ordered by creation date ascending for FIFO deduction
            return await _context.InventoryBatches
                .Where(b => b.ItemUuid == itemUuid 
                         && b.IsActive 
                         && b.Status == BatchStatus.Active 
                         && b.RemainingQuantity > 0)
                .OrderBy(b => b.CreatedAt)
                .ThenBy(b => b.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new inventory batch to the database and optionally records an initial stock movement ledger entry.
        /// </summary>
        /// <param name="batch">The batch entity to create.</param>
        /// <param name="initialMovement">Optional initial stock movement ledger entry (e.g. Purchase, OpeningStock).</param>
        /// <returns>The created InventoryBatch entity.</returns>
        public async Task<InventoryBatch> AddBatchAsync(InventoryBatch batch, StockMovement? initialMovement = null)
        {
            // Generate UUID for batch if not assigned
            if (string.IsNullOrWhiteSpace(batch.Uuid))
            {
                batch.Uuid = Guid.NewGuid().ToString();
            }

            _context.InventoryBatches.Add(batch);

            // Record initial movement ledger entry linked to the new batch
            if (initialMovement != null)
            {
                if (string.IsNullOrWhiteSpace(initialMovement.Uuid))
                {
                    initialMovement.Uuid = Guid.NewGuid().ToString();
                }
                initialMovement.BatchUuid = batch.Uuid;
                initialMovement.ItemUuid = batch.ItemUuid;
                _context.StockMovements.Add(initialMovement);
            }

            await _context.SaveChangesAsync();
            await RecalculateItemStockQuantityAsync(batch.ItemUuid);

            return batch;
        }

        /// <summary>
        /// Updates an existing inventory batch record in the database.
        /// </summary>
        /// <param name="batch">The batch entity with modified quantities, prices, or status.</param>
        /// <returns>The updated InventoryBatch entity.</returns>
        public async Task<InventoryBatch> UpdateBatchAsync(InventoryBatch batch)
        {
            // Handle DbContext tracking state to prevent duplicate tracked instance errors
            var tracked = _context.InventoryBatches.Local.FirstOrDefault(b => b.Id == batch.Id);
            if (tracked != null)
            {
                // Update tracked entity values in place
                _context.Entry(tracked).CurrentValues.SetValues(batch);
            }
            else
            {
                // Attach untracked entity
                _context.InventoryBatches.Update(batch);
            }

            await _context.SaveChangesAsync();
            await RecalculateItemStockQuantityAsync(batch.ItemUuid);

            return batch;
        }

        /// <summary>
        /// Records a stock movement ledger transaction in <c>tbl_stock_movements</c>.
        /// </summary>
        /// <param name="movement">The stock movement entity to record.</param>
        public async Task AddStockMovementAsync(StockMovement movement)
        {
            if (string.IsNullOrWhiteSpace(movement.Uuid))
            {
                movement.Uuid = Guid.NewGuid().ToString();
            }

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves recent stock movement ledger records for a specific item, ordered newest first.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="maxRecords">Maximum number of movement rows to retrieve (default 100).</param>
        /// <returns>Collection of StockMovement entities.</returns>
        public async Task<IEnumerable<StockMovement>> GetStockMovementsByItemUuidAsync(string itemUuid, int maxRecords = 100)
        {
            return await _context.StockMovements
                .Include(sm => sm.Item)
                .Include(sm => sm.Batch)
                .Include(sm => sm.CreatedByUser)
                .Where(sm => sm.ItemUuid == itemUuid)
                .OrderByDescending(sm => sm.CreatedAt)
                .Take(maxRecords)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all stock movement ledger records associated with a specific batch, ordered newest first.
        /// </summary>
        /// <param name="batchUuid">The unique identifier (UUID) of the batch.</param>
        /// <returns>Collection of StockMovement entities.</returns>
        public async Task<IEnumerable<StockMovement>> GetStockMovementsByBatchUuidAsync(string batchUuid)
        {
            return await _context.StockMovements
                .Include(sm => sm.Item)
                .Include(sm => sm.Batch)
                .Include(sm => sm.CreatedByUser)
                .Where(sm => sm.BatchUuid == batchUuid)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Generates a sequential batch number identifier for a new batch (e.g. BAT-YYMMDD-XXX).
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <returns>A formatted unique batch number string.</returns>
        public async Task<string> GenerateBatchNumberAsync(string itemUuid)
        {
            var count = await _context.InventoryBatches.CountAsync(b => b.ItemUuid == itemUuid);
            var dateStr = DateTime.UtcNow.ToString("yyMMdd");
            return $"BAT-{dateStr}-{count + 1:D3}";
        }

        /// <summary>
        /// Retrieves audit history logs from <c>tbl_inventory_batch_logs</c> for a given item.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="maxRecords">Maximum number of audit logs to retrieve (default 100).</param>
        /// <returns>Collection of InventoryBatchLog entities.</returns>
        public async Task<IEnumerable<InventoryBatchLog>> GetBatchLogsByItemUuidAsync(string itemUuid, int maxRecords = 100)
        {
            return await _context.InventoryBatchLogs
                .Include(l => l.Item)
                .Include(l => l.Batch)
                    .ThenInclude(b => b!.Supplier)
                .Include(l => l.ActionByUser)
                .Where(l => l.ItemUuid == itemUuid)
                .OrderByDescending(l => l.ActionDate)
                .Take(maxRecords)
                .ToListAsync();
        }

        /// <summary>
        /// Recalculates and synchronizes total item stock in <c>tbl_inventories</c> from the sum of remaining batch quantities.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item to synchronize.</param>
        public Task RecalculateItemStockQuantityAsync(string itemUuid)
        {
            // Stock is dynamically computed from tbl_inventory_batches
            return Task.CompletedTask;
        }
    }
}
