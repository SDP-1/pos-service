namespace pos_service.Models.Enums
{
    /// <summary>
    /// Main status for an order. Represents the high-level lifecycle state.
    /// </summary>
    public enum MainOrderStatus
    {
        /// <summary>
        /// Default unspecified status.
        /// </summary>
        Default     = 0,

        /// <summary>
        /// The order is in progress or not yet paid.
        /// </summary>
        Pending     = 1,

        /// <summary>
        /// The order has been fully paid.
        /// </summary>
        Paid        = 2,

        /// <summary>
        /// The order is sold on credit (loan).
        /// </summary>
        Loan        = 3,

        /// <summary>
        /// The loan associated with the order has been fully settled.
        /// </summary>
        LoanSettled = 4,

        /// <summary>
        /// The order was cancelled.
        /// </summary>
        Cancelled   = 5,
    }

    /// <summary>
    /// Sub-status for an order. Optional details about the order state. Designed to be nullable on the Order entity.
    /// </summary>
    public enum OrderSubStatus
    {
        /// <summary>
        /// No sub-status specified.
        /// </summary>
        None   = 0,

        /// <summary>
        /// The order contains return/refund items.
        /// </summary>
        Return = 1,
    }

    /// <summary>
    /// Status of a loan settlement process for credit/loan orders.
    /// </summary>
    public enum LoanSettlementStatus
    {
        /// <summary>
        /// Settlement record created but not yet processed.
        /// </summary>
        Created          = 0,
        /// <summary>
        /// Part of the loan has been settled.
        /// </summary>
        PartiallySettled = 1,
        /// <summary>
        /// The loan has been fully settled.
        /// </summary>
        Completed        = 2
    }
}
