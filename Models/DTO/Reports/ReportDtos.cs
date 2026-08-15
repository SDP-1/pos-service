namespace pos_service.Models.DTO.Reports
{
    public class DailySalesDto
    {
        public DateTime SalesDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
    }

    public class SalesSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalItemsSold { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NetProfit { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class SalesDetailsDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string SaleType { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal NetAmount { get; set; }
        public string MainStatus { get; set; } = string.Empty;
    }

    public class ProductSalesDto
    {
        public string ItemName { get; set; } = string.Empty;
        public string? BarCode { get; set; }
        public decimal TotalQuantitySold { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class CategorySalesDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int UniqueItemsCount { get; set; }
        public decimal TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class CurrentStockDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? BarCode { get; set; }
        public decimal StockQuantity { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalSellingValue { get; set; }
        public decimal TotalCostValue { get; set; }
    }

    public class LowStockDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? BarCode { get; set; }
        public decimal StockQuantity { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public decimal LowStockThreshold { get; set; }
        public decimal SellingPrice { get; set; }
    }

    public class PurchaseDto
    {
        public DateTime PurchaseDate { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? BarCode { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal QuantityPurchased { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public string RecordedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }

    public class ExpenseDto
    {
        public DateTime ExpenseDate { get; set; }
        public string ExpenseReason { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal AdjustmentQuantity { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
    }

    public class ProfitLossDto
    {
        public decimal TotalSalesRevenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalDiscountsGiven { get; set; }
        public decimal NetProfit { get; set; }
        public decimal MarginPercentage { get; set; }
    }

    public class CashRegisterDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalanceOutstanding { get; set; }
        public decimal TotalNetRevenue { get; set; }
    }

    public class CustomerSalesDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int TotalSuppliedItems { get; set; }
        public decimal TotalStockQuantity { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }

    public class SalesReturnDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal ReturnedQuantity { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string ReturnReason { get; set; } = string.Empty;
    }

    public class CashierPerformanceDto
    {
        public int CashierId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int TotalOrdersProcessed { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalDiscountsGiven { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
