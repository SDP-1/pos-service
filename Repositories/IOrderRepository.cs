using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public interface IOrderRepository
    {
        /// <summary>
        /// Adds a loan settlement log to the database.
        /// </summary>
        Task AddLoanSettlementLogAsync(LoanSettlementLog log);

        /// <summary>
        /// Saves order payment settlement updates and creates an associated loan settlement log within a transaction.
        /// </summary>
        /// <param name="order">The order entity with updated settlement status.</param>
        /// <param name="log">The loan settlement audit log to record.</param>
        Task SaveRecordSettlementAsync(Order order, LoanSettlementLog log);

        /// <summary>
        /// Atomically saves a new order, initial loan log, customer loyalty/debt updates, batch quantity deductions, and stock movement logs in a single transaction.
        /// </summary>
        /// <param name="order">The order entity to persist.</param>
        /// <param name="initialLog">Optional initial loan settlement log.</param>
        /// <param name="customerToUpdate">Optional customer entity with updated debt/loyalty balance.</param>
        /// <param name="batchesToUpdate">Optional list of inventory batches to deduct remaining stock from.</param>
        /// <param name="stockMovementsToAdd">Optional list of stock movement ledger records to insert.</param>
        /// <returns>The created Order entity.</returns>
        Task<Order> SaveCreateOrderAsync(Order order, LoanSettlementLog? initialLog, Customer? customerToUpdate, List<InventoryBatch>? batchesToUpdate = null, List<StockMovement>? stockMovementsToAdd = null);

        /// <summary>
        /// Atomically saves order modifications, customer updates, batch quantity adjustments, and stock movement logs in a transaction.
        /// </summary>
        /// <param name="existingOrder">The existing order with modified values.</param>
        /// <param name="customerToUpdate">Optional customer entity with updated balance.</param>
        /// <param name="batchesToUpdate">Optional list of modified inventory batches.</param>
        /// <param name="stockMovementsToAdd">Optional list of stock movements to insert.</param>
        /// <returns>The updated Order entity.</returns>
        Task<Order> SaveUpdateOrderAsync(Order existingOrder, Customer? customerToUpdate, List<InventoryBatch>? batchesToUpdate = null, List<StockMovement>? stockMovementsToAdd = null);

        /// <summary>
        /// Deletes or voids an order within a transaction, applying inventory batch stock reversals, stock movements, and customer adjustments.
        /// </summary>
        /// <param name="order">The order entity to delete.</param>
        /// <param name="isPermanent">If true, permanently removes the record; otherwise soft-deletes.</param>
        /// <param name="customerToUpdate">Optional customer entity with reversed loyalty points.</param>
        /// <param name="batchesToUpdate">Optional collection of inventory batches with reversed stock quantities.</param>
        /// <param name="stockMovementsToAdd">Optional audit stock movement ledger entries for the reversal.</param>
        Task SaveDeleteOrderAsync(
            Order order, 
            bool isPermanent, 
            Customer? customerToUpdate = null, 
            List<InventoryBatch>? batchesToUpdate = null, 
            List<StockMovement>? stockMovementsToAdd = null);

        /// <summary>
        /// Updates the main status and sub-status of an order and commits changes.
        /// </summary>
        /// <param name="order">The order entity with updated status.</param>
        /// <returns>The updated Order entity.</returns>
        Task<Order> SaveUpdateOrderStatusAsync(Order order);

        /// <summary>
        /// Creates a new order in the data store.
        /// </summary>
        /// <param name="order">The order entity to create.</param>
        /// <returns>The created order entity with updated identifiers.</returns>
        Task<Order> CreateAsync(Order order);

        /// <summary>
        /// Retrieves a specific order by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order.</param>
        /// <param name="isActiveOnly">only get active recodes - default true.</param>
        /// <returns>The order entity if found, otherwise null.</returns>
        Task<Order?> GetByIdAsync(int id, bool isActiveOnly = true);

        /// <summary>
        /// Retrieves an order by its order number.
        /// </summary>
        /// <param name="orderNumber">The order number to search for.</param>
        /// <returns>The order entity if found, otherwise null.</returns>
        Task<Order?> GetByOrderNumberAsync(string orderNumber);

        /// <summary>
        /// Retrieves an order by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the order to retrieve.</param>
        /// <returns>The order entity if found, otherwise null.</returns>
        Task<Order?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Retrieves a list of orders based on the specified query parameters.
        /// </summary>
        /// <param name="query">The query parameters for filtering and paginating orders.</param>
        /// <returns>A list of order entities matching the query criteria.</returns>
        Task<List<Order>> GetAllAsync(OrderQueryDto query);

        /// <summary>
        /// Updates an existing order in the data store.
        /// </summary>
        /// <param name="order">The order entity with updated information.</param>
        /// <returns>The updated order entity.</returns>
        Task<Order> UpdateAsync(Order order);

        /// <summary>
        /// Deletes an order from the data store.
        /// </summary>
        /// <param name="order">The order entity with updated information.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteAsync(Order order);

        /// <summary>
        /// Gets the count of orders matching the specified query parameters.
        /// </summary>
        /// <param name="query">The query parameters for filtering orders.</param>
        /// <returns>The number of orders matching the query criteria.</returns>
        Task<int> GetCountAsync(OrderQueryDto query);

        /// <summary>
        /// Generates a unique order number for new orders.
        /// </summary>
        /// <returns>A unique order number string.</returns>
        Task<string> GenerateOrderNumberAsync();


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
        Task<List<Order>> GetOrdersByDateAndStatusAsync(DateTime? startDate, DateTime? endDate, MainOrderStatus? status, OrderSubStatus? subStatus);

        /// <summary>
        /// Returns returned-items summary rows (backed by a database view) for the specified order number.
        /// </summary>
        /// <param name="orderNumber">Order number to query returned items for.</param>
        /// <returns>List of ReturnedItemsSummary rows associated with the order.</returns>
        Task<List<pos_service.Models.ReturnedItemsSummary>> GetReturnedItemsSummaryByOrderNumberAsync(string orderNumber);

        /// <summary>
        /// Retrieves an individual order item by its UUID.
        /// </summary>
        /// <param name="uuid">The UUID of the order item.</param>
        /// <returns>OrderItem if found; otherwise null.</returns>
        Task<OrderItem?> GetOrderItemByUuidAsync(string uuid);

        /// <summary>
        /// Retrieves all orders that are inactive (IsActive == false).
        /// </summary>
        /// <returns>List of inactive orders.</returns>
        Task<List<Order>> GetInactiveOrdersAsync();
    }
}