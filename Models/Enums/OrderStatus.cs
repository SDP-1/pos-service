namespace pos_service.Models.Enums
{
    public enum OrderStatus
    {
        Default = 0,
        Pending = 1, // The order is in progress or not yet paid
        Paid    = 2,    // The order has been fully paid
        Loan    = 3    // The order is sold on credit
    }
}
