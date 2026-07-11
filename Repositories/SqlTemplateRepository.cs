using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Reports;
using System.Text.Json;

namespace pos_service.Repositories
{
    /// <summary>
    /// Repository class handling data access and projection logic for SQL templates.
    /// </summary>
    public class SqlTemplateRepository : ISqlTemplateRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTemplateRepository"/> class.
        /// </summary>
        /// <param name="context">The database context instance.</param>
        public SqlTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all SQL templates mapped to response DTOs.
        /// </summary>
        /// <returns>A collection of SQL template response DTOs.</returns>
        public async Task<IEnumerable<SqlTemplateResDto>> GetAllTemplatesAsync()
        {
            var query = _context.SqlTemplates;

            return await makeSqlTemplateResponseDto(query);
        }

        /// <summary>
        /// Retrieves a SQL template by its identifier, mapped to a response DTO.
        /// </summary>
        /// <param name="id">The unique ID of the SQL template.</param>
        /// <returns>The SQL template response DTO if found, otherwise null.</returns>
        public async Task<SqlTemplateResDto?> GetTemplateByIdAsync(int id)
        {
            var query = _context.SqlTemplates
                .Where(t => t.Id == id);

            var result = await makeSqlTemplateResponseDto(query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a SQL template by its unique UUID, mapped to a response DTO.
        /// </summary>
        /// <param name="uuid">The UUID of the SQL template.</param>
        /// <returns>The SQL template response DTO if found, otherwise null.</returns>
        public async Task<SqlTemplateResDto?> GetTemplateByUuidAsync(string uuid)
        {
            var query = _context.SqlTemplates
                .Where(t => t.Uuid == uuid);

            var result = await makeSqlTemplateResponseDto(query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a SQL template by its name, mapped to a response DTO.
        /// </summary>
        /// <param name="name">The name of the SQL template.</param>
        /// <returns>The SQL template response DTO if found, otherwise null.</returns>
        public async Task<SqlTemplateResDto?> GetTemplateByNameAsync(string name)
        {
            var query = _context.SqlTemplates
                .Where(t => t.TemplateName.ToLower() == name.ToLower());

            var result = await makeSqlTemplateResponseDto(query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the tracking entity of a SQL template by its UUID.
        /// Used for modifications and updates.
        /// </summary>
        /// <param name="uuid">The UUID of the SQL template.</param>
        /// <returns>The tracked SQL template entity if found, otherwise null.</returns>
        public async Task<SqlTemplate?> GetTemplateEntityByUuidAsync(string uuid)
        {
            return await _context.SqlTemplates
                .FirstOrDefaultAsync(t => t.Uuid == uuid);
        }

        /// <summary>
        /// Adds a new SQL template to the database.
        /// </summary>
        /// <param name="template">The SQL template entity to add.</param>
        /// <returns>The added SQL template entity with generated database identifiers.</returns>
        public async Task<SqlTemplate> AddTemplateAsync(SqlTemplate template)
        {
            _context.SqlTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        /// <summary>
        /// Updates an existing SQL template in the database.
        /// </summary>
        /// <param name="template">The updated SQL template entity.</param>
        /// <returns>The updated SQL template entity.</returns>
        public async Task<SqlTemplate> UpdateTemplateAsync(SqlTemplate template)
        {
            _context.Entry(template).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return template;
        }

        /// <summary>
        /// Performs a hard delete on a SQL template and removes linked join mappings.
        /// </summary>
        /// <param name="uuid">The UUID of the SQL template to delete.</param>
        /// <returns>True if the template was deleted successfully, otherwise false.</returns>
        public async Task<bool> DeleteTemplateAsync(string uuid)
        {
            var template = await _context.SqlTemplates.FirstOrDefaultAsync(t => t.Uuid == uuid);
            if (template == null) return false;

            _context.SqlTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Helper mapping method to load SQL template entities and map them to response DTOs in-memory.
        /// </summary>
        /// <param name="query">The SQL queryable to execute.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of mapped SQL template response DTOs.</returns>
        private async Task<List<SqlTemplateResDto>> makeSqlTemplateResponseDto(IQueryable<SqlTemplate> query)
        {
            var templates = await query
                .AsNoTracking()
                .ToListAsync();

            return templates.Select(t => new SqlTemplateResDto
            {
                Id = t.Id,
                Uuid = t.Uuid,
                TemplateName = t.TemplateName,
                Description = t.Description,
                SqlQuery = t.SqlQuery,
                Placeholders = SafeDeserializeList<SqlPlaceholderDto>(t.PlaceholdersJson),
                SelectValues = SafeDeserializeList<SqlSelectValueDto>(t.SelectValuesJson),
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                UpdatedAt = t.UpdatedAt,
                UpdatedBy = t.UpdatedBy
            }).ToList();
        }

        /// <summary>
        /// Safely deserializes JSON to List&lt;T&gt;, returning an empty list on any error.
        /// </summary>
        /// <typeparam name="T">The type to deserialize list items to.</typeparam>
        /// <param name="json">The JSON string representation of the list.</param>
        /// <returns>A list of deserialized elements, or an empty list if deserialization fails.</returns>
        private static List<T> SafeDeserializeList<T>(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<T>();
            try 
            { 
                return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>(); 
            }
            catch 
            { 
                return new List<T>(); 
            }
        }
    }
}
