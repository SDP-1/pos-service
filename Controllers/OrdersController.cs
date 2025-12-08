using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models;
using pos_service.Models.DTO.Order;
using pos_service.Models.Enums;
using pos_service.Services;
using pos_service.Authorization;
using pos_service.Exceptions;

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
            try
            {
                var order = await _orderService.CreateOrderAsync(orderDto, _currentUser);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                var order = await _orderService.GetOrderAsync(id, _currentUser);
                if (order == null)
                    return Ok("Order not found");

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                var order = await _orderService.GetOrderByUuidAsync(uuid, _currentUser);
                if (order == null)
                    return Ok("Order not found");

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                var order = await _orderService.GetOrderByOrderNumberAsync(number, _currentUser);
                if (order == null)
                    return Ok("Order not found");

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                var orders = await _orderService.GetOrdersAsync(query, _currentUser);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                var order = await _orderService.UpdateOrderAsync(id, orderDto, _currentUser);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
            try
            {
                if (permanent)
                    EnsurePermission(PermissionType.ORDER_DELETE_PERMANENTLY);

                var result = await _orderService.DeleteOrderAsync(id, _currentUser, permanent);
                if (!result)
                    return NotFound("Order not found");

                return Ok("Successfully deleted");
            }
            catch (PermissionDeniedException ex) {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the status of an existing order.
        /// </summary>
        /// <param name="id">The unique identifier of the order to update.</param>
        /// <param name="status"> New status.</param>
        /// <returns>The updated order details if successful.</returns>
        [HttpPatch("{id:int}/status/{status:int}")]
        [Permission(PermissionType.ORDER_UPDATE_STATUS)]
        public async Task<ActionResult<OrderResDto>> UpdateOrderStatus(int id, OrderStatus status)
        {
            if (status == OrderStatus.Default)
                return BadRequest("status is required.");

            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, status, _currentUser);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}