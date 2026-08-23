namespace pos_service.Models.Enums
{
    /// <summary>
    /// Identifies the originating source entity / document type linked to an inventory stock movement record.
    /// Uses UPPER_SNAKE_CASE naming convention.
    /// </summary>
    public enum StockMovementReferenceType
    {
        /// <summary>
        /// Sourced from a supplier purchase transaction (<c>tbl_purchases</c>).
        /// </summary>
        PURCHASE         = 1,

        /// <summary>
        /// Sourced from a customer sales order (<c>tbl_orders</c>).
        /// </summary>
        ORDER            = 2,

        /// <summary>
        /// Sourced from a customer order item return (<c>tbl_order_items</c> / <c>tbl_orders</c>).
        /// </summary>
        ORDER_RETURN     = 3,

        /// <summary>
        /// Sourced from a voided or deleted order stock reversal (<c>tbl_orders</c>).
        /// </summary>
        ORDER_DELETE     = 4,

        /// <summary>
        /// Sourced from a manual batch creation or direct lot entry (<c>tbl_inventory_batches</c>).
        /// </summary>
        MANUAL_BATCH     = 5,

        /// <summary>
        /// Sourced from a manual stock adjustment or write-off event.
        /// </summary>
        STOCK_ADJUSTMENT = 6,

        /// <summary>
        /// Sourced from an inventory transfer between locations or branches.
        /// </summary>
        TRANSFER         = 7,

        /// <summary>
        /// Sourced from an initial stock count or system data migration.
        /// </summary>
        OPENING_STOCK    = 8
    }
}
