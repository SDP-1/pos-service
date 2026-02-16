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
        CalculateLoyaltyPointsForCreditOrders = 4,

        /// <summary>
        /// When this setting is enabled, the cursor always returns to the Barcode field after each scan, 
        /// allowing continuous scanning without pressing Enter. If you need to change the quantity, 
        /// you must manually navigate to the Qty field (for example, using the arrow keys), enter the value, and confirm it.
        /// When this setting is disabled, after scanning a barcode the cursor automatically moves to the Qty field. 
        /// You can immediately enter the quantity and press Enter to add the item to the order, without manually navigating to the quantity field.
        /// </summary>
        AlwaysFocusBarcodeField = 5,

        /// <summary>
        /// When enabled, users are allowed to delete orders from the system. When disabled,
        /// order deletion is prevented by the application logic.
        /// </summary>
        AllowDeleteOrder = 6,
    }
}
