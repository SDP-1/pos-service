namespace pos_service.Models.Enums
{
    /// <summary>
    /// Represents the 15 system core report types.
    /// </summary>
    public enum ReportType
    {
        DailySales          = 1,
        SalesSummary        = 2,
        SalesDetails        = 3,
        ProductSales        = 4,
        CategorySales       = 5,
        CurrentStock        = 6,
        LowStock            = 7,
        Purchase            = 8,
        Expense             = 9,
        ProfitLoss          = 10,
        CashRegister        = 11,
        CustomerSales       = 12,
        Supplier            = 13,
        SalesReturn         = 14,
        CashierPerformance  = 15
    }
}
