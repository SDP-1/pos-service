using pos_service.Models.DTO.Reports;

namespace pos_service.Services.Reports
{
    /// <summary>
    /// Interface definition for business logic operations handling system reports.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Retrieves the daily sales report aggregated by day.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of daily sales summary DTO records.</returns>
        Task<IEnumerable<DailySalesDto>> GetDailySalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves overall sales summary KPI statistics for a given date range.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>The sales summary KPI DTO object if found; otherwise, null.</returns>
        Task<SalesSummaryDto?> GetSalesSummaryReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves detailed line-item sales order transaction records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of detailed sales transaction DTO records.</returns>
        Task<IEnumerable<SalesDetailsDto>> GetSalesDetailsReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves product-wise sales revenue and quantity performance report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of product sales DTO records.</returns>
        Task<IEnumerable<ProductSalesDto>> GetProductSalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves category-wise sales revenue breakdown report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of category sales DTO records.</returns>
        Task<IEnumerable<CategorySalesDto>> GetCategorySalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves the real-time current stock inventory levels report.
        /// </summary>
        /// <returns>A collection of current stock inventory DTO records.</returns>
        Task<IEnumerable<CurrentStockDto>> GetCurrentStockReportAsync();

        /// <summary>
        /// Retrieves items that are currently below or at reorder alert stock thresholds.
        /// </summary>
        /// <returns>A collection of low stock alert DTO records.</returns>
        Task<IEnumerable<LowStockDto>> GetLowStockReportAsync();

        /// <summary>
        /// Retrieves purchase order history and stock intake report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of purchase intake DTO records.</returns>
        Task<IEnumerable<PurchaseDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves shop operational expense transaction records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of expense DTO records.</returns>
        Task<IEnumerable<ExpenseDto>> GetExpenseReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves net profit and loss financial summary report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A profit and loss financial summary DTO object if found; otherwise, null.</returns>
        Task<ProfitLossDto?> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves cash register drawer opening, closing, and movement sessions report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of cash register session DTO records.</returns>
        Task<IEnumerable<CashRegisterDto>> GetCashRegisterReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves customer-wise purchasing and revenue contribution report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of customer sales DTO records.</returns>
        Task<IEnumerable<CustomerSalesDto>> GetCustomerSalesReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves supplier performance, contacts, and order total history report.
        /// </summary>
        /// <returns>A collection of supplier summary DTO records.</returns>
        Task<IEnumerable<SupplierDto>> GetSupplierReportAsync();

        /// <summary>
        /// Retrieves returned sales orders and item return history report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of sales return DTO records.</returns>
        Task<IEnumerable<SalesReturnDto>> GetSalesReturnReportAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Retrieves cashier-wise order processing and sales performance breakdown.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of cashier performance DTO records.</returns>
        Task<IEnumerable<CashierPerformanceDto>> GetCashierPerformanceReportAsync(DateTime? startDate, DateTime? endDate);
    }
}
