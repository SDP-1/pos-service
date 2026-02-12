using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    /// <summary>
    /// Service interface for managing order operations in the POS system.
    /// Defines contract for order creation, retrieval, updating, and status management.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order in the system.
        /// </summary>
        /// <param name="orderDto">The order data transfer object containing order details.</param>
        /// <param name="currentUser">The current user creating the order.</param>
        /// <returns>The created order details.</returns>
        Task<OrderResDto> CreateOrderAsync(OrderReqDto orderDto, CurrentUser currentUser);

        /// <summary>
        /// Retrieves a specific order by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order.</param>
        /// <param name="currentUser">The current user requesting the order.</param>
        /// <returns>The order details if found, otherwise null.</returns>
        Task<OrderResDto?> GetOrderAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Retrieves an order by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the order to retrieve.</param>
        /// <param name="currentUser">The current user requesting the order.</param>
        /// <returns>The order details if found, otherwise null.</returns>
        Task<OrderResDto?> GetOrderByUuidAsync(string uuid, CurrentUser currentUser);

        /// <summary>
        /// Retrieves an order by its order number.
        /// </summary>
        /// <param name="uuid">The order number to search for.</param>
        /// <param name="currentUser">The current user requesting the order.</param>
        /// <returns>The order details if found, otherwise null.</returns>
        Task<OrderResDto?> GetOrderByOrderNumberAsync(string uuid, CurrentUser currentUser);

        // Returns order header with order items enriched with returned quantities
        Task<OrderResDto?> GetOrderWithReturnedItemsAsync(string orderNumber, CurrentUser currentUser);

        /// <summary>
        /// Retrieves a list of orders based on the specified query parameters.
        /// </summary>
        /// <param name="query">The query parameters for filtering and paginating orders.</param>
        /// <param name="currentUser">The current user requesting the orders.</param>
        /// <returns>A paginated list of orders matching the query criteria.</returns>
        Task<OrderListResDto> GetOrdersAsync(OrderQueryDto query, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing order with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="orderDto">The order data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user updating the order.</param>
        /// <returns>The updated order details.</returns>
        Task<OrderResDto> UpdateOrderAsync(int id, OrderReqDto orderDto, CurrentUser currentUser);

        /// <summary>
        /// Deletes an order with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order to delete.</param>
        /// <param name="currentUser">The current user deleting the order.</param>
        /// <param name="isPermanent">permanent delete - Default false.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteOrderAsync(int id, CurrentUser currentUser, bool isPermanent = false);

        /// <summary>
        /// Updates the status of an existing order.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="status">The new status to set for the order.</param>
        /// <param name="currentUser">The current user updating the order status.</param>
        /// <returns>The updated order details.</returns>
        Task<OrderResDto> UpdateOrderStatusAsync(int id, pos_service.Models.Enums.MainOrderStatus status, CurrentUser currentUser);


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
        Task<List<OrderResDto>> GetOrdersByDateAndStatusAsync(DateTime? startDate, DateTime? endDate, pos_service.Models.Enums.MainOrderStatus? status, pos_service.Models.Enums.OrderSubStatus? subStatus, CurrentUser currentUser);

        // Returns returned-items summary rows (from view) for given order number
        Task<List<pos_service.Models.DTO.ReturnedItems.ReturnedItemsSummaryResDto>> GetReturnedItemsSummaryByOrderNumberAsync(string orderNumber, CurrentUser currentUser);
    }
}