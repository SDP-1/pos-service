using Microsoft.AspNetCore.Mvc;
using pos_service.Models;
using pos_service.Services;
using Microsoft.Extensions.Logging;
using pos_service.Models.DTO.Settings;
using pos_service.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using pos_service.Authorization;
using pos_service.Models.Enums;

namespace pos_service.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ShopController : SystemBaseController
    {
        private readonly IShopService _shopService;
        private readonly ILogger<ShopController> _logger;

        public ShopController(IShopService shopService, ILogger<ShopController> logger, ICurrentUserService currentUserService) :base(currentUserService)
        {
            _shopService = shopService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dto = await _shopService.GetAsync();
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Permission(PermissionType.SHOP_DETAILS_UPDATE)]
        public async Task<IActionResult> CreateOrUpdate([FromForm] ShopReqDto req)
        {
            if (!ModelState.IsValid) 
                   return BadRequest(ModelState);

            var dto = await _shopService.CreateOrUpdateAsync(req, _currentUser);
            _logger?.LogInformation("Shop create/update performed by {user}", _currentUser.Uuid);
            return Ok(dto);
        }
    }
}
