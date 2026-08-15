namespace pos_service.Models.Enums
{
    public enum SettingKey
    {
        /// <summary>
        /// This allow when item stock is zero, the order can still be created. When disabled, orders cannot be created if any item has zero stock.
        /// </summary>
        AllowZeroStock                                                   = 1,

        /// <summary>
        /// If enabled, allow creating orders with negative balance (i.e., credit/loan sales).
        /// When an order has a negative balance and this setting is true, the order will be
        /// created with status `Loan`. When the balance is zero or positive the order will
        /// be marked as `Paid` instead of `Pending`.
        /// </summary>
        AllowOrdesForLoan                                                = 2,

        /// <summary>
        /// When enabled, allow creating Loan orders (negative balance) even when no customer
        /// is provided on the order. When disabled, credit/loan orders must include a CustomerId.
        /// </summary>
        AllowCreditOrderWithoutCustomer                                  = 3,

        /// <summary>
        /// When enabled, loyalty points will be calculated for orders created as Loan (credit) bills.
        /// When disabled, loan/credit orders will not grant earning points; return items will still deduct points.
        /// </summary>
        CalculateLoyaltyPointsForCreditOrders                            = 4,

        /// <summary>
        /// When this setting is enabled, the cursor always returns to the Barcode field after each scan, 
        /// allowing continuous scanning without pressing Enter. If you need to change the quantity, 
        /// you must manually navigate to the Qty field (for example, using the arrow keys), enter the value, and confirm it.
        /// When this setting is disabled, after scanning a barcode the cursor automatically moves to the Qty field. 
        /// You can immediately enter the quantity and press Enter to add the item to the order, without manually navigating to the quantity field.
        /// </summary>
        AlwaysFocusBarcodeField                                          = 5,

        /// <summary>
        /// When enabled, users are allowed to delete orders from the system. When disabled,
        /// order deletion is prevented by the application logic.
        /// </summary>
        AllowDeleteOrder                                                 = 6,

        /// <summary>
        /// When enabled, a reason is required when decreasing (Increase = false) inventory during adjustments.
        /// When disabled, the reason field is optional.
        /// </summary>
        RequireReasonOnDecreaseStock                                     = 7,

        /// <summary>
        /// When enabled, inventory (stock quantity) cannot be updated when editing/updating items via the item edit section.
        /// When disabled, inventory can be freely updated during item edits.
        /// </summary>
        DisableInventoryUpdateInItemEdit                                 = 8,

        // --- 15 Report Visibility Settings ---
        /// <summary>
        /// When enabled, t report is visible and accessible in the system. When disabled, this report is hidden from users.
        /// Each enum value corresponds to a specific report type, allowing granular control over report visibility.
        /// </summary>
        ShowReportDailySales                                             = 9,
        ShowReportSalesSummary                                           = 10,
        ShowReportSalesDetails                                           = 11,
        ShowReportProductSales                                           = 12,
        ShowReportCategorySales                                          = 13,
        ShowReportCurrentStock                                           = 14,
        ShowReportLowStock                                               = 15,
        ShowReportPurchase                                               = 16,
        ShowReportExpense                                                = 17,
        ShowReportProfitLoss                                             = 18,
        ShowReportCashRegister                                           = 19,
        ShowReportCustomerSales                                          = 20,
        ShowReportSupplier                                               = 21,
        ShowReportSalesReturn                                            = 22,
        ShowReportCashierPerformance                                     = 23,
    }
}
