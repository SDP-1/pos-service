using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Orders;

namespace pos_service.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
        {
            _logger = logger;
            _context = context;
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
                                                    .Include(o => o.Customer);

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
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.IsActive);
        }

        public async Task<Order?> GetByUuidAsync(string uuid)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cashier)
                .Include(o => o.Customer)
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
                ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);

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
                ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);

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
            var today = DateTime.UtcNow;
            var year = today.Year;
            var month = today.Month.ToString("D2");

            // Get the last order number for today
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith($"ORD-{year}-{month}"))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
            {
                return $"ORD-{year}-{month}-00001";
            }

            var lastNumber = int.Parse(lastOrder.OrderNumber.Split('-').Last());
            return $"ORD-{year}-{month}-{(lastNumber + 1):D5}";
        }
    }
}
