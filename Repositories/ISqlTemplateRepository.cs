using pos_service.Models;
using pos_service.Models.DTO.Reports;

namespace pos_service.Repositories
{
    public interface ISqlTemplateRepository
    {
        /// <summary>
        /// Retrieves all active SQL templates mapped to response DTOs.
        /// </summary>
        /// <returns>A collection of SQL template response DTOs.</returns>
        Task<IEnumerable<SqlTemplateResDto>> GetAllTemplatesAsync();

        /// <summary>
        /// Retrieves an active SQL template by its identifier, mapped to a response DTO.
        /// </summary>
        /// <param name="id">The unique ID of the SQL template.</param>
        /// <returns>The SQL template response DTO if found and active, otherwise null.</returns>
        Task<SqlTemplateResDto?> GetTemplateByIdAsync(int id);

        /// <summary>
        /// Retrieves an active SQL template by its unique UUID, mapped to a response DTO.
        /// </summary>
        /// <param name="uuid">The UUID of the SQL template.</param>
        /// <returns>The SQL template response DTO if found and active, otherwise null.</returns>
        Task<SqlTemplateResDto?> GetTemplateByUuidAsync(string uuid);

        /// <summary>
        /// Retrieves a SQL template by its name, mapped to a response DTO.
        /// </summary>
        /// <param name="name">The name of the SQL template.</param>
        /// <returns>The SQL template response DTO if found and active, otherwise null.</returns>
        Task<SqlTemplateResDto?> GetTemplateByNameAsync(string name);

        /// <summary>
        /// Retrieves the tracking entity of a SQL template by its UUID.
        /// Used for modifications and updates.
        /// </summary>
        /// <param name="uuid">The UUID of the SQL template.</param>
        /// <returns>The SQL template entity if found, otherwise null.</returns>
        Task<SqlTemplate?> GetTemplateEntityByUuidAsync(string uuid);

        /// <summary>
        /// Adds a new SQL template to the data store.
        /// </summary>
        /// <param name="template">The SQL template entity to add.</param>
        /// <returns>The added SQL template entity.</returns>
        Task<SqlTemplate> AddTemplateAsync(SqlTemplate template);

        /// <summary>
        /// Updates an existing SQL template in the data store.
        /// </summary>
        /// <param name="template">The updated SQL template entity.</param>
        /// <returns>The updated SQL template entity.</returns>
        Task<SqlTemplate> UpdateTemplateAsync(SqlTemplate template);

        /// <summary>
        /// Performs a soft delete on a SQL template by setting its IsActive flag to false.
        /// </summary>
        /// <param name="uuid">The UUID of the SQL template to delete.</param>
        /// <returns>True if the template was deleted successfully, otherwise false.</returns>
        Task<bool> DeleteTemplateAsync(string uuid);
    }
}
