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
    /// Controller for handling PDF document exports.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PdfController : SystemBaseController
    {
        private readonly IPdfService _pdfService;

        public PdfController(
            IPdfService pdfService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _pdfService = pdfService;
        }

        /// <summary>
        /// Converts HTML content into a downloadable PDF binary document.
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportPdf([FromBody] PdfExportReqDto req)
        {
            if (string.IsNullOrWhiteSpace(req.HtmlContent))
                return BadRequest("HtmlContent is required");

            byte[] pdfBytes = await _pdfService.ConvertHtmlToPdfAsync(req.HtmlContent);
            string fileName = $"{req.Title.Replace(" ", "_")}_{DateTime.Now:yyyy-MM-dd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
