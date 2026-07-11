using pos_service.Models;
using pos_service.Models.DTO.Reports;

namespace pos_service.Repositories
{
    public interface IReportTemplateRepository
    {
        /// <summary>
        /// Retrieves all active report templates mapped to response DTOs.
        /// </summary>
        /// <returns>A collection of report template response DTOs.</returns>
        Task<IEnumerable<ReportTemplateResDto>> GetAllTemplatesAsync();

        /// <summary>
        /// Retrieves an active report template by its identifier, mapped to a response DTO.
        /// </summary>
        /// <param name="id">The unique ID of the report template.</param>
        /// <returns>The report template response DTO if found and active, otherwise null.</returns>
        Task<ReportTemplateResDto?> GetTemplateByIdAsync(int id);

        /// <summary>
        /// Retrieves an active report template by its unique UUID, mapped to a response DTO.
        /// </summary>
        /// <param name="uuid">The UUID of the report template.</param>
        /// <returns>The report template response DTO if found and active, otherwise null.</returns>
        Task<ReportTemplateResDto?> GetTemplateByUuidAsync(string uuid);

        /// <summary>
        /// Retrieves a report template by its name, mapped to a response DTO.
        /// </summary>
        /// <param name="name">The name of the report template.</param>
        /// <returns>The report template response DTO if found and active, otherwise null.</returns>
        Task<ReportTemplateResDto?> GetTemplateByNameAsync(string name);

        /// <summary>
        /// Retrieves the tracking entity of a report template by its UUID.
        /// Used for modifications and updates.
        /// </summary>
        /// <param name="uuid">The UUID of the report template.</param>
        /// <returns>The report template entity if found, otherwise null.</returns>
        Task<ReportTemplate?> GetTemplateEntityByUuidAsync(string uuid);

        /// <summary>
        /// Adds a new report template to the data store.
        /// </summary>
        /// <param name="template">The report template entity to add.</param>
        /// <returns>The added report template entity.</returns>
        Task<ReportTemplate> AddTemplateAsync(ReportTemplate template);

        /// <summary>
        /// Updates an existing report template in the data store.
        /// </summary>
        /// <param name="template">The updated report template entity.</param>
        /// <returns>The updated report template entity.</returns>
        Task<ReportTemplate> UpdateTemplateAsync(ReportTemplate template);

        /// <summary>
        /// Performs a soft delete on a report template by setting its IsActive flag to false.
        /// </summary>
        /// <param name="uuid">The UUID of the report template to delete.</param>
        /// <returns>True if the template was deleted successfully, otherwise false.</returns>
        Task<bool> DeleteTemplateAsync(string uuid);
    }
}
