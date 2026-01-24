using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models;
using pos_service.Models.DTO.Items;
using pos_service.Services;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for managing items in the POS system.
    /// Provides comprehensive CRUD operations for item management with administrative access control.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemsController : SystemBaseController
    {
        private readonly IItemService _itemService;

        /// <summary>
        /// Initializes a new instance of the ItemsController class.
        /// </summary>
        /// <param name="itemService">The item service for business logic operations.</param>
        /// <param name="currentUserService">The current user service for authentication context.</param>
        public ItemsController(IItemService itemService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _itemService = itemService;
        }

        /// <summary>
        /// Retrieves all items from the system.
        /// </summary>
        /// <returns>A list of all items in the system.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetAllItems()
        {
            var items = await _itemService.GetAllItemsAsync(_currentUser);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a specific item by its composite identifier (ID and SubID).
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>The item details if found, otherwise returns NotFound.</returns>
        [HttpGet("{id:int}/{subId:int}")]
        public async Task<ActionResult<ItemResDto>> GetItemById(int id, int subId)
        {
            var item = await _itemService.GetItemByIdAsync(id, subId, _currentUser);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        /// <summary>
        /// Retrieves all items that share the same main identifier.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <returns>A list of items with the specified main ID.</returns>
        [HttpGet("main/{id:int}")]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetItemsByMainId(int id)
        {
            var items = await _itemService.GetItemsByMainIdAsync(id, _currentUser);
            return Ok(items);
        }

        /// <summary>
        /// Retrieves minimal item details by barcode for quick lookups.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <returns>Minimal item details if found, otherwise returns NotFound.</returns>
        [HttpGet("barcode/{barCode}/min")]
        public async Task<ActionResult<IEnumerable<ItemMiniResDto>>> GetItemMinDetailsByBarCode(string barCode)
        {
            var items = await _itemService.GetItemMinDetailsByBarCodeAsync(barCode, _currentUser);
            if (items == null)
            {
                return NotFound();
            }
            return Ok(items);
        }

        /// <summary>
        /// Retrieves complete item details by barcode.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <returns>Complete item details if found, otherwise returns NotFound.</returns>
        [HttpGet("barcode/{barCode}")]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetItemByBarCode(string barCode)
        {
            var item = await _itemService.GetItemByBarCodeAsync(barCode, _currentUser);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        /// <summary>
        /// Retrieves an item by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the item to retrieve.</param>
        /// <returns>The item details if found, otherwise returns NotFound.</returns>
        [HttpGet("uuid/{uuid:guid}")]
        public async Task<ActionResult<ItemResDto>> GetItemByUuid(string uuid)
        {
            var item = await _itemService.GetItemByUuidAsync(uuid, _currentUser);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        /// <summary>
        /// Retrieves quantity information for all items with the specified main ID.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <returns>A dictionary containing quantity information for the items.</returns>
        [HttpGet("quantity/main/{id:int}")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetQuantitiesByMainId(int id)
        {
            var quantities = await _itemService.GetQuantitiesByMainIdAsync(id, _currentUser);
            return Ok(quantities);
        }

        /// <summary>
        /// Retrieves the current quantity of an item by its UUID.
        /// </summary>
        /// <param name="uuid">The UUID of the item.</param>
        /// <returns>The quantity value if found, otherwise returns NotFound.</returns>
        [HttpGet("quantity/uuid/{uuid:guid}")]
        public async Task<ActionResult<decimal>> GetQuantityByUuid(string uuid)
        {
            var quantity = await _itemService.GetQuantityByUuidAsync(uuid, _currentUser);
            if (quantity == null)
            {
                return NotFound();
            }
            return Ok(quantity.Value);
        }

        /// <summary>
        /// Retrieves the current quantity of an item by its composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>The quantity value if found, otherwise returns NotFound.</returns>
        [HttpGet("quantity/id/{id:int}/{subId:int}")]
        public async Task<ActionResult<decimal>> GetQuantityById(int id, int subId)
        {
            var quantity = await _itemService.GetQuantityByIdAsync(id, subId, _currentUser);
            if (quantity == null)
            {
                return NotFound();
            }
            return Ok(quantity.Value);
        }

        /// <summary>
        /// Creates a new item in the system.
        /// </summary>
        /// <param name="itemDto">The item data transfer object containing item information.</param>
        /// <returns>The newly created item details with location header.</returns>
        [HttpPost]
        public async Task<ActionResult<ItemResDto>> CreateItem([FromBody] ItemReqDto itemDto)
        {
            var newItem = await _itemService.CreateItemAsync(itemDto, _currentUser);
            if (newItem == null)
            {
                return Conflict("An item with the same Id and SubId already exists.");
            }
            return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id, subId = newItem.SubId }, newItem);
        }

        /// <summary>
        /// Adds stock quantity to an existing item. This endpoint allows non-admin access.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="quantity">The quantity to add to the item's stock.</param>
        /// <returns>The updated item details if successful, otherwise returns NotFound.</returns>
        [HttpPost("{id:int}/{subId:int}/add-stock")]
        public async Task<ActionResult<ItemResDto>> AddStock(int id, int subId, [FromQuery] decimal quantity = 0)
        {
            var updatedItem = await _itemService.AddStockAsync(id, subId, quantity, _currentUser);
            if (updatedItem == null)
            {
                return NotFound();
            }
            return Ok(updatedItem);
        }

        /// <summary>
        /// Updates an existing item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to update.</param>
        /// <param name="subId">The sub-identifier of the item to update.</param>
        /// <param name="itemDto">The item data transfer object containing updated information.</param>
        /// <returns>NoContent if successful, BadRequest if IDs don't match, or NotFound if item doesn't exist.</returns>
        [HttpPut("{id:int}/{subId:int}")]
        public async Task<ActionResult<ItemResDto>> UpdateItem(int id, int subId, [FromBody] ItemReqDto itemDto)
        {
            // For updates the body must include Id and SubId and they must match the route.
            if (!itemDto.Id.HasValue || !itemDto.SubId.HasValue)
            {
                return BadRequest("Request body must include Id and SubId for updates.");
            }

            if (id != itemDto.Id.Value || subId != itemDto.SubId.Value)
            {
                return BadRequest("The route parameters must match the item's Id and SubId.");
            }

            var result = await _itemService.UpdateItemAsync(id, subId, itemDto, _currentUser);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Deletes an item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to delete.</param>
        /// <param name="subId">The sub-identifier of the item to delete.</param>
        /// <returns>NoContent if successful, otherwise returns NotFound.</returns>
        [HttpDelete("{id:int}/{subId:int}")]
        public async Task<IActionResult> DeleteItem(int id, int subId)
        {
            var success = await _itemService.DeleteItemAsync(id, subId, _currentUser);
            if (!success)
                return NotFound();

            return Ok();
        }

        /// <summary>
        /// Retrieves all items supplied by a specific supplier ID.
        /// </summary>
        /// <param name="supplierId">The unique identifier of the supplier.</param>
        /// <returns>A list of items associated with the specified supplier.</returns>
        [HttpGet("supplier/{supplierId:int}")]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetItemsBySupplierId(int supplierId)
        {
            var items = await _itemService.GetItemsBySupplierIdAsync(supplierId, _currentUser);
            if (items == null || !items.Any())
            {
                return NotFound($"No items found for supplier ID {supplierId}.");
            }
            return Ok(items);
        }
    }
}