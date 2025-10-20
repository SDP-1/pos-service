namespace pos_service.Models.Enums
{
    public enum OrderStatus
    {
        Pending, // The order is in progress or not yet paid
        Paid,    // The order has been fully paid
        Loan     // The order is sold on credit
    }
}
