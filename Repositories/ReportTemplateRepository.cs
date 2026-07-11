using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Reports;
using System.Text.Json;

namespace pos_service.Repositories
{
    /// <summary>
    /// Repository class handling data access and projection logic for report templates.
    /// </summary>
    public class ReportTemplateRepository : IReportTemplateRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportTemplateRepository"/> class.
        /// </summary>
        /// <param name="context">The database context instance.</param>
        public ReportTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all active report templates mapped to response DTOs.
        /// </summary>
        /// <returns>A collection of active report template response DTOs.</returns>
        public async Task<IEnumerable<ReportTemplateResDto>> GetAllTemplatesAsync()
        {
            var query = _context.ReportTemplates
                .OrderByDescending(t => t.CreatedAt);

            return await makeReportTemplateResponseDto(query);
        }

        /// <summary>
        /// Retrieves an active report template by its identifier, mapped to a response DTO.
        /// </summary>
        /// <param name="id">The unique ID of the report template.</param>
        /// <returns>The report template response DTO if found and active, otherwise null.</returns>
        public async Task<ReportTemplateResDto?> GetTemplateByIdAsync(int id)
        {
            var query = _context.ReportTemplates
                .Where(t => t.Id == id);

            var result = await makeReportTemplateResponseDto(query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves an active report template by its unique UUID, mapped to a response DTO.
        /// </summary>
        /// <param name="uuid">The UUID of the report template.</param>
        /// <returns>The report template response DTO if found and active, otherwise null.</returns>
        public async Task<ReportTemplateResDto?> GetTemplateByUuidAsync(string uuid)
        {
            var query = _context.ReportTemplates
                .Where(t => t.Uuid == uuid);

            var result = await makeReportTemplateResponseDto(query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a report template by its name, mapped to a response DTO.
        /// </summary>
        /// <param name="name">The name of the report template.</param>
        /// <returns>The report template response DTO if found and active, otherwise null.</returns>
        public async Task<ReportTemplateResDto?> GetTemplateByNameAsync(string name)
        {
            var query = _context.ReportTemplates
                .Where(t => t.ReportName.ToLower() == name.ToLower());

            var result = await makeReportTemplateResponseDto(query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves the tracking entity of a report template by its UUID.
        /// Used for modifications and updates.
        /// </summary>
        /// <param name="uuid">The UUID of the report template.</param>
        /// <returns>The tracked report template entity if found, otherwise null.</returns>
        public async Task<ReportTemplate?> GetTemplateEntityByUuidAsync(string uuid)
        {
            return await _context.ReportTemplates
                .FirstOrDefaultAsync(t => t.Uuid == uuid);
        }

        /// <summary>
        /// Adds a new report template to the database.
        /// </summary>
        /// <param name="template">The report template entity to add.</param>
        /// <returns>The added report template entity with generated database identifiers.</returns>
        public async Task<ReportTemplate> AddTemplateAsync(ReportTemplate template)
        {
            _context.ReportTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        /// <summary>
        /// Updates an existing report template in the database.
        /// </summary>
        /// <param name="template">The updated report template entity.</param>
        /// <returns>The updated report template entity.</returns>
        public async Task<ReportTemplate> UpdateTemplateAsync(ReportTemplate template)
        {
            _context.Entry(template).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return template;
        }

        /// <summary>
        /// Performs a hard delete on a report template by removing it from the database.
        /// </summary>
        /// <param name="uuid">The UUID of the report template to delete.</param>
        /// <returns>True if the template was deleted successfully, otherwise false.</returns>
        public async Task<bool> DeleteTemplateAsync(string uuid)
        {
            var template = await _context.ReportTemplates.FirstOrDefaultAsync(t => t.Uuid == uuid);
            if (template == null) return false;

            // Hard delete
            _context.ReportTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Helper mapping method to load report template entities and associated SQL templates, mapping them to response DTOs in-memory.
        /// </summary>
        /// <param name="query">The report queryable to execute.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of mapped report template response DTOs.</returns>
        private async Task<List<ReportTemplateResDto>> makeReportTemplateResponseDto(IQueryable<ReportTemplate> query)
        {
            var results = await query
                .AsNoTracking()
                .Select(t => new
                {
                    ReportTemplate = t,
                    SqlTemplates = t.ReportTemplateSqlTemplates
                        .Where(rtst => rtst.SqlTemplate != null && rtst.SqlTemplate.IsActive && rtst.IsActive)
                        .Select(rtst => rtst.SqlTemplate)
                        .ToList()
                })
                .ToListAsync();

            return results.Select(x => new ReportTemplateResDto
            {
                Id = x.ReportTemplate.Id,
                Uuid = x.ReportTemplate.Uuid,
                ReportName = x.ReportTemplate.ReportName,
                Description = x.ReportTemplate.Description,
                HtmlContent = x.ReportTemplate.HtmlContent,
                IsActive = x.ReportTemplate.IsActive,
                CreatedAt = x.ReportTemplate.CreatedAt,
                CreatedBy = x.ReportTemplate.CreatedBy,
                UpdatedAt = x.ReportTemplate.UpdatedAt,
                UpdatedBy = x.ReportTemplate.UpdatedBy,
                Parameters = SafeDeserialize<ReportParametersDto>(x.ReportTemplate.ParametersJson) ?? new ReportParametersDto(),
                SqlPlaceholderMappings = SafeDeserializeList<SqlPlaceholderMappingDto>(x.ReportTemplate.SqlPlaceholderMappingsJson),
                SqlTemplates = x.SqlTemplates.Select(st => new SqlTemplateResDto
                {
                    Id = st.Id,
                    Uuid = st.Uuid,
                    TemplateName = st.TemplateName,
                    Description = st.Description,
                    SqlQuery = st.SqlQuery,
                    Placeholders = SafeDeserializeList<SqlPlaceholderDto>(st.PlaceholdersJson),
                    SelectValues = SafeDeserializeList<SqlSelectValueDto>(st.SelectValuesJson),
                    IsActive = st.IsActive,
                    CreatedAt = st.CreatedAt,
                    CreatedBy = st.CreatedBy,
                    UpdatedAt = st.UpdatedAt,
                    UpdatedBy = st.UpdatedBy
                }).ToList()
            }).ToList();
        }

        /// <summary>
        /// Safely deserializes JSON to T, returning null on any error.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON string to.</typeparam>
        /// <param name="json">The JSON string representation of the object.</param>
        /// <returns>The deserialized object of type T, or null if deserialization fails.</returns>
        private static T? SafeDeserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try 
            { 
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); 
            }
            catch 
            { 
                return null; 
            }
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
