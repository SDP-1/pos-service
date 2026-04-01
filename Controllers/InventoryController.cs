using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Inventory;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : SystemBaseController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryResDto>>> GetAll()
        {
            var result = await _inventoryService.GetAllAsync(_currentUser);
            return Ok(result);
        }

        [HttpGet("{itemUuid:guid}")]
        public async Task<ActionResult<InventoryResDto>> GetByItemUuid(string itemUuid)
        {
            var inventory = await _inventoryService.GetByItemUuidAsync(itemUuid, _currentUser);
            if (inventory == null)
                return NotFound();

            return Ok(inventory);
        }

        [HttpPut("{itemUuid:guid}")]
        public async Task<ActionResult<InventoryResDto>> Upsert(string itemUuid, [FromBody] InventoryReqDto dto)
        {
            var inventory = await _inventoryService.UpsertAsync(itemUuid, dto, _currentUser);
            return Ok(inventory);
        }

        [HttpPost("{itemUuid:guid}/adjust")]
        public async Task<ActionResult<InventoryResDto>> AdjustStock(string itemUuid, [FromBody] InventoryAdjustReqDto dto)
        {
            var inventory = await _inventoryService.AdjustStockAsync(itemUuid, dto, _currentUser);
            if (inventory == null)
                return NotFound();

            return Ok(inventory);
        }
    }
}
