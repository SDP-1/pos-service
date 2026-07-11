using AutoMapper;
using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Reports;
using pos_service.Repositories;
using pos_service.Security;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace pos_service.Services
{
    /// <summary>
    /// Service implementation for managing and executing SQL templates.
    /// </summary>
    public class SqlTemplateService : ISqlTemplateService
    {
        private readonly ISqlTemplateRepository _repository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTemplateService"/> class.
        /// </summary>
        /// <param name="repository">The SQL template repository.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="mapper">The auto-mapper instance.</param>
        public SqlTemplateService(ISqlTemplateRepository repository, AppDbContext context, IMapper mapper)
        {
            _repository = repository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all active SQL templates.
        /// </summary>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>A collection of SQL template response DTOs.</returns>
        public async Task<IEnumerable<SqlTemplateResDto>> GetAllTemplatesAsync(CurrentUser currentUser)
        {
            return await _repository.GetAllTemplatesAsync();
        }

        /// <summary>
        /// Retrieves an SQL template by its unique UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>The matching SQL template response DTO, or null if not found.</returns>
        public async Task<SqlTemplateResDto?> GetTemplateByUuidAsync(string uuid, CurrentUser currentUser)
        {
            return await _repository.GetTemplateByUuidAsync(uuid);
        }

        /// <summary>
        /// Creates a new SQL template.
        /// </summary>
        /// <param name="dto">The creation model payload.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>The created SQL template response DTO.</returns>
        /// <exception cref="ArgumentNullException">Thrown if dto is null.</exception>
        /// <exception cref="ArgumentException">Thrown if query fails SELECT validation or template name is duplicated.</exception>
        public async Task<SqlTemplateResDto> CreateTemplateAsync(SqlTemplateReqDto dto, CurrentUser currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Validate that the query only executes SELECT statements (prevents injection/writes)
            if (!SqlQueryValidator.ValidateSelectOnly(dto.SqlQuery, out var errorMsg))
            {
                throw new ArgumentException(errorMsg);
            }

            // Ensure the template name is globally unique
            var existing = await _repository.GetTemplateByNameAsync(dto.TemplateName);
            if (existing != null)
            {
                throw new ArgumentException($"A SQL Template named '{dto.TemplateName}' already exists.");
            }

            var template = new SqlTemplate
            {
                Uuid = Guid.NewGuid().ToString(),
                TemplateName = dto.TemplateName,
                Description = dto.Description,
                SqlQuery = dto.SqlQuery,
                PlaceholdersJson = JsonSerializer.Serialize(dto.Placeholders),
                SelectValuesJson = JsonSerializer.Serialize(dto.SelectValues),
                IsActive = dto.IsActive
            };

            var created = await _repository.AddTemplateAsync(template);
            var result = await _repository.GetTemplateByUuidAsync(created.Uuid);
            return result!;
        }

        /// <summary>
        /// Updates an existing SQL template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to update.</param>
        /// <param name="dto">The updated data model payload.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>The updated SQL template response DTO.</returns>
        /// <exception cref="ArgumentNullException">Thrown if dto is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if template is not found.</exception>
        /// <exception cref="ArgumentException">Thrown if query fails SELECT validation or template name duplicate conflict.</exception>
        public async Task<SqlTemplateResDto> UpdateTemplateAsync(string uuid, SqlTemplateReqDto dto, CurrentUser currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var template = await _repository.GetTemplateEntityByUuidAsync(uuid);
            if (template == null) throw new KeyNotFoundException("SQL Template not found.");

            // Validate query safety constraints
            if (!SqlQueryValidator.ValidateSelectOnly(dto.SqlQuery, out var errorMsg))
            {
                throw new ArgumentException(errorMsg);
            }

            // Check that updated template name does not conflict with another existing template
            var existing = await _repository.GetTemplateByNameAsync(dto.TemplateName);
            if (existing != null && existing.Uuid != uuid)
            {
                throw new ArgumentException($"A SQL Template named '{dto.TemplateName}' already exists.");
            }

            // Prevent deactivating if currently used by any report templates
            if (!dto.IsActive && template.IsActive)
            {
                var referencedReportNames = await _context.ReportTemplateSqlTemplates
                    .Where(rtst => rtst.SqlTemplateId == template.Id)
                    .Select(rtst => rtst.ReportTemplate.ReportName)
                    .Distinct()
                    .ToListAsync();

                if (referencedReportNames.Any())
                {
                    throw new ArgumentException($"Cannot deactivate SQL Template '{template.TemplateName}' because it is currently used by the following Report Template(s): {string.Join(", ", referencedReportNames)}");
                }
            }

            template.TemplateName = dto.TemplateName;
            template.Description = dto.Description;
            template.SqlQuery = dto.SqlQuery;
            template.PlaceholdersJson = JsonSerializer.Serialize(dto.Placeholders);
            template.SelectValuesJson = JsonSerializer.Serialize(dto.SelectValues);
            template.IsActive = dto.IsActive;
            template.UpdatedAt = DateTime.Now;

            var updated = await _repository.UpdateTemplateAsync(template);
            var result = await _repository.GetTemplateByUuidAsync(updated.Uuid);
            return result!;
        }

        /// <summary>
        /// Deletes an SQL template by UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to delete.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>True if deletion succeeded, otherwise false.</returns>
        public async Task<bool> DeleteTemplateAsync(string uuid, CurrentUser currentUser)
        {
            var template = await _context.SqlTemplates
                .FirstOrDefaultAsync(t => t.Uuid == uuid);
            if (template == null) return false;

            var referencedReportNames = await _context.ReportTemplateSqlTemplates
                .Where(rtst => rtst.SqlTemplateId == template.Id)
                .Select(rtst => rtst.ReportTemplate.ReportName)
                .Distinct()
                .ToListAsync();

            if (referencedReportNames.Any())
            {
                throw new ArgumentException($"Cannot delete SQL Template '{template.TemplateName}' because it is currently used by the following Report Template(s): {string.Join(", ", referencedReportNames)}");
            }

            return await _repository.DeleteTemplateAsync(uuid);
        }

        /// <summary>
        /// Normalizes placeholder keys by removing spaces, underscores, and converting to lowercase.
        /// </summary>
        /// <param name="key">The raw key string.</param>
        /// <returns>The normalized key string.</returns>
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            return key.Replace("_", "").Replace(" ", "").ToLowerInvariant();
        }

        /// <summary>
        /// Executes an SQL template dynamically by parsing placeholders, binding ADO.NET parameters, and fetching tabular rows.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the SQL template.</param>
        /// <param name="parameterValues">Dictionary of runtime user parameter mappings.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>A list of result rows represented as column-value dictionaries.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if SQL template is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if validation of SQL SELECT safety checks fail.</exception>
        /// <exception cref="ArgumentException">Thrown if any required parameter is missing from the request.</exception>
        public async Task<List<Dictionary<string, object>>> ExecuteTemplateAsync(string uuid, Dictionary<string, string> parameterValues, CurrentUser currentUser)
        {
            var template = await _repository.GetTemplateByUuidAsync(uuid);
            if (template == null) throw new KeyNotFoundException("SQL Template not found.");

            var query = template.SqlQuery;

            // Extra safety validation before raw SQL execution
            if (!SqlQueryValidator.ValidateSelectOnly(query, out var validationError))
            {
                throw new InvalidOperationException($"SQL Execution Blocked: {validationError}");
            }

            var definitions = template.Placeholders;

            // Regex matches placeholders wrapped in double curly braces, e.g. {{my_param}}, with optional single/double surrounding quotes
            var pattern = @"['""]?\{\{([a-zA-Z0-9_]+)\}\}['""]?";
            var matches = Regex.Matches(query, pattern);

            // Using dictionary to map placeholder names to their SQL Parameter values without duplicate keys
            var sqlParams = new Dictionary<string, object?>();

            foreach (Match match in matches)
            {
                var placeholderName = match.Groups[1].Value;
                var paramKey = placeholderName.ToLowerInvariant();

                if (sqlParams.ContainsKey(paramKey)) continue;

                // Load definition from template mappings using normalized keys (ignores spaces and underscores)
                var normPH = NormalizeKey(placeholderName);
                var def = definitions.FirstOrDefault(d => NormalizeKey(d.Name) == normPH);
                
                // Retrieve user input value matched by normalized key
                string? rawVal = null;
                foreach (var kvp in parameterValues)
                {
                    if (NormalizeKey(kvp.Key) == normPH)
                    {
                        rawVal = kvp.Value;
                        break;
                    }
                }

                // Enforce required constraint validation
                if (def != null && def.Required && string.IsNullOrEmpty(rawVal))
                {
                    throw new ArgumentException($"Required query parameter '{placeholderName}' is missing.");
                }

                // Parse user value according to DTO declared data type (Number/Date/Text)
                object? boundVal = null;
                if (!string.IsNullOrEmpty(rawVal))
                {
                    if (def?.DataType == "Number")
                    {
                        if (decimal.TryParse(rawVal, out var decVal)) boundVal = decVal;
                        else boundVal = rawVal;
                    }
                    else if (def?.DataType == "Date")
                    {
                        if (DateTime.TryParse(rawVal, out var dateVal))
                        {
                            if (placeholderName.Contains("end", StringComparison.OrdinalIgnoreCase))
                            {
                                dateVal = dateVal.Date.AddDays(1).AddSeconds(-1);
                            }
                            boundVal = dateVal;
                        }
                        else boundVal = rawVal;
                    }
                    else
                    {
                        boundVal = rawVal;
                    }
                }

                sqlParams.Add(paramKey, boundVal);
            }

            // Replace template double-curly placeholders in SQL string with database parameter name tokens (@paramKey)
            var preparedSql = Regex.Replace(query, pattern, m => "@" + m.Groups[1].Value.ToLowerInvariant());

            var results = new List<Dictionary<string, object>>();

            // Establish direct ADO.NET connection via EF Core DbContext to run raw queries
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = preparedSql;
                command.CommandType = CommandType.Text;

                // Map and bind standard DbParameters safely to prevent SQL injection vulnerabilities
                foreach (var sp in sqlParams)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + sp.Key;
                    parameter.Value = sp.Value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }

                // Execute query reader and project records dynamically to rows mapping column names to cell values
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var colName = reader.GetName(i);
                        var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[colName] = val!;
                    }
                    results.Add(row);
                }

                return results;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
