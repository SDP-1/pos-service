using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;
using pos_service.Repositories.Base;

namespace pos_service.Repositories
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        private readonly ILogger<OrderRepository> _logger;
        private readonly IStoredProcedureExecutor _spExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRepository"/> class with the database context, logger, and stored procedure executor.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="spExecutor">The stored procedure executor service.</param>
        public OrderRepository(
            AppDbContext context, 
            ILogger<OrderRepository> logger,
            IStoredProcedureExecutor spExecutor) : base(context)
        {
            _logger = logger;
            _spExecutor = spExecutor;
        }

        /// <summary>
        /// Adds a loan settlement log entry to the data store.
        /// </summary>
        public async Task AddLoanSettlementLogAsync(LoanSettlementLog log)
        {
            _context.LoanSettlementLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Saves order payment settlement updates and creates an associated loan settlement log within a transaction.
        /// </summary>
        /// <param name="order">The order entity with updated settlement status.</param>
        /// <param name="log">The loan settlement audit log to record.</param>
        public async Task SaveRecordSettlementAsync(Order order, LoanSettlementLog log)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.LoanSettlementLogs.Add(log);
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Atomically saves a new order, initial loan log, customer loyalty/debt updates, batch quantity deductions, and stock movement logs in a single transaction.
        /// </summary>
        /// <param name="order">The order entity to persist.</param>
        /// <param name="initialLog">Optional initial loan settlement log.</param>
        /// <param name="customerToUpdate">Optional customer entity with updated debt/loyalty balance.</param>
        /// <param name="batchesToUpdate">Optional list of inventory batches to deduct remaining stock from.</param>
        /// <param name="stockMovementsToAdd">Optional list of stock movement ledger records to insert.</param>
        /// <returns>The created Order entity.</returns>
        public async Task<Order> SaveCreateOrderAsync(
            Order order, 
            LoanSettlementLog? initialLog, 
            Customer? customerToUpdate,
            List<InventoryBatch>? batchesToUpdate = null,
            List<StockMovement>? stockMovementsToAdd = null)
        {
            // Execute order insertion, stock deduction, and ledger updates in an ACID transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Ensure order has unique external identifier
                if (string.IsNullOrWhiteSpace(order.Uuid))
                {
                    order.Uuid = Guid.NewGuid().ToString();
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Link and persist loan settlement log if this order was paid on credit/loan
                if (initialLog != null)
                {
                    initialLog.OrderId = order.Id;
                    _context.LoanSettlementLogs.Add(initialLog);
                }

                // Apply remaining batch quantity decrements
                if (batchesToUpdate != null && batchesToUpdate.Any())
                {
                    foreach (var batch in batchesToUpdate)
                    {
                        var tracked = _context.InventoryBatches.Local.FirstOrDefault(b => b.Id == batch.Id);
                        if (tracked != null)
                        {
                            _context.Entry(tracked).CurrentValues.SetValues(batch);
                        }
                        else
                        {
                            _context.InventoryBatches.Update(batch);
                        }
                    }
                }

                // Insert immutable audit ledger entries for stock movements
                if (stockMovementsToAdd != null && stockMovementsToAdd.Any())
                {
                    foreach (var movement in stockMovementsToAdd)
                    {
                        if (string.IsNullOrWhiteSpace(movement.Uuid))
                        {
                            movement.Uuid = Guid.NewGuid().ToString();
                        }
                        _context.StockMovements.Add(movement);
                    }
                }

                // Update customer running balance / loyalty points if provided
                if (customerToUpdate != null)
                {
                    _context.Customers.Update(customerToUpdate);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Atomically saves order modifications, customer updates, batch quantity adjustments, and stock movement logs in a transaction.
        /// </summary>
        /// <param name="existingOrder">The existing order with modified values.</param>
        /// <param name="customerToUpdate">Optional customer entity with updated balance.</param>
        /// <param name="batchesToUpdate">Optional list of modified inventory batches.</param>
        /// <param name="stockMovementsToAdd">Optional list of stock movements to insert.</param>
        /// <returns>The updated Order entity.</returns>
        public async Task<Order> SaveUpdateOrderAsync(
            Order existingOrder, 
            Customer? customerToUpdate,
            List<InventoryBatch>? batchesToUpdate = null,
            List<StockMovement>? stockMovementsToAdd = null)
        {
            // Execute order updates and stock reversals/deductions within a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Update(existingOrder);

                // Update adjusted batch remaining quantities
                if (batchesToUpdate != null && batchesToUpdate.Any())
                {
                    foreach (var batch in batchesToUpdate)
                    {
                        var tracked = _context.InventoryBatches.Local.FirstOrDefault(b => b.Id == batch.Id);
                        if (tracked != null)
                        {
                            _context.Entry(tracked).CurrentValues.SetValues(batch);
                        }
                        else
                        {
                            _context.InventoryBatches.Update(batch);
                        }
                    }
                }

                // Insert corresponding stock movement ledger entries
                if (stockMovementsToAdd != null && stockMovementsToAdd.Any())
                {
                    foreach (var movement in stockMovementsToAdd)
                    {
                        if (string.IsNullOrWhiteSpace(movement.Uuid))
                        {
                            movement.Uuid = Guid.NewGuid().ToString();
                        }
                        _context.StockMovements.Add(movement);
                    }
                }

                // Update customer balance if affected
                if (customerToUpdate != null)
                {
                    _context.Customers.Update(customerToUpdate);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return existingOrder;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Deletes or voids an order within a transaction, applying inventory batch stock reversals, stock movements, and customer adjustments.
        /// </summary>
        /// <param name="order">The order entity to delete.</param>
        /// <param name="isPermanent">If true, permanently removes the record; otherwise soft-deletes.</param>
        /// <param name="customerToUpdate">Optional customer entity with reversed loyalty points.</param>
        /// <param name="batchesToUpdate">Optional collection of inventory batches with reversed stock quantities.</param>
        /// <param name="stockMovementsToAdd">Optional audit stock movement ledger entries for the reversal.</param>
        public async Task SaveDeleteOrderAsync(
            Order order, 
            bool isPermanent, 
            Customer? customerToUpdate = null, 
            List<InventoryBatch>? batchesToUpdate = null, 
            List<StockMovement>? stockMovementsToAdd = null)
        {
            // Atomically perform order deletion/voiding, inventory batch stock reversals, and customer updates
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Synchronize restored/reversed inventory batches
                if (batchesToUpdate != null && batchesToUpdate.Any())
                {
                    foreach (var batch in batchesToUpdate)
                    {
                        var tracked = _context.InventoryBatches.Local.FirstOrDefault(b => b.Id == batch.Id || b.Uuid == batch.Uuid);
                        if (tracked != null)
                        {
                            _context.Entry(tracked).CurrentValues.SetValues(batch);
                        }
                        else
                        {
                            _context.InventoryBatches.Update(batch);
                        }
                    }
                }

                // Insert corresponding reversal stock movement ledger entries
                if (stockMovementsToAdd != null && stockMovementsToAdd.Any())
                {
                    foreach (var movement in stockMovementsToAdd)
                    {
                        if (string.IsNullOrWhiteSpace(movement.Uuid))
                        {
                            movement.Uuid = Guid.NewGuid().ToString();
                        }
                        _context.StockMovements.Add(movement);
                    }
                }

                // Update customer loyalty points if reversed
                if (customerToUpdate != null)
                {
                    _context.Customers.Update(customerToUpdate);
                }

                if (isPermanent)
                {
                    // Permanently remove loan settlement logs, line items, and the parent order
                    if (order.LoanSettlementLogs != null && order.LoanSettlementLogs.Any())
                    {
                        _context.LoanSettlementLogs.RemoveRange(order.LoanSettlementLogs);
                    }

                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        _context.OrderItems.RemoveRange(order.OrderItems);
                    }

                    _context.Orders.Remove(order);
                }
                else
                {
                    // Soft-delete parent order and cascade inactive status to child items/logs
                    order.IsActive = false;
                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        foreach (var item in order.OrderItems)
                        {
                            item.IsActive = false;
                        }
                    }

                    if (order.LoanSettlementLogs != null && order.LoanSettlementLogs.Any())
                    {
                        foreach (var log in order.LoanSettlementLogs)
                        {
                            log.IsActive = false;
                        }
                    }

                    _context.Orders.Update(order);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Updates the main status and sub-status of an order and commits changes within a transaction.
        /// </summary>
        /// <param name="order">The order entity with updated status.</param>
        /// <returns>The updated Order entity.</returns>
        public async Task<Order> SaveUpdateOrderStatusAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Creates a new order in the data store, assigning a UUID.
        /// </summary>
        /// <param name="order">The order entity to create.</param>
        /// <returns>The created order entity with updated identifiers, or null on unexpected error.</returns>
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

        /// <summary>
        /// Retrieves a specific order by its database id.
        /// </summary>
        /// <param name="id">Order database id.</param>
        /// <param name="isActiveOnly">When true only active orders are considered.</param>
        /// <returns>The Order when found; otherwise null.</returns>
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
        /// <summary>
        /// Retrieves an order by its order number.
        /// </summary>
        /// <param name="orderNumber">Order number to search for.</param>
        /// <returns>The Order when found; otherwise null.</returns>
        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cashier)
                .Include(o => o.Customer)
                .Include(o => o.LoanSettlementLogs)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        /// <summary>
        /// Retrieves an active order by its UUID.
        /// </summary>
        /// <param name="uuid">Order UUID.</param>
        /// <returns>The Order when found and active; otherwise null.</returns>
        public async Task<Order?> GetByUuidAsync(string uuid)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Cashier)
                .Include(o => o.Customer)
                .Include(o => o.LoanSettlementLogs)
                .FirstOrDefaultAsync(o => o.Uuid == uuid && o.IsActive);
        }

        /// <summary>
        /// Retrieves a list of orders based on the specified query parameters with filtering, sorting and pagination applied.
        /// </summary>
        /// <param name="query">Filtering, sorting and pagination parameters.</param>
        /// <returns>List of Order entities matching the query.</returns>
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

        /// <summary>
        /// Updates an existing order in the database.
        /// </summary>
        /// <param name="order">Order entity with updated values.</param>
        /// <returns>The updated Order entity.</returns>
        public async Task<Order> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        /// <summary>
        /// Deletes the specified order from the database.
        /// </summary>
        /// <param name="order">Order entity to delete.</param>
        /// <returns>True when deletion completed.</returns>
        public async Task<bool> DeleteAsync(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets the count of orders matching the specified query parameters.
        /// </summary>
        /// <param name="query">Filtering parameters to count against.</param>
        /// <returns>The number of orders matching the query.</returns>
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

        /// <summary>
        /// Generates a unique sequential order number for the current month in format YYYYMM + 7-digit sequence.
        /// </summary>
        /// <returns>A unique order number string.</returns>
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

        /// <summary>
        /// Returns returned-items summary rows (backed by view) for given order number.
        /// </summary>
        /// <param name="orderNumber">Order number to query returned items for.</param>
        /// <returns>List of ReturnedItemsSummary rows associated with the order.</returns>
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

        /// <summary>
        /// Retrieves an individual order item by its UUID.
        /// </summary>
        /// <param name="uuid">The UUID of the order item.</param>
        /// <returns>OrderItem if found; otherwise null.</returns>
        public async Task<OrderItem?> GetOrderItemByUuidAsync(string uuid)
        {
            try
            {
                return await _context.OrderItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(oi => oi.Uuid == uuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order item with UUID {Uuid}", uuid);
                return null;
            }
        }

        /// <summary>
        /// Retrieves all orders that are inactive (IsActive == false).
        /// </summary>
        /// <returns>List of inactive orders.</returns>
        public async Task<List<Order>> GetInactiveOrdersAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Cashier)
                    .Include(o => o.Customer)
                    .Where(o => !o.IsActive)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inactive orders");
                return new List<Order>();
            }
        }
    }
}
