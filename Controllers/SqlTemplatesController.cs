using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Authorization;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Reports;
using pos_service.Models.Enums;
using pos_service.Services;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller handling API endpoints for managing SQL templates.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SqlTemplatesController : SystemBaseController
    {
        private readonly ISqlTemplateService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTemplatesController"/> class.
        /// </summary>
        /// <param name="service">The SQL template service.</param>
        /// <param name="currentUserService">The current user service profile.</param>
        public SqlTemplatesController(ISqlTemplateService service, ICurrentUserService currentUserService) 
            : base(currentUserService)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all active SQL templates.
        /// </summary>
        /// <returns>A list of active SQL templates.</returns>
        [HttpGet]
        [Permission(PermissionType.SQL_TEMPLATE_VIEW)]
        public async Task<ActionResult<IEnumerable<SqlTemplateResDto>>> GetAll()
        {
            var templates = await _service.GetAllTemplatesAsync(_currentUser);
            return Ok(templates);
        }

        /// <summary>
        /// Retrieves a single SQL template by UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the SQL template.</param>
        /// <returns>The matching SQL template or a 404 response.</returns>
        [HttpGet("{uuid}")]
        [Permission(PermissionType.SQL_TEMPLATE_VIEW)]
        public async Task<ActionResult<SqlTemplateResDto>> GetByUuid(string uuid)
        {
            var template = await _service.GetTemplateByUuidAsync(uuid, _currentUser);
            if (template == null) return NotFound("SQL Template not found");
            return Ok(template);
        }

        /// <summary>
        /// Creates a new SQL template.
        /// </summary>
        /// <param name="dto">The creation model payload.</param>
        /// <returns>The created SQL template model with route information.</returns>
        [HttpPost]
        [Permission(PermissionType.SQL_TEMPLATE_CREATE)]
        public async Task<ActionResult<SqlTemplateResDto>> Create([FromBody] SqlTemplateReqDto dto)
        {
            try
            {
                var created = await _service.CreateTemplateAsync(dto, _currentUser);
                return CreatedAtAction(nameof(GetByUuid), new { uuid = created.Uuid }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing SQL template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to update.</param>
        /// <param name="dto">The updated data model payload.</param>
        /// <returns>The updated SQL template model.</returns>
        [HttpPut("{uuid}")]
        [Permission(PermissionType.SQL_TEMPLATE_EDIT)]
        public async Task<ActionResult<SqlTemplateResDto>> Update(string uuid, [FromBody] SqlTemplateReqDto dto)
        {
            try
            {
                var updated = await _service.UpdateTemplateAsync(uuid, dto, _currentUser);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes an SQL template by UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to delete.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("{uuid}")]
        [Permission(PermissionType.SQL_TEMPLATE_DELETE)]
        public async Task<IActionResult> Delete(string uuid)
        {
            try
            {
                var success = await _service.DeleteTemplateAsync(uuid, _currentUser);
                if (!success) return NotFound("SQL Template not found");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
