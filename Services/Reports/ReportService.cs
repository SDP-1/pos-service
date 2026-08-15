using pos_service.Models.DTO.Reports;
using pos_service.Repositories.Reports;

namespace pos_service.Services.Reports
{
    /// <summary>
    /// Service implementation for managing and fetching system reports from the repository layer.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Retrieves the daily sales report aggregated by day.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of daily sales summary DTO records.</returns>
        public Task<IEnumerable<DailySalesDto>> GetDailySalesReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetDailySalesReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves overall sales summary KPI statistics for a given date range.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>The sales summary KPI DTO object if found; otherwise, null.</returns>
        public Task<SalesSummaryDto?> GetSalesSummaryReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetSalesSummaryReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves detailed line-item sales order transaction records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of detailed sales transaction DTO records.</returns>
        public Task<IEnumerable<SalesDetailsDto>> GetSalesDetailsReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetSalesDetailsReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves product-wise sales revenue and quantity performance report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of product sales DTO records.</returns>
        public Task<IEnumerable<ProductSalesDto>> GetProductSalesReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetProductSalesReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves category-wise sales revenue breakdown report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of category sales DTO records.</returns>
        public Task<IEnumerable<CategorySalesDto>> GetCategorySalesReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetCategorySalesReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves the real-time current stock inventory levels report.
        /// </summary>
        /// <returns>A collection of current stock inventory DTO records.</returns>
        public Task<IEnumerable<CurrentStockDto>> GetCurrentStockReportAsync()
            => _repo.GetCurrentStockReportAsync();

        /// <summary>
        /// Retrieves items that are currently below or at reorder alert stock thresholds.
        /// </summary>
        /// <returns>A collection of low stock alert DTO records.</returns>
        public Task<IEnumerable<LowStockDto>> GetLowStockReportAsync()
            => _repo.GetLowStockReportAsync();

        /// <summary>
        /// Retrieves purchase order history and stock intake report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of purchase intake DTO records.</returns>
        public Task<IEnumerable<PurchaseDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetPurchaseReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves shop operational expense transaction records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of expense DTO records.</returns>
        public Task<IEnumerable<ExpenseDto>> GetExpenseReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetExpenseReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves net profit and loss financial summary report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A profit and loss financial summary DTO object if found; otherwise, null.</returns>
        public Task<ProfitLossDto?> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetProfitLossReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves cash register drawer opening, closing, and movement sessions report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of cash register session DTO records.</returns>
        public Task<IEnumerable<CashRegisterDto>> GetCashRegisterReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetCashRegisterReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves customer-wise purchasing and revenue contribution report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of customer sales DTO records.</returns>
        public Task<IEnumerable<CustomerSalesDto>> GetCustomerSalesReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetCustomerSalesReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves supplier performance, contacts, and order total history report.
        /// </summary>
        /// <returns>A collection of supplier summary DTO records.</returns>
        public Task<IEnumerable<SupplierDto>> GetSupplierReportAsync()
            => _repo.GetSupplierReportAsync();

        /// <summary>
        /// Retrieves returned sales orders and item return history report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of sales return DTO records.</returns>
        public Task<IEnumerable<SalesReturnDto>> GetSalesReturnReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetSalesReturnReportAsync(startDate, endDate);

        /// <summary>
        /// Retrieves cashier-wise order processing and sales performance breakdown.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A collection of cashier performance DTO records.</returns>
        public Task<IEnumerable<CashierPerformanceDto>> GetCashierPerformanceReportAsync(DateTime? startDate, DateTime? endDate)
            => _repo.GetCashierPerformanceReportAsync(startDate, endDate);
    }
}
