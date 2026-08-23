namespace pos_service.Models.Enums
{
    /// <summary>
    /// Specifies the business trigger / reason category for an inventory stock movement.
    /// Uses UPPER_SNAKE_CASE naming convention.
    /// </summary>
    public enum StockMovementType
    {
        /// <summary>
        /// Stock received from a supplier purchase order or goods receipt note.
        /// </summary>
        PURCHASE          = 1,

        /// <summary>
        /// Stock depleted due to a customer sale transaction.
        /// </summary>
        SALE              = 2,

        /// <summary>
        /// Stock replenished from a customer order return or item exchange.
        /// </summary>
        SALE_RETURN       = 3,

        /// <summary>
        /// Stock deducted due to returning items back to the supplier.
        /// </summary>
        PURCHASE_RETURN   = 4,

        /// <summary>
        /// Stock written off due to physical breakage, spillage, or product damage.
        /// </summary>
        DAMAGE_WRITE_OFF  = 5,

        /// <summary>
        /// Stock written off because items exceeded their safe expiration date.
        /// </summary>
        EXPIRY_WRITE_OFF  = 6,

        /// <summary>
        /// Manual positive inventory adjustment (e.g. found stock, correction).
        /// </summary>
        MANUAL_ADJUST_IN  = 7,

        /// <summary>
        /// Manual negative inventory adjustment (e.g. shrinkage, correction).
        /// </summary>
        MANUAL_ADJUST_OUT = 8,

        /// <summary>
        /// Initial stock quantity recorded when setting up a new item or system migration.
        /// </summary>
        OPENING_STOCK     = 9,

        /// <summary>
        /// Stock transfer between storage locations, bins, or shop branches.
        /// </summary>
        TRANSFER          = 10,

        /// <summary>
        /// Stock adjustment resulting from a formal physical audit or cycle count.
        /// </summary>
        STOCK_COUNT       = 11
    }
}
