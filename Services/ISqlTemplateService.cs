using pos_service.Models;
using pos_service.Models.DTO.Reports;

namespace pos_service.Services
{
    /// <summary>
    /// Service interface for managing SQL templates.
    /// </summary>
    public interface ISqlTemplateService
    {
        /// <summary>
        /// Retrieves all active SQL templates.
        /// </summary>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>A list of SQL template response DTOs.</returns>
        Task<IEnumerable<SqlTemplateResDto>> GetAllTemplatesAsync(CurrentUser currentUser);

        /// <summary>
        /// Retrieves an SQL template by its unique UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>The SQL template response DTO, or null if not found.</returns>
        Task<SqlTemplateResDto?> GetTemplateByUuidAsync(string uuid, CurrentUser currentUser);

        /// <summary>
        /// Creates a new SQL template.
        /// </summary>
        /// <param name="dto">The creation template model.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>The created SQL template response DTO.</returns>
        Task<SqlTemplateResDto> CreateTemplateAsync(SqlTemplateReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing SQL template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to update.</param>
        /// <param name="dto">The update template model.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>The updated SQL template response DTO.</returns>
        Task<SqlTemplateResDto> UpdateTemplateAsync(string uuid, SqlTemplateReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Deletes an SQL template by UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to delete.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>True if deleted successfully, otherwise false.</returns>
        Task<bool> DeleteTemplateAsync(string uuid, CurrentUser currentUser);

        /// <summary>
        /// Executes a template SQL query by binding user parameter values dynamically.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the SQL template.</param>
        /// <param name="parameterValues">Dictionary mapping placeholder keys to user input values.</param>
        /// <param name="currentUser">The current authenticated user.</param>
        /// <returns>A list of rows, where each row is represented as a dictionary of column names to values.</returns>
        Task<List<Dictionary<string, object>>> ExecuteTemplateAsync(string uuid, Dictionary<string, string> parameterValues, CurrentUser currentUser);
    }
}
