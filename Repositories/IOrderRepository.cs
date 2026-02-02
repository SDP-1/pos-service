using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public interface IOrderRepository
    {
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
        Task<List<Order>> GetOrdersByDateAndStatusAsync(DateTime? startDate, DateTime? endDate, OrderStatus? status);
    }
}