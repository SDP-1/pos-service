using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Authorization;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Reports;
using pos_service.Models.Enums;
using pos_service.Services;
using pos_service.Services.Reports;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for handling system analytics and performance reporting queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Permission(PermissionType.REPORT_VIEW)]
    public class ReportsController : SystemBaseController
    {
        private readonly IReportService _reportService;

        public ReportsController(
            IReportService reportService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Retrieves the daily sales report aggregated by day.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of daily sales summary DTO records.</returns>
        [HttpGet("daily-sales")]
        public async Task<IActionResult> GetDailySalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetDailySalesReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves overall sales summary KPI statistics for a given date range.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>The sales summary KPI DTO object.</returns>
        [HttpGet("sales-summary")]
        public async Task<IActionResult> GetSalesSummaryReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetSalesSummaryReportAsync(startDate, endDate);
            return Ok(data ?? new Models.DTO.Reports.SalesSummaryDto());
        }

        /// <summary>
        /// Retrieves detailed line-item sales order transaction records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of detailed sales transaction DTO records.</returns>
        [HttpGet("sales-details")]
        public async Task<IActionResult> GetSalesDetailsReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetSalesDetailsReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves product-wise sales revenue and quantity performance report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of product sales DTO records.</returns>
        [HttpGet("product-sales")]
        public async Task<IActionResult> GetProductSalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetProductSalesReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves category-wise sales revenue breakdown report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of category sales DTO records.</returns>
        [HttpGet("category-sales")]
        public async Task<IActionResult> GetCategorySalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetCategorySalesReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves the real-time current stock inventory levels report.
        /// </summary>
        /// <returns>A list of current stock inventory DTO records.</returns>
        [HttpGet("current-stock")]
        public async Task<IActionResult> GetCurrentStockReport()
        {
            var data = await _reportService.GetCurrentStockReportAsync();
            return Ok(data);
        }

        /// <summary>
        /// Retrieves items that are currently below or at reorder alert stock thresholds.
        /// </summary>
        /// <returns>A list of low stock alert DTO records.</returns>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockReport()
        {
            var data = await _reportService.GetLowStockReportAsync();
            return Ok(data);
        }

        /// <summary>
        /// Retrieves purchase order history and stock intake report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of purchase intake DTO records.</returns>
        [HttpGet("purchase")]
        public async Task<IActionResult> GetPurchaseReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetPurchaseReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves shop operational expense transaction records.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of expense DTO records.</returns>
        [HttpGet("expense")]
        public async Task<IActionResult> GetExpenseReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetExpenseReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves net profit and loss financial summary report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A profit and loss financial summary DTO object.</returns>
        [HttpGet("profit-loss")]
        public async Task<IActionResult> GetProfitLossReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetProfitLossReportAsync(startDate, endDate);
            return Ok(data ?? new Models.DTO.Reports.ProfitLossDto());
        }

        /// <summary>
        /// Retrieves cash register drawer opening, closing, and movement sessions report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of cash register session DTO records.</returns>
        [HttpGet("cash-register")]
        public async Task<IActionResult> GetCashRegisterReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetCashRegisterReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves customer-wise purchasing and revenue contribution report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of customer sales DTO records.</returns>
        [HttpGet("customer-sales")]
        public async Task<IActionResult> GetCustomerSalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetCustomerSalesReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves supplier performance, contacts, and order total history report.
        /// </summary>
        /// <returns>A list of supplier summary DTO records.</returns>
        [HttpGet("supplier")]
        public async Task<IActionResult> GetSupplierReport()
        {
            var data = await _reportService.GetSupplierReportAsync();
            return Ok(data);
        }

        /// <summary>
        /// Retrieves returned sales orders and item return history report.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of sales return DTO records.</returns>
        [HttpGet("sales-return")]
        public async Task<IActionResult> GetSalesReturnReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetSalesReturnReportAsync(startDate, endDate);
            return Ok(data);
        }

        /// <summary>
        /// Retrieves cashier-wise order processing and sales performance breakdown.
        /// </summary>
        /// <param name="startDate">Optional start date filter boundary.</param>
        /// <param name="endDate">Optional end date filter boundary.</param>
        /// <returns>A list of cashier performance DTO records.</returns>
        [HttpGet("cashier-performance")]
        public async Task<IActionResult> GetCashierPerformanceReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _reportService.GetCashierPerformanceReportAsync(startDate, endDate);
            return Ok(data);
        }
    }
}
