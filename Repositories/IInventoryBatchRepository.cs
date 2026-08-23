using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IInventoryBatchRepository
    {
        /// <summary>
        /// Retrieves an inventory batch entity by its unique identifier (UUID), including item and supplier details.
        /// </summary>
        /// <param name="batchUuid">The unique identifier (UUID) of the batch.</param>
        /// <returns>InventoryBatch entity when found; otherwise null.</returns>
        Task<InventoryBatch?> GetByUuidAsync(string batchUuid);

        /// <summary>
        /// Retrieves all active, non-depleted inventory batches for a specific item, ordered by creation date ascending (FIFO).
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="includeExpired">Whether to include batches marked as expired.</param>
        /// <returns>Collection of active InventoryBatch entities.</returns>
        Task<IEnumerable<InventoryBatch>> GetActiveBatchesByItemUuidAsync(string itemUuid, bool includeExpired = false);

        /// <summary>
        /// Retrieves recommended inventory batches for stock deduction based on FIFO/FEFO rules to fulfill a requested quantity.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="requestedQuantity">The quantity required to be deducted.</param>
        /// <returns>Ordered collection of candidate InventoryBatch entities.</returns>
        Task<IEnumerable<InventoryBatch>> GetFefoBatchesAsync(string itemUuid, decimal requestedQuantity);

        /// <summary>
        /// Adds a new inventory batch to the database and optionally records an initial stock movement ledger entry.
        /// </summary>
        /// <param name="batch">The batch entity to create.</param>
        /// <param name="initialMovement">Optional initial stock movement ledger entry (e.g. Purchase, OpeningStock).</param>
        /// <returns>The created InventoryBatch entity.</returns>
        Task<InventoryBatch> AddBatchAsync(InventoryBatch batch, StockMovement? initialMovement = null);

        /// <summary>
        /// Updates an existing inventory batch record in the database.
        /// </summary>
        /// <param name="batch">The batch entity with modified quantities, prices, or status.</param>
        /// <returns>The updated InventoryBatch entity.</returns>
        Task<InventoryBatch> UpdateBatchAsync(InventoryBatch batch);

        /// <summary>
        /// Records a stock movement ledger transaction in <c>tbl_stock_movements</c>.
        /// </summary>
        /// <param name="movement">The stock movement entity to record.</param>
        Task AddStockMovementAsync(StockMovement movement);

        /// <summary>
        /// Retrieves recent stock movement ledger records for a specific item, ordered newest first.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="maxRecords">Maximum number of movement rows to retrieve (default 100).</param>
        /// <returns>Collection of StockMovement entities.</returns>
        Task<IEnumerable<StockMovement>> GetStockMovementsByItemUuidAsync(string itemUuid, int maxRecords = 100);

        /// <summary>
        /// Retrieves all stock movement ledger records associated with a specific batch, ordered newest first.
        /// </summary>
        /// <param name="batchUuid">The unique identifier (UUID) of the batch.</param>
        /// <returns>Collection of StockMovement entities.</returns>
        Task<IEnumerable<StockMovement>> GetStockMovementsByBatchUuidAsync(string batchUuid);

        /// <summary>
        /// Generates a sequential batch number identifier for a new batch (e.g. BATCH-YYMMDD-XXX).
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <returns>A formatted unique batch number string.</returns>
        Task<string> GenerateBatchNumberAsync(string itemUuid);

        /// <summary>
        /// Retrieves audit history logs from <c>tbl_inventory_batch_logs</c> for a given item.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="maxRecords">Maximum number of audit logs to retrieve (default 100).</param>
        /// <returns>Collection of InventoryBatchLog entities.</returns>
        Task<IEnumerable<InventoryBatchLog>> GetBatchLogsByItemUuidAsync(string itemUuid, int maxRecords = 100);

        /// <summary>
        /// Recalculates and synchronizes total item stock in <c>tbl_inventories</c> from the sum of remaining batch quantities.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item to synchronize.</param>
        Task RecalculateItemStockQuantityAsync(string itemUuid);
    }
}
