using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models;
using pos_service.Models.DTO.Orders;
using pos_service.Models.Enums;
using pos_service.Services;
using pos_service.Authorization;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for managing orders in the POS system.
    /// Provides comprehensive order management operations including creation, retrieval, updating, and status management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : SystemBaseController
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the OrdersController class.
        /// </summary>
        /// <param name="orderService">The order service for business logic operations.</param>
        /// <param name="currentUserService">The current user service for authentication context.</param>
        public OrdersController(IOrderService orderService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a new order in the system.
        /// </summary>
        /// <param name="orderDto">The order data transfer object containing order information.</param>
        /// <returns>The newly created order details with location header.</returns>
        [HttpPost]
        [Permission(PermissionType.ORDER_ADD)]
        public async Task<ActionResult<OrderResDto>> CreateOrder([FromBody] OrderReqDto orderDto)
        {
           var order = await _orderService.CreateOrderAsync(orderDto, _currentUser);
           return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        /// <summary>
        /// Retrieves a specific order by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order.</param>
        /// <returns>The order details if found, otherwise returns NotFound.</returns>
        [HttpGet("{id:int}")]
        [Permission(PermissionType.ORDER_VIEW)]
        public async Task<ActionResult<OrderResDto>> GetOrder(int id)
        {
           var order = await _orderService.GetOrderAsync(id, _currentUser);
           if (order == null)
               return NotFound("Order not found");

            return Ok(order);
        }

        /// <summary>
        /// Record a loan settlement payment for an order.
        /// </summary>
        [HttpPost("{id:int}/settle")]
        [Permission(PermissionType.ORDER_SETTLEMENT)]
        public async Task<ActionResult<OrderResDto>> RecordSettlement(int id, [FromBody] LoanSettlementLogReqDto dto)
        {
            var order = await _orderService.RecordSettlementAsync(id, dto.AmountPaid, dto.Description, _currentUser);
            return Ok(order);
        }

        /// <summary>
        /// Retrieves an order by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the order to retrieve.</param>
        /// <returns>The order details if found, otherwise returns NotFound.</returns>
        [HttpGet("uuid/{uuid}")]
        [Permission(PermissionType.ORDER_VIEW)]
        public async Task<ActionResult<OrderResDto>> GetOrderByUuid(string uuid)
        {
           var order = await _orderService.GetOrderByUuidAsync(uuid, _currentUser);
           if (order == null)
               return NotFound("Order not found");

            return Ok(order);
        }

        /// <summary>
        /// Retrieves an order by its order number.
        /// </summary>
        /// <param name="number">The order number to search for.</param>
        /// <returns>The order details if found, otherwise returns NotFound.</returns>
        [HttpGet("number/{number}")]
        [Permission(PermissionType.ORDER_VIEW)]
        public async Task<ActionResult<OrderResDto>> GetOrderByOrderNumber(string number)
        {
            var order = await _orderService.GetOrderByOrderNumberAsync(number, _currentUser);
            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        /// <summary>
        /// Returns the order and enriches each order item with returned quantity and remaining quantity.
        /// </summary>
        [HttpGet("number/{number}/with-returns")]
        [Permission(PermissionType.ORDER_VIEW)]
        public async Task<ActionResult<OrderResDto>> GetOrderByOrderNumberWithReturns(string number)
        {
            var order = await _orderService.GetOrderWithReturnedItemsAsync(number, _currentUser);
            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        /// <summary>
        /// Retrieves a list of orders based on the specified query parameters.
        /// </summary>
        /// <param name="query">The query parameters for filtering and paginating orders.</param>
        /// <returns>A paginated list of orders matching the query criteria.</returns>
        [HttpGet]
        [Permission(PermissionType.ORDER_VIEW)]
        public async Task<ActionResult<OrderListResDto>> GetOrders([FromQuery] OrderQueryDto query)
        {
            var orders = await _orderService.GetOrdersAsync(query, _currentUser);
            return Ok(orders);
        }

        /// <summary>
        /// Updates an existing order with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="orderDto">The order data transfer object containing updated information.</param>
        /// <returns>The updated order details if successful.</returns>
        [HttpPut("{id:int}")]
        [Permission(PermissionType.ORDER_UPDATE)]
        public async Task<ActionResult<OrderResDto>> UpdateOrder(int id, [FromBody] OrderReqDto orderDto)
        {
           var order = await _orderService.UpdateOrderAsync(id, orderDto, _currentUser);
           return Ok(order);
        }

        /// <summary>
        /// Deletes an order with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order to delete.</param>
        /// <returns>NoContent if successful, otherwise returns NotFound.</returns>
        [HttpDelete("{id:int}/{permanent:bool?}")]
        [Permission(PermissionType.ORDER_DELETE)]
        public async Task<ActionResult> DeleteOrder(int id, bool permanent = false)
        {
            if (permanent)
                EnsurePermission(PermissionType.ORDER_DELETE_PERMANENTLY);

            var result = await _orderService.DeleteOrderAsync(id, _currentUser, permanent);
            if (!result)
                return NotFound("Order not found");

            return Ok("Successfully deleted");
        }

        /// <summary>
        /// Updates the status of an existing order.
        /// /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="status"> New status.</param>
        /// <returns>The updated order details if successful.</returns>
        [HttpPatch("{id:int}/status/{status:int}")]
        [Permission(PermissionType.ORDER_UPDATE_STATUS)]
        public async Task<ActionResult<OrderResDto>> UpdateOrderStatus(int id, pos_service.Models.Enums.MainOrderStatus status)
        {
            if (status == pos_service.Models.Enums.MainOrderStatus.Default)
                return BadRequest("status is required.");

                var order = await _orderService.UpdateOrderStatusAsync(id, status, _currentUser);
                return Ok(order);
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
        [HttpGet("report/by-date-status")]
        [Permission(PermissionType.ORDER_VIEW)]
        public async Task<ActionResult<List<OrderResDto>>> GetOrdersByDateAndStatus(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] pos_service.Models.Enums.MainOrderStatus? status,
            [FromQuery] pos_service.Models.Enums.OrderSubStatus? subStatus)
        {
            var orders = await _orderService.GetOrdersByDateAndStatusAsync(startDate, endDate, status, subStatus, _currentUser);
            return Ok(orders);
        }
    }
}