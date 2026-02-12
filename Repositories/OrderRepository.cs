using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;
using pos_service.Repositories.Base;

namespace pos_service.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRepository> _logger;
        private readonly IStoredProcedureExecutor _spExecutor;

        public OrderRepository(
            AppDbContext context, 
            ILogger<OrderRepository> logger,
            IStoredProcedureExecutor spExecutor)
        {
            _logger = logger;
            _context = context;
            _spExecutor = spExecutor;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            try { 
                order.Uuid = Guid.NewGuid().ToString();
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating order: {@Order}", order);
                return null;
            }
        }

        public async Task<Order?> GetByIdAsync(int id, bool isActiveOnly = true)
        {
            try
            {
                IQueryable<Order> query = _context.Orders
                                                    .Include(o => o.OrderItems)
                                                    .Include(o => o.Cashier)
                                                    .Include(o => o.Customer)
                                                    .Include(o => o.LoanSettlementLogs);

                // Apply active filter only when requested
                if (isActiveOnly)
                {
                    query = query.Where(o => o.IsActive);
                }

                return await query.FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while Get order");
                return null;
            }
        }
        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cashier)
                .Include(o => o.Customer)
                .Include(o => o.LoanSettlementLogs)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order?> GetByUuidAsync(string uuid)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cashier)
                .Include(o => o.Customer)
                .Include(o => o.LoanSettlementLogs)
                .FirstOrDefaultAsync(o => o.Uuid == uuid && o.IsActive);
        }

        public async Task<List<Order>> GetAllAsync(OrderQueryDto query)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cashier)
                .Include(o => o.Customer)
                .Where(o => o.IsActive)
                .AsQueryable();

            // Apply filters
            if (query.StartDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.EndDate.Value);

            if (query.Status.HasValue)
                ordersQuery = ordersQuery.Where(o => o.MainStatus == query.Status.Value);

            if (query.SubStatus.HasValue)
                ordersQuery = ordersQuery.Where(o => o.SubStatus == query.SubStatus.Value);

            if (query.PaymentMethod.HasValue)
                ordersQuery = ordersQuery.Where(o => o.PaymentMethod == query.PaymentMethod.Value);

            if (query.SaleType.HasValue)
                ordersQuery = ordersQuery.Where(o => o.SaleType == query.SaleType.Value);

            if (query.CustomerId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CustomerId == query.CustomerId.Value);

            if (query.CashierId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CashierId == query.CashierId.Value);

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(query.SearchTerm) ||
                    o.Customer.FirstName.Contains(query.SearchTerm) ||
                    o.Cashier.FirstName.Contains(query.SearchTerm));
            }

            // Apply sorting
            ordersQuery = query.SortBy?.ToLower() switch
            {
                "ordernumber" => query.SortDescending ?
                    ordersQuery.OrderByDescending(o => o.OrderNumber) :
                    ordersQuery.OrderBy(o => o.OrderNumber),
                "netamount" => query.SortDescending ?
                    ordersQuery.OrderByDescending(o => o.NetAmount) :
                    ordersQuery.OrderBy(o => o.NetAmount),
                "createdat" or _ => query.SortDescending ?
                    ordersQuery.OrderByDescending(o => o.CreatedAt) :
                    ordersQuery.OrderBy(o => o.CreatedAt)
            };

            // Apply pagination
            if (query.PageSize > 0)
            {
                ordersQuery = ordersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await ordersQuery.ToListAsync();
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteAsync(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCountAsync(OrderQueryDto query)
        {
            var ordersQuery = _context.Orders.Where(o => o.IsActive);

            // Apply same filters as GetAllAsync
            if (query.StartDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.EndDate.Value);

            if (query.Status.HasValue)
                ordersQuery = ordersQuery.Where(o => o.MainStatus == query.Status.Value);

            if (query.SubStatus.HasValue)
                ordersQuery = ordersQuery.Where(o => o.SubStatus == query.SubStatus.Value);

            if (query.PaymentMethod.HasValue)
                ordersQuery = ordersQuery.Where(o => o.PaymentMethod == query.PaymentMethod.Value);

            if (query.SaleType.HasValue)
                ordersQuery = ordersQuery.Where(o => o.SaleType == query.SaleType.Value);

            if (query.CustomerId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CustomerId == query.CustomerId.Value);

            if (query.CashierId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CashierId == query.CashierId.Value);

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(query.SearchTerm) ||
                    o.Customer.FirstName.Contains(query.SearchTerm) ||
                    o.Cashier.FirstName.Contains(query.SearchTerm));
            }

            return await ordersQuery.CountAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Now;
            var yearMonth = today.ToString("yyyyMM"); // e.g., 202602

            // Get the last order number for this month
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(yearMonth))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastOrder != null)
            {
                // Extract last 7 digits
                var lastNumberPart = lastOrder.OrderNumber.Substring(6, 7);
                nextNumber = int.Parse(lastNumberPart) + 1;
            }

            // Format: YYYYMM + 7-digit sequence = 13 digits total
            return $"{yearMonth}{nextNumber:D7}";
        }

        /// <summary>
        /// Retrieves orders filtered by date range and status.
        /// Key behaviors:
        /// - If StartDate is null, defaults to today's date
        /// - If StartDate > EndDate, automatically swaps the dates
        /// - Returns all active orders in the given date range
        /// - Status filter is optional; if null, returns all statuses
        /// - Results are ordered by CreatedAt in descending order
        /// - Includes related data: OrderItems, Cashier, and Customer
        /// </summary>
        /// <param name="startDate">Start date for the date range filter. Defaults to today if not provided.</param>
        /// <param name="endDate">End date for the date range filter. If null, includes all dates from startDate onwards.</param>
        /// <param name="status">Order status filter. If null, returns all statuses.</param>
        /// <returns>List of active orders matching the criteria, ordered by CreatedAt descending.</returns>
        public async Task<List<Order>> GetOrdersByDateAndStatusAsync(DateTime? startDate, DateTime? endDate, pos_service.Models.Enums.MainOrderStatus? status, pos_service.Models.Enums.OrderSubStatus? subStatus)
        {
            try
            {
                // Replicate stored procedure logic:
                // 1. Default StartDate to Today if NULL
                var effectiveStartDate = startDate ?? DateTime.Today;

                // 2. AUTO-SWAP LOGIC: If Start is newer than End, swap them
                DateTime? effectiveEndDate = endDate;
                if (effectiveEndDate.HasValue && effectiveStartDate > effectiveEndDate.Value)
                {
                    var temp = effectiveStartDate;
                    effectiveStartDate = effectiveEndDate.Value;
                    effectiveEndDate = temp;
                }

                // 3. Build query with the same filtering logic as SP
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Cashier)
                    .Include(o => o.Customer)
                    .Where(o => o.IsActive)
                    .Where(o => o.CreatedAt >= effectiveStartDate);

                // Apply end date filter if provided
                if (effectiveEndDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= effectiveEndDate.Value);

                // Apply status filter if provided
                if (status.HasValue)
                    query = query.Where(o => o.MainStatus == status.Value);

                if (subStatus.HasValue)
                    query = query.Where(o => o.SubStatus == subStatus.Value);

                // Order by creation date descending for reporting
                query = query.OrderByDescending(o => o.CreatedAt);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error getting orders by date and status: StartDate={StartDate}, EndDate={EndDate}, Status={Status}", 
                    startDate, endDate, status);
                throw;
            }
        }

        public async Task<List<ReturnedItemsSummary>> GetReturnedItemsSummaryByOrderNumberAsync(string orderNumber)
        {
            try
            {
                return await _context.Set<ReturnedItemsSummary>()
                    .Where(r => r.OrderNumber == orderNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching returned items summary for order {OrderNumber}", orderNumber);
                return new List<ReturnedItemsSummary>();
            }
        }
    }
}
