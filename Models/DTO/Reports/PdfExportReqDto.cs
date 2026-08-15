namespace pos_service.Models.DTO.Reports
{
    /// <summary>
    /// Data transfer object for requesting PDF report export.
    /// </summary>
    public class PdfExportReqDto
    {
        /// <summary>
        /// Gets or sets the raw HTML content string to be converted into PDF format.
        /// </summary>
        public string HtmlContent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title for the generated PDF document file.
        /// </summary>
        public string Title { get; set; } = "Report";
    }
}
