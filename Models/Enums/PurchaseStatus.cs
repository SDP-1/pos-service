namespace pos_service.Models.Enums
{
    /// <summary>
    /// Represents the status of a goods receipt / purchase order from a supplier.
    /// </summary>
    public enum PurchaseStatus
    {
        /// <summary>
        /// The purchase goods have been fully received into stock.
        /// </summary>
        Received          = 1,

        /// <summary>
        /// Some items from this purchase have been returned to the supplier.
        /// </summary>
        PartiallyReturned = 2,

        /// <summary>
        /// All items from this purchase have been returned to the supplier.
        /// </summary>
        FullyReturned     = 3,

        /// <summary>
        /// The purchase order was cancelled or voided.
        /// </summary>
        Cancelled         = 4
    }
}
