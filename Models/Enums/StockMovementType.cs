namespace pos_service.Models.Enums
{
    /// <summary>
    /// Specifies the business trigger / reason category for an inventory stock movement.
    /// </summary>
    public enum StockMovementType
    {
        /// <summary>
        /// Stock received from a supplier purchase order or goods receipt note.
        /// </summary>
        Purchase        = 1,

        /// <summary>
        /// Stock depleted due to a customer sale transaction.
        /// </summary>
        Sale            = 2,

        /// <summary>
        /// Stock replenished from a customer order return or item exchange.
        /// </summary>
        SaleReturn      = 3,

        /// <summary>
        /// Stock deducted due to returning items back to the supplier.
        /// </summary>
        PurchaseReturn  = 4,

        /// <summary>
        /// Stock written off due to physical breakage, spillage, or product damage.
        /// </summary>
        DamageWriteOff  = 5,

        /// <summary>
        /// Stock written off because items exceeded their safe expiration date.
        /// </summary>
        ExpiryWriteOff  = 6,

        /// <summary>
        /// Manual positive inventory adjustment (e.g. found stock, correction).
        /// </summary>
        ManualAdjustIn  = 7,

        /// <summary>
        /// Manual negative inventory adjustment (e.g. shrinkage, correction).
        /// </summary>
        ManualAdjustOut = 8,

        /// <summary>
        /// Initial stock quantity recorded when setting up a new item or system migration.
        /// </summary>
        OpeningStock    = 9,

        /// <summary>
        /// Stock transfer between storage locations, bins, or shop branches.
        /// </summary>
        Transfer        = 10,

        /// <summary>
        /// Stock adjustment resulting from a formal physical audit or cycle count.
        /// </summary>
        StockCount      = 11
    }
}
