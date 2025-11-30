using pos_service.Models;
using pos_service.Models.DTO.Order;
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
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteOrderAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Updates the status of an existing order.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="status">The new status to set for the order.</param>
        /// <param name="currentUser">The current user updating the order status.</param>
        /// <returns>The updated order details.</returns>
        Task<OrderResDto> UpdateOrderStatusAsync(int id, OrderStatus status, CurrentUser currentUser);
    }
}