using pos_service.Models;
using pos_service.Models.DTO.Reports;

namespace pos_service.Services
{
    /// <summary>
    /// Service interface for managing Report templates and rendering report layouts.
    /// </summary>
    public interface IReportTemplateService
    {
        /// <summary>
        /// Retrieves all report templates in the system.
        /// </summary>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>A collection of report template response DTOs.</returns>
        Task<IEnumerable<ReportTemplateResDto>> GetAllTemplatesAsync(CurrentUser currentUser);

        /// <summary>
        /// Retrieves a report template by its unique UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>The report template response DTO, or null if not found.</returns>
        Task<ReportTemplateResDto?> GetTemplateByUuidAsync(string uuid, CurrentUser currentUser);

        /// <summary>
        /// Creates a new report template.
        /// </summary>
        /// <param name="dto">The creation template model.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>The created report template response DTO.</returns>
        Task<ReportTemplateResDto> CreateTemplateAsync(ReportTemplateReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing report template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to update.</param>
        /// <param name="dto">The update template model.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>The updated report template response DTO, or null if not found.</returns>
        Task<ReportTemplateResDto?> UpdateTemplateAsync(string uuid, ReportTemplateReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Deletes a report template by UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to delete.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>True if deleted successfully, otherwise false.</returns>
        Task<bool> DeleteTemplateAsync(string uuid, CurrentUser currentUser);



        /// <summary>
        /// Generates the HTML report populated with dynamic query values.
        /// </summary>
        /// <param name="uuid">The unique UUID identifier of the report template.</param>
        /// <param name="parameterValues">Dictionary of user selected parameters mapping placeholder keys to actual values.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>A tuple containing the rendered HTML content string and the PDF target filename.</returns>
        Task<(string HtmlContent, string Filename)> GenerateDynamicReportAsync(string uuid, Dictionary<string, string> parameterValues, CurrentUser currentUser);
    }
}
