using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.InventoryBatches;
using pos_service.Models.DTO.StockMovements;
using pos_service.Models.Enums;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryBatchesController : SystemBaseController
    {
        private readonly IInventoryBatchService _batchService;

        public InventoryBatchesController(
            IInventoryBatchService batchService,
            ICurrentUserService currentUserService
        ) : base(currentUserService)
        {
            _batchService = batchService;
        }

        /// <summary>
        /// Retrieves all batches for an item (optionally including expired ones).
        /// </summary>
        [HttpGet("item/{itemUuid}")]
        public async Task<ActionResult<IEnumerable<InventoryBatchResDto>>> GetByItemUuid(string itemUuid, [FromQuery] bool includeExpired = false)
        {
            var batches = await _batchService.GetBatchesByItemUuidAsync(itemUuid, includeExpired, _currentUser);
            return Ok(batches);
        }

        /// <summary>
        /// Retrieves active batches for POS item selection with FEFO recommendation indicator.
        /// </summary>
        [HttpGet("pos/{itemUuid}")]
        public async Task<ActionResult<IEnumerable<InventoryBatchSelectDto>>> GetBatchesForPos(string itemUuid)
        {
            var batches = await _batchService.GetBatchesForPosAsync(itemUuid, _currentUser);
            return Ok(batches);
        }

        /// <summary>
        /// Creates a new batch with batch-specific pricing and stock.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<InventoryBatchResDto>> CreateBatch([FromBody] InventoryBatchReqDto dto)
        {
            var created = await _batchService.CreateBatchAsync(dto, _currentUser);
            return Ok(created);
        }

        /// <summary>
        /// Updates batch pricing in-place for active batch.
        /// </summary>
        [HttpPut("{batchUuid}/prices")]
        public async Task<ActionResult<InventoryBatchResDto>> UpdateBatchPrices(
            string batchUuid,
            [FromQuery] decimal costPrice,
            [FromQuery] decimal markedPrice,
            [FromQuery] decimal retailPrice,
            [FromQuery] decimal wholesalePrice,
            [FromQuery] decimal retailDiscountRatio = 0.0m,
            [FromQuery] decimal wholesaleDiscountRatio = 0.0m)
        {
            var result = await _batchService.UpdateBatchPricesAsync(
                batchUuid,
                costPrice,
                markedPrice,
                retailPrice,
                wholesalePrice,
                retailDiscountRatio,
                wholesaleDiscountRatio,
                _currentUser);
            return Ok(result);
        }

        /// <summary>
        /// Adjusts stock for a specific batch (write-off, damage, manual adjustment).
        /// </summary>
        [HttpPost("{batchUuid}/adjust")]
        public async Task<ActionResult<InventoryBatchResDto>> AdjustBatch(
            string batchUuid,
            [FromQuery] decimal quantityDelta,
            [FromQuery] StockMovementType movementType = StockMovementType.MANUAL_ADJUST_IN,
            [FromQuery] string? reason = null,
            [FromQuery] string? comment = null)
        {
            var result = await _batchService.AdjustBatchStockAsync(batchUuid, quantityDelta, movementType, reason, comment, _currentUser);
            return Ok(result);
        }

        /// <summary>
        /// Deactivates or updates active status of a batch (requires 0 remaining stock to deactivate).
        /// </summary>
        [HttpPatch("{batchUuid}/status")]
        public async Task<ActionResult<InventoryBatchResDto>> SetStatus(
            string batchUuid,
            [FromQuery] bool isActive,
            [FromQuery] BatchStatus? status = null)
        {
            var result = await _batchService.SetBatchStatusAsync(batchUuid, isActive, status, _currentUser);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves immutable stock movement ledger records for an item.
        /// </summary>
        [HttpGet("movements/{itemUuid}")]
        public async Task<ActionResult<IEnumerable<StockMovementResDto>>> GetMovementsByItem(string itemUuid)
        {
            var movements = await _batchService.GetStockMovementsByItemUuidAsync(itemUuid, _currentUser);
            return Ok(movements);
        }

        /// <summary>
        /// Retrieves audit logs for an item's batches.
        /// </summary>
        [HttpGet("logs/{itemUuid}")]
        public async Task<ActionResult<IEnumerable<InventoryBatchLogResDto>>> GetLogsByItem(string itemUuid)
        {
            var logs = await _batchService.GetBatchLogsByItemUuidAsync(itemUuid, _currentUser);
            return Ok(logs);
        }
    }
}
