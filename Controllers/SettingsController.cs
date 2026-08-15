using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Authorization;
using pos_service.Controllers.Base;
using pos_service.Models;
using pos_service.Models.DTO.Settings;
using pos_service.Models.Enums;
using pos_service.Services;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for managing system settings in the POS system.
    /// Provides endpoints for viewing and updating application-wide settings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : SystemBaseController
    {
        private readonly ISettingService _settingService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the SettingsController class.
        /// </summary>
        /// <param name="settingService">The setting service for business logic operations.</param>
        /// <param name="currentUserService">The current user service for authentication context.</param>
        /// <param name="mapper">AutoMapper instance for DTO conversions.</param>
        public SettingsController(
            ISettingService settingService, 
            ICurrentUserService currentUserService,
            IMapper mapper) : base(currentUserService)
        {
            _settingService = settingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all system settings.
        /// </summary>
        /// <returns>A list of all settings in the system.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SettingResDto>>> GetAllSettings()
        {
            var settings = await _settingService.GetAllAsync(_currentUser);
            var settingsDto = _mapper.Map<IEnumerable<SettingResDto>>(settings);
            return Ok(settingsDto);
        }

        /// <summary>
        /// Retrieves a setting by its key.
        /// </summary>
        /// <param name="key">The setting key to search for.</param>
        /// <returns>The setting details if found, otherwise returns NotFound.</returns>
        [HttpGet("key/{key}")]
        [Permission(PermissionType.SETTING_MANAGE)]
        public async Task<ActionResult<SettingResDto>> GetSettingByKey(SettingKey key)
        {
            var setting = await _settingService.GetByKeyAsync(key, _currentUser);
            if (setting == null)
                return NotFound("Setting not found");

            var settingDto = _mapper.Map<SettingResDto>(setting);
            return Ok(settingDto);
        }
        /// <summary>
        /// Turn a setting on or off by key.
        /// </summary>
        /// <param name="key">The setting key to set.</param>
        /// <param name="value">True to enable, false to disable.</param>
        [HttpPatch("key/{key}/value/{value:bool}")]
        [Permission(PermissionType.SETTING_MANAGE)]
        public async Task<ActionResult<SettingResDto>> SetSettingValue(SettingKey key, bool value)
        {
            // Only system admin (role 1) is allowed to change settings even though permission is SETTING_VIEW.
            if (!_currentUser.IsInRole(1))
                return Forbid();

            var updated = await _settingService.SetSettingValueAsync(key, value, _currentUser);
            if (updated == null) return NotFound("Setting not found");

            return Ok(_mapper.Map<SettingResDto>(updated));
        }

        /// <summary>
        /// Toggle endpoint for updating setting value via PUT {key}/toggle/{value:bool}.
        /// </summary>
        [HttpPut("{key}/toggle/{value:bool}")]
        [Permission(PermissionType.SETTING_MANAGE)]
        [Permission(PermissionType.REPORT_VISIBILITY_MANAGE)]
        public async Task<IActionResult> UpdateToggle(string key, bool value)
        {
            if (!Enum.TryParse<SettingKey>(key, ignoreCase: true, out var settingKey))
                return BadRequest(new { message = $"Unknown setting key: {key}" });

            var updated = await _settingService.SetSettingValueAsync(settingKey, value, _currentUser);
            if (updated == null) return NotFound($"Setting not found for key: {key}");

            return Ok(new { message = "Setting updated successfully.", key, value });
        }
    }
}
