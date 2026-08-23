namespace pos_service.Models.Enums
{
    /// <summary>
    /// Represents the lifecycle status of an inventory batch.
    /// </summary>
    public enum BatchStatus
    {
        /// <summary>
        /// The batch is active and available for sales/deductions.
        /// </summary>
        Active     = 1,

        /// <summary>
        /// All available stock in the batch has been consumed (zero remaining quantity).
        /// </summary>
        Depleted   = 2,

        /// <summary>
        /// The batch has passed its expiry date and is locked from normal sale.
        /// </summary>
        Expired    = 3,

        /// <summary>
        /// The batch was returned to the supplier or vendor.
        /// </summary>
        Returned   = 4,

        /// <summary>
        /// The batch was discarded or written off due to damage, shrinkage, or audit reconciliation.
        /// </summary>
        WrittenOff = 5
    }
}
