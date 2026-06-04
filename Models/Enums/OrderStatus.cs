namespace pos_service.Models.Enums
{
    // Main status for an order. Represents the high-level lifecycle state.
    public enum MainOrderStatus
    {
        Default   = 0,

        Pending   = 1,   // The order is in progress or not yet paid
        Paid      = 2,   // The order has been fully paid
        Loan      = 3,   // The order is sold on credit
        LoanSettled = 4, // The loan associated with the order has been fully settled
        Cancelled = 5,
    }

    // Sub-status for an order. Optional details about the order state.
    // Designed to be nullable on the Order entity.
    public enum OrderSubStatus
    {
        None   = 0,

        Return = 1, // The order contains return/refund items
    }

    public enum LoanSettlementStatus
    {
        Created          = 0,
        PartiallySettled = 1,
        Completed        = 2
    }
}
