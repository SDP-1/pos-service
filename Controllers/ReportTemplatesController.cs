using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Authorization;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Reports;
using pos_service.Models.Enums;
using pos_service.Services;
using System.Text;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller handling API endpoints for managing and executing report templates.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportTemplatesController : SystemBaseController
    {
        private readonly IReportTemplateService _service;
        private readonly IPdfService            _pdfService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportTemplatesController"/> class.
        /// </summary>
        /// <param name="service">The report template service.</param>
        /// <param name="pdfService">The PDF conversion service.</param>
        /// <param name="currentUserService">The current user service profile.</param>
        public ReportTemplatesController(IReportTemplateService service, IPdfService pdfService, ICurrentUserService currentUserService) 
            : base(currentUserService)
        {
            _service    = service;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Retrieves all report templates in the system.
        /// </summary>
        /// <returns>A list of report templates.</returns>
        [HttpGet]
        [Permission(PermissionType.REPORT_TEMPLATE_VIEW)]
        public async Task<ActionResult<IEnumerable<ReportTemplateResDto>>> GetAll()
        {
            var templates = await _service.GetAllTemplatesAsync(_currentUser);
            return Ok(templates);
        }

        /// <summary>
        /// Retrieves a report template by its unique UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template.</param>
        /// <returns>The matching report template or a 404 response.</returns>
        [HttpGet("{uuid}")]
        [Permission(PermissionType.REPORT_TEMPLATE_VIEW)]
        public async Task<ActionResult<ReportTemplateResDto>> GetByUuid(string uuid)
        {
            var template = await _service.GetTemplateByUuidAsync(uuid, _currentUser);
            if (template == null) return NotFound("Report template not found");
            return Ok(template);
        }

        /// <summary>
        /// Creates a new report template.
        /// </summary>
        /// <param name="dto">The creation model payload.</param>
        /// <returns>The created template model with route information.</returns>
        [HttpPost]
        [Permission(PermissionType.REPORT_TEMPLATE_CREATE)]
        public async Task<ActionResult<ReportTemplateResDto>> Create([FromBody] ReportTemplateReqDto dto)
        {
            try
            {
                var created = await _service.CreateTemplateAsync(dto, _currentUser);
                return CreatedAtAction(nameof(GetByUuid), new { uuid = created.Uuid }, created);
            }
            catch (InvalidOperationException ex)
            {
                // Conflict status indicates name duplication or template constraints
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing report template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to update.</param>
        /// <param name="dto">The updated data model payload.</param>
        /// <returns>The updated report template model.</returns>
        [HttpPut("{uuid}")]
        [Permission(PermissionType.REPORT_TEMPLATE_EDIT)]
        public async Task<ActionResult<ReportTemplateResDto>> Update(string uuid, [FromBody] ReportTemplateReqDto dto)
        {
            try
            {
                var updated = await _service.UpdateTemplateAsync(uuid, dto, _currentUser);
                if (updated == null) return NotFound("Report template not found");
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a report template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to delete.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("{uuid}")]
        [Permission(PermissionType.REPORT_TEMPLATE_DELETE)]
        public async Task<IActionResult> Delete(string uuid)
        {
            var success = await _service.DeleteTemplateAsync(uuid, _currentUser);
            if (!success) return NotFound("Report template not found");
            return NoContent();
        }

        /// <summary>
        /// Imports and extracts raw string content from an uploaded HTML file.
        /// </summary>
        /// <param name="file">The form file attachment (.html extension required).</param>
        /// <returns>An object containing the parsed HTML content string.</returns>
        [HttpPost("import")]
        [Permission(PermissionType.REPORT_TEMPLATE_CREATE)]
        public async Task<IActionResult> ImportTemplate([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Enforce file extension check
            if (!Path.GetExtension(file.FileName).Equals(".html", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .html files are supported");

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            return Ok(new { content });
        }

        /// <summary>
        /// Downloads the template raw HTML markup as an attachment.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the report template.</param>
        /// <returns>A download file stream containing the HTML markup.</returns>
        [HttpGet("{uuid}/download")]
        [Permission(PermissionType.REPORT_TEMPLATE_DOWNLOAD)]
        public async Task<IActionResult> DownloadTemplate(string uuid)
        {
            var template = await _service.GetTemplateByUuidAsync(uuid, _currentUser);
            if (template == null) return NotFound("Template not found");

            var bytes = Encoding.UTF8.GetBytes(template.HtmlContent);
            return File(bytes, "text/html", $"{template.ReportName}.html");
        }


        /// <summary>
        /// Generates a dynamic report PDF using target parameters and executes matching SQL queries.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the report template to execute.</param>
        /// <param name="parameterValues">Dictionary of bound prompt values mapped by parameter key.</param>
        /// <returns>The generated PDF document file stream.</returns>
        [HttpPost("{uuid}/execute")]
        [Permission(PermissionType.REPORT_TEMPLATE_DOWNLOAD)]
        public async Task<IActionResult> ExecuteReport(string uuid, [FromBody] Dictionary<string, string> parameterValues)
        {
            try
            {
                // Generate dynamic HTML by compiling SQL queries and template tags
                var (htmlContent, filename) = await _service.GenerateDynamicReportAsync(uuid, parameterValues ?? new Dictionary<string, string>(), _currentUser);
                
                // Perform HTML-to-PDF conversion inside the dedicated browser service
                byte[] pdfBytes = await _pdfService.ConvertHtmlToPdfAsync(htmlContent);

                // Expose target filename header so frontend client can download with proper naming
                Response.Headers["X-Report-Filename"]             = filename;
                Response.Headers["Access-Control-Expose-Headers"] = "X-Report-Filename, Content-Disposition";

                return File(pdfBytes, "application/pdf", filename);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
