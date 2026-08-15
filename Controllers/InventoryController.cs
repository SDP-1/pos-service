using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.DTO.Items;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : SystemBaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IItemService      _itemService;

        public InventoryController(
            IInventoryService inventoryService, 
            IItemService itemService, 
            ICurrentUserService currentUserService
            ) : base(currentUserService)
        {
            _inventoryService = inventoryService;
            _itemService      = itemService;
        }

        /// <summary>
        /// Retrieves all items supplied by the specified supplier including inventory details.
        /// </summary>
        [HttpGet("supplier/{supplierId}/items")]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetItemsBySupplier(int supplierId)
        {
            var items = await _itemService.GetItemsBySupplierIdAsync(supplierId, _currentUser);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves all inventories accessible to the current user.
        /// </summary>
        /// <returns>200 OK with the list of inventory records.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryResDto>>> GetAll()
        {
            var result = await _inventoryService.GetAllAsync(_currentUser);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves inventory by the associated item UUID.
        /// </summary>
        /// <param name="itemUuid">UUID of the item to fetch inventory for.</param>
        /// <returns>200 OK with the inventory or 404 NotFound when missing.</returns>
        [HttpGet("{itemUuid:guid}")]
        public async Task<ActionResult<InventoryResDto>> GetByItemUuid(string itemUuid)
        {
            var inventory = await _inventoryService.GetByItemUuidAsync(itemUuid, _currentUser);
            if (inventory == null)
                return NotFound();

            return Ok(inventory);
        }

        /// <summary>
        /// Creates or updates inventory record for the specified item UUID.
        /// </summary>
        [HttpPut("{itemUuid:guid}")]
        public async Task<ActionResult<InventoryResDto>> Update(string itemUuid, [FromBody] InventoryReqDto dto)
        {
            var inventory = await _inventoryService.UpdateAsync(itemUuid, dto, _currentUser);
            return Ok(inventory);
        }

        /// <summary>
        /// Adjusts stock quantity for the specified item.
        /// </summary>
        /// <param name="itemUuid">UUID of the item to adjust.</param>
        /// <param name="dto">Adjustment details including increase/decrease and reason.</param>
        /// <returns>200 OK with the updated inventory or 404 NotFound when item is missing.</returns>
        [HttpPost("{itemUuid:guid}/adjust")]
        public async Task<ActionResult<InventoryResDto>> AdjustStock(string itemUuid, [FromBody] InventoryAdjustReqDto dto)
        {
            // Delegate stock adjustment to service which performs validation and audit logging
            var inventory = await _inventoryService.AdjustStockAsync(itemUuid, dto, _currentUser);
            if (inventory == null)
                return NotFound();

            return Ok(inventory);
        }

        /// <summary>
        /// Get inventory adjustment audit history for an item.
        /// </summary>
        /// <param name="itemUuid">Item UUID to query (required)</param>
        /// <param name="startDate">Start date for filtering adjustments (optional)</param>
        /// <param name="endDate">End date for filtering adjustments (optional)</param>
        /// <param name="maxRecords">Maximum number of records to return (optional, default 100)</param>
        /// <returns>List of inventory audit history records</returns>
        [HttpGet("{itemUuid:guid}/audit-history")]
        public async Task<ActionResult<IEnumerable<InventoryAdjustAuditResDto>>> GetAuditHistory(
            string itemUuid,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? maxRecords = null)
        {
            try
            {
                var auditHistory = await _inventoryService.GetAuditHistoryAsync(
                    itemUuid,
                    startDate,
                    endDate,
                    maxRecords,
                    _currentUser);

                return Ok(auditHistory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
