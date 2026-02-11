namespace pos_service.Models.Enums
{
    public enum SettingKey
    {
        /// <summary>
        /// This allow when item stock is zero, the order can still be created. When disabled, orders cannot be created if any item has zero stock.
        /// </summary>
        AllowZeroStock = 1,

        /// <summary>
        /// If enabled, allow creating orders with negative balance (i.e., credit/loan sales).
        /// When an order has a negative balance and this setting is true, the order will be
        /// created with status `Loan`. When the balance is zero or positive the order will
        /// be marked as `Paid` instead of `Pending`.
        /// </summary>
        AllowOrdesForLoan = 2,

        /// <summary>
        /// When enabled, allow creating Loan orders (negative balance) even when no customer
        /// is provided on the order. When disabled, credit/loan orders must include a CustomerId.
        /// </summary>
        AllowCreditOrderWithoutCustomer = 3,

        /// <summary>
        /// When enabled, loyalty points will be calculated for orders created as Loan (credit) bills.
        /// When disabled, loan/credit orders will not grant earning points; return items will still deduct points.
        /// </summary>
        CalculateLoyaltyPointsForCreditOrders = 4
    }
}
