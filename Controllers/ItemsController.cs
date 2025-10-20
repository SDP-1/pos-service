using Microsoft.AspNetCore.Mvc;
using pos_service.Models.DTO;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        // GET: api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetAllItems()
        {
            var items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }

        // GET: api/items/1001/0
        [HttpGet("{id:int}/{subId:int}")]
        public async Task<ActionResult<ItemResDto>> GetItemById(int id, int subId)
        {
            var item = await _itemService.GetItemByIdAsync(id, subId);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // GET: api/items/main/1001
        [HttpGet("main/{id:int}")]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetItemsByMainId(int id)
        {
            var items = await _itemService.GetItemsByMainIdAsync(id);
            return Ok(items);
        }

        // GET: api/items/barcode/5449000000996
        [HttpGet("barcode/{barCode}")]
        public async Task<ActionResult<IEnumerable<ItemResDto>>> GetItemByBarCode(string barCode)
        {
            var item = await _itemService.GetItemByBarCodeAsync(barCode);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // GET: api/items/uuid/a1b2c3d4-....
        [HttpGet("uuid/{uuid:guid}")]
        public async Task<ActionResult<ItemResDto>> GetItemByUuid(Guid uuid)
        {
            var item = await _itemService.GetItemByUuidAsync(uuid);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // GET: api/items/quantity/main/1001
        [HttpGet("quantity/main/{id:int}")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetQuantitiesByMainId(int id)
        {
            var quantities = await _itemService.GetQuantitiesByMainIdAsync(id);
            return Ok(quantities);
        }

        // GET: api/items/quantity/uuid/a1b2c3d4-....
        [HttpGet("quantity/uuid/{uuid:guid}")]
        public async Task<ActionResult<decimal>> GetQuantityByUuid(Guid uuid)
        {
            var quantity = await _itemService.GetQuantityByUuidAsync(uuid);
            if (quantity == null)
            {
                return NotFound();
            }
            return Ok(quantity.Value);
        }

        // GET: api/items/quantity/id/1001/0
        [HttpGet("quantity/id/{id:int}/{subId:int}")]
        public async Task<ActionResult<decimal>> GetQuantityById(int id, int subId)
        {
            var quantity = await _itemService.GetQuantityByIdAsync(id, subId);
            if (quantity == null)
            {
                return NotFound();
            }
            return Ok(quantity.Value);
        }

        // --- POST/PUT/DELETE ENDPOINTS ---

        // POST: api/items
        [HttpPost]
        public async Task<ActionResult<ItemResDto>> CreateItem([FromBody] ItemReqDto itemDto)
        {
            var newItem = await _itemService.CreateItemAsync(itemDto);
            if (newItem == null)
            {
                return Conflict("An item with the same Id and SubId already exists.");
            }
            return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id, subId = newItem.SubId }, newItem);
        }

        // POST: api/items/1001/0/add-stock?quantity=10.5
        [HttpPost("{id:int}/{subId:int}/add-stock")]
        public async Task<ActionResult<ItemResDto>> AddStock(int id, int subId, [FromQuery] decimal quantity = 0)
        {
            var updatedItem = await _itemService.AddStockAsync(id, subId, quantity);
            if (updatedItem == null)
            {
                return NotFound();
            }
            return Ok(updatedItem);
        }

        // PUT: api/items/1001/0
        [HttpPut("{id:int}/{subId:int}")]
        public async Task<IActionResult> UpdateItem(int id, int subId, [FromBody] ItemReqDto itemDto)
        {
            if (id != itemDto.Id || subId != itemDto.SubId)
            {
                return BadRequest("The route parameters must match the item's Id and SubId.");
            }

            var success = await _itemService.UpdateItemAsync(id, subId, itemDto);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/items/1001/0
        [HttpDelete("{id:int}/{subId:int}")]
        public async Task<IActionResult> DeleteItem(int id, int subId)
        {
            var success = await _itemService.DeleteItemAsync(id, subId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
