using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Authorization;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Purchases;
using pos_service.Models.Enums;
using pos_service.Services;
using pos_service.Services.Purchases;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchasesController : SystemBaseController
    {
        private readonly IPurchaseService _purchaseService;

        public PurchasesController(
            IPurchaseService purchaseService,
            ICurrentUserService currentUserService
        ) : base(currentUserService)
        {
            _purchaseService = purchaseService;
        }

        /// <summary>
        /// Retrieves all purchases accessible to the current user.
        /// </summary>
        /// <returns>200 OK with the collection of purchase record DTOs.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseResDto>>> GetAll()
        {
            var purchases = await _purchaseService.GetAllPurchasesAsync(_currentUser);
            return Ok(purchases);
        }

        /// <summary>
        /// Retrieves detailed information for a specific purchase by its unique identifier (UUID),
        /// including purchased items, batch allocations, costs, and supplier details.
        /// </summary>
        /// <param name="uuid">The unique identifier (UUID) of the purchase record.</param>
        /// <returns>200 OK with the purchase details DTO, or 404 NotFound if not found.</returns>
        [HttpGet("{uuid}")]
        public async Task<ActionResult<PurchaseResDto>> GetByUuid(string uuid)
        {
            var purchase = await _purchaseService.GetByUuidAsync(uuid, _currentUser);
            if (purchase == null)
            {
                return NotFound($"Purchase with UUID {uuid} not found");
            }
            return Ok(purchase);
        }

        /// <summary>
        /// Creates a new purchase / goods receipt record, updating inventory quantities,
        /// generating inventory batches, and recording pricing details.
        /// </summary>
        /// <param name="dto">The purchase request DTO containing header info, purchased items, and batch details.</param>
        /// <returns>201 CreatedAtAction with the newly created purchase details DTO.</returns>
        [HttpPost]
        [Permission(PermissionType.ITEM_ADD)]
        public async Task<ActionResult<PurchaseResDto>> Create([FromBody] PurchaseReqDto dto)
        {
            var created = await _purchaseService.CreatePurchaseAsync(dto, _currentUser);
            return CreatedAtAction(nameof(GetByUuid), new { uuid = created.Uuid }, created);
        }

        /// <summary>
        /// Deletes / voids a purchase record by its unique identifier (UUID) and updates inventory accordingly.
        /// </summary>
        /// <param name="uuid">The unique identifier (UUID) of the purchase record to delete.</param>
        /// <returns>204 NoContent on successful deletion, or 404 NotFound if not found.</returns>
        [HttpDelete("{uuid}")]
        [Permission(PermissionType.ITEM_DELETE)]
        public async Task<ActionResult> Delete(string uuid)
        {
            var deleted = await _purchaseService.DeletePurchaseAsync(uuid, _currentUser);
            if (!deleted)
            {
                return NotFound($"Purchase with UUID {uuid} not found");
            }
            return NoContent();
        }
    }
}
