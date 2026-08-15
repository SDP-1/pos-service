using System.Data;
using pos_service.Data;
using pos_service.Models.DTO.Reports;

namespace pos_service.Repositories.Reports
{
    /// <summary>
    /// Repository implementation for executing stored procedures to fetch report analytics data.
    /// Uses generic reflection-based stored procedure execution.
    /// </summary>
    public class ReportRepository : BaseRepository, IReportRepository
    {
        public ReportRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Executes sp_report_daily_sales to retrieve daily sales analytics.
        /// </summary>
        public async Task<IEnumerable<DailySalesDto>> GetDailySalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<DailySalesDto>("sp_report_daily_sales",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<SalesSummaryDto?> GetSalesSummaryReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureSingleAsync<SalesSummaryDto>("sp_report_sales_summary",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<SalesDetailsDto>> GetSalesDetailsReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<SalesDetailsDto>("sp_report_sales_details",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<ProductSalesDto>> GetProductSalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<ProductSalesDto>("sp_report_product_sales",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<CategorySalesDto>> GetCategorySalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<CategorySalesDto>("sp_report_category_sales",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<CurrentStockDto>> GetCurrentStockReportAsync()
        {
            return await ExecuteStoredProcedureAsync<CurrentStockDto>("sp_report_current_stock");
        }

        public async Task<IEnumerable<LowStockDto>> GetLowStockReportAsync()
        {
            return await ExecuteStoredProcedureAsync<LowStockDto>("sp_report_low_stock");
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<PurchaseDto>("sp_report_purchase",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpenseReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<ExpenseDto>("sp_report_expense",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<ProfitLossDto?> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureSingleAsync<ProfitLossDto>("sp_report_profit_loss",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<CashRegisterDto>> GetCashRegisterReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<CashRegisterDto>("sp_report_cash_register",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<CustomerSalesDto>> GetCustomerSalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<CustomerSalesDto>("sp_report_customer_sales",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<SupplierDto>> GetSupplierReportAsync()
        {
            return await ExecuteStoredProcedureAsync<SupplierDto>("sp_report_supplier");
        }

        public async Task<IEnumerable<SalesReturnDto>> GetSalesReturnReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<SalesReturnDto>("sp_report_sales_return",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }

        public async Task<IEnumerable<CashierPerformanceDto>> GetCashierPerformanceReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteStoredProcedureAsync<CashierPerformanceDto>("sp_report_cashier_performance",
                CreateParameter("p_StartDate", startDate),
                CreateParameter("p_EndDate", endDate));
        }
    }
}
