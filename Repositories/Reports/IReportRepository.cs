using pos_service.Models.DTO.Reports;

namespace pos_service.Repositories.Reports
{
    /// <summary>
    /// Repository interface for executing stored procedures to fetch report analytics data.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Executes sp_report_daily_sales to retrieve daily sales analytics.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of daily sales DTO objects.</returns>
        Task<IEnumerable<DailySalesDto>> GetDailySalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_sales_summary to retrieve overall sales KPI statistics.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>The sales summary KPI DTO object if found; otherwise, null.</returns>
        Task<SalesSummaryDto?> GetSalesSummaryReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_sales_details to retrieve detailed line-item sales records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of detailed sales transaction DTO objects.</returns>
        Task<IEnumerable<SalesDetailsDto>> GetSalesDetailsReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_product_sales to retrieve product-wise sales revenue.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of product sales DTO objects.</returns>
        Task<IEnumerable<ProductSalesDto>> GetProductSalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_category_sales to retrieve category-wise sales revenue breakdown.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of category sales DTO objects.</returns>
        Task<IEnumerable<CategorySalesDto>> GetCategorySalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_current_stock to retrieve current stock levels.
        /// </summary>
        /// <returns>A collection of current stock DTO objects.</returns>
        Task<IEnumerable<CurrentStockDto>> GetCurrentStockReportAsync();

        /// <summary>
        /// Executes sp_report_low_stock to retrieve low stock alert items.
        /// </summary>
        /// <returns>A collection of low stock alert DTO objects.</returns>
        Task<IEnumerable<LowStockDto>> GetLowStockReportAsync();

        /// <summary>
        /// Executes sp_report_purchase to retrieve purchase intake history.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of purchase intake DTO objects.</returns>
        Task<IEnumerable<PurchaseDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_expense to retrieve shop operational expenses.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of expense DTO objects.</returns>
        Task<IEnumerable<ExpenseDto>> GetExpenseReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_profit_loss to retrieve net profit and loss statistics.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A profit and loss financial summary DTO object if found; otherwise, null.</returns>
        Task<ProfitLossDto?> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_cash_register to retrieve cash drawer session movements.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of cash register session DTO objects.</returns>
        Task<IEnumerable<CashRegisterDto>> GetCashRegisterReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_customer_sales to retrieve customer revenue statistics.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of customer sales DTO objects.</returns>
        Task<IEnumerable<CustomerSalesDto>> GetCustomerSalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_supplier to retrieve supplier performance and contact details.
        /// </summary>
        /// <returns>A collection of supplier DTO objects.</returns>
        Task<IEnumerable<SupplierDto>> GetSupplierReportAsync();

        /// <summary>
        /// Executes sp_report_sales_return to retrieve returned sales orders.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of sales return DTO objects.</returns>
        Task<IEnumerable<SalesReturnDto>> GetSalesReturnReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Executes sp_report_cashier_performance to retrieve cashier order totals and sales performance.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of cashier performance DTO objects.</returns>
        Task<IEnumerable<CashierPerformanceDto>> GetCashierPerformanceReportAsync(DateTime? startDate, DateTime? endDate);
    }
}
