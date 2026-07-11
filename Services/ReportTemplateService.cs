using AutoMapper;
using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Reports;
using pos_service.Repositories;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace pos_service.Services
{
    /// <summary>
    /// Service implementation for managing report templates, resolving database placeholders, and executing dynamic layouts.
    /// </summary>
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly IReportTemplateRepository _repository;
        private readonly IMapper _mapper;
        private readonly IShopRepository _shopRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISqlTemplateService _sqlTemplateService;
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportTemplateService"/> class.
        /// </summary>
        /// <param name="repository">The report template repository.</param>
        /// <param name="mapper">The automapper instance.</param>
        /// <param name="shopRepository">The shop data repository.</param>
        /// <param name="orderRepository">The orders data repository.</param>
        /// <param name="itemRepository">The items data repository.</param>
        /// <param name="customerRepository">The customer data repository.</param>
        /// <param name="sqlTemplateService">The SQL template service.</param>
        /// <param name="context">The database application context.</param>
        public ReportTemplateService(
            IReportTemplateRepository repository,
            IMapper mapper,
            IShopRepository shopRepository,
            IOrderRepository orderRepository,
            IItemRepository itemRepository,
            ICustomerRepository customerRepository,
            ISqlTemplateService sqlTemplateService,
            AppDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _shopRepository = shopRepository;
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _customerRepository = customerRepository;
            _sqlTemplateService = sqlTemplateService;
            _context = context;
        }

        /// <summary>
        /// Retrieves all report templates in the system.
        /// </summary>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>A collection of report template response DTOs.</returns>
        public async Task<IEnumerable<ReportTemplateResDto>> GetAllTemplatesAsync(CurrentUser currentUser)
        {
            return await _repository.GetAllTemplatesAsync();
        }

        /// <summary>
        /// Retrieves a report template by its unique UUID.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>The matching report template response DTO, or null if not found.</returns>
        public async Task<ReportTemplateResDto?> GetTemplateByUuidAsync(string uuid, CurrentUser currentUser)
        {
            return await _repository.GetTemplateByUuidAsync(uuid);
        }

        /// <summary>
        /// Creates a new report template.
        /// </summary>
        /// <param name="dto">The creation model payload.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>The created report template response DTO.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a report template with the same name already exists.</exception>
        public async Task<ReportTemplateResDto> CreateTemplateAsync(ReportTemplateReqDto dto, CurrentUser currentUser)
        {
            // Validate report name uniqueness
            var existing = await _repository.GetTemplateByNameAsync(dto.ReportName);
            if (existing != null)
                throw new InvalidOperationException($"A report template with name '{dto.ReportName}' already exists.");

            var template = _mapper.Map<ReportTemplate>(dto);
            template.Uuid = Guid.NewGuid().ToString();
            template.CreatedAt = DateTime.Now;
            template.CreatedBy = currentUser.Uuid;
            template.IsActive = dto.IsActive;

            // Link join references for SQL Templates associated with this report
            if (dto.SqlTemplateUuids != null && dto.SqlTemplateUuids.Count > 0)
            {
                foreach (var sqlUuid in dto.SqlTemplateUuids)
                {
                    var sqlTemplate = await _context.SqlTemplates.FirstOrDefaultAsync(st => st.Uuid == sqlUuid && st.IsActive);
                    if (sqlTemplate != null)
                    {
                        template.ReportTemplateSqlTemplates.Add(new ReportTemplateSqlTemplate
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            ReportTemplate = template,
                            SqlTemplate = sqlTemplate
                        });
                    }
                }
            }

            var created = await _repository.AddTemplateAsync(template);
            var result = await _repository.GetTemplateByUuidAsync(created.Uuid);
            return result!;
        }

        /// <summary>
        /// Updates an existing report template.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to update.</param>
        /// <param name="dto">The updated data model payload.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>The updated report template response DTO, or null if template is not found.</returns>
        /// <exception cref="InvalidOperationException">Thrown if updated name conflicts with another template.</exception>
        public async Task<ReportTemplateResDto?> UpdateTemplateAsync(string uuid, ReportTemplateReqDto dto, CurrentUser currentUser)
        {
            var template = await _repository.GetTemplateEntityByUuidAsync(uuid);
            if (template == null) return null;

            // Enforce name uniqueness checks on modification
            if (template.ReportName.ToLower() != dto.ReportName.ToLower())
            {
                var existing = await _repository.GetTemplateByNameAsync(dto.ReportName);
                if (existing != null)
                    throw new InvalidOperationException($"A report template with name '{dto.ReportName}' already exists.");
            }

            _mapper.Map(dto, template);
            template.UpdatedAt = DateTime.Now;
            template.UpdatedBy = currentUser.Uuid;

            // Clear legacy join entries and rebuild bindings
            var existingJoins = await _context.ReportTemplateSqlTemplates
                .Where(rt => rt.ReportTemplateId == template.Id)
                .ToListAsync();
            _context.ReportTemplateSqlTemplates.RemoveRange(existingJoins);

            // Rebuild the bindings to selected SQL templates
            if (dto.SqlTemplateUuids != null && dto.SqlTemplateUuids.Count > 0)
            {
                foreach (var sqlUuid in dto.SqlTemplateUuids)
                {
                    var sqlTemplate = await _context.SqlTemplates.FirstOrDefaultAsync(st => st.Uuid == sqlUuid && st.IsActive);
                    if (sqlTemplate != null)
                    {
                        template.ReportTemplateSqlTemplates.Add(new ReportTemplateSqlTemplate
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            ReportTemplate = template,
                            SqlTemplate = sqlTemplate
                        });
                    }
                }
            }

            var updated = await _repository.UpdateTemplateAsync(template);
            var result = await _repository.GetTemplateByUuidAsync(updated.Uuid);
            return result;
        }

        /// <summary>
        /// Deletes a report template by marking it as inactive.
        /// </summary>
        /// <param name="uuid">The UUID identifier of the template to delete.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>True if deletion succeeded, otherwise false.</returns>
        public async Task<bool> DeleteTemplateAsync(string uuid, CurrentUser currentUser)
        {
            return await _repository.DeleteTemplateAsync(uuid);
        }



        /// <summary>
        /// Normalizes keys by removing spaces, underscores, and converting to lowercase.
        /// </summary>
        /// <param name="key">The raw key string.</param>
        /// <returns>The normalized key string.</returns>
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            return key.Replace("_", "").Replace(" ", "").ToLowerInvariant();
        }

        /// <summary>
        /// Converts PascalCase/CamelCase/Space strings to snake_case.
        /// </summary>
        /// <param name="input">The raw text string.</param>
        /// <returns>The snake_case converted string.</returns>
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var clean = input.Replace(" ", "_").Replace("-", "_");
            var snake = Regex.Replace(clean, @"(?<!^)(?=[A-Z][a-z])|(?<=[a-z])(?=[A-Z])", "_");
            return snake.ToLowerInvariant().Replace("__", "_").Trim('_');
        }

        /// <summary>
        /// Generates the HTML report populated with dynamic query values.
        /// </summary>
        /// <param name="uuid">The unique UUID identifier of the report template.</param>
        /// <param name="parameterValues">Dictionary of user selected parameters mapping placeholder keys to actual values.</param>
        /// <param name="currentUser">The current authenticated user profile.</param>
        /// <returns>A tuple containing the rendered HTML content string and the PDF target filename.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the report template is not found.</exception>
        public async Task<(string HtmlContent, string Filename)> GenerateDynamicReportAsync(string uuid, Dictionary<string, string> parameterValues, CurrentUser currentUser)
        {
            var reportTemplate = await _repository.GetTemplateByUuidAsync(uuid);
            if (reportTemplate == null)
            {
                throw new KeyNotFoundException("Report template not found.");
            }

            var shop = await _shopRepository.GetAsync();
            var placeholders = new Dictionary<string, string>
            {
                { "shop_name", shop?.Name ?? "POS Store" },
                { "shop_address", shop?.Address ?? "Colombo, Sri Lanka" },
                { "shop_phone", shop?.PhoneNumber ?? "N/A" },
                { "report_date", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                { "report_name", reportTemplate.ReportName }
            };

            // Map and parse user input parameter values (such as dates, entities)
            foreach (var kvp in parameterValues)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                // Format raw date inputs to YYYY/MM/DD format for PDF layouts
                string displayValue = value;
                if (Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}"))
                {
                    if (DateTime.TryParse(value, out DateTime parsedParamDate))
                    {
                        displayValue = parsedParamDate.ToString("yyyy/MM/dd");
                    }
                    else
                    {
                        displayValue = value.Replace('-', '/');
                    }
                }

                placeholders[key] = displayValue;
                string snakeKey = Regex.Replace(key, "(?<!^)(?=[A-Z])", "_").ToLowerInvariant();
                placeholders[snakeKey] = displayValue;
                placeholders[key.ToLowerInvariant()] = displayValue;

                // Fetch details of entities like Item, Supplier, User for clearer report representations
                if ((key.Equals("item", StringComparison.OrdinalIgnoreCase) || key.Equals("itemId", StringComparison.OrdinalIgnoreCase)) && int.TryParse(value, out int itemId))
                {
                    var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
                    if (item != null)
                    {
                        placeholders["item_name"] = item.Name;
                        placeholders["item"] = item.Name;
                    }
                }
                else if ((key.Equals("supplier", StringComparison.OrdinalIgnoreCase) || key.Equals("supplierId", StringComparison.OrdinalIgnoreCase)) && int.TryParse(value, out int supplierId))
                {
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId);
                    if (supplier != null)
                    {
                        placeholders["supplier_name"] = supplier.Name;
                        placeholders["supplier"] = supplier.Name;
                    }
                }
                else if (key.Equals("user", StringComparison.OrdinalIgnoreCase) || key.Equals("userId", StringComparison.OrdinalIgnoreCase) || key.Equals("cashier", StringComparison.OrdinalIgnoreCase))
                {
                    var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Uuid == value);
                    if (userEntity != null)
                    {
                        string fullName = $"{userEntity.FirstName} {userEntity.LastName}".Trim();
                        placeholders["user_name"] = fullName;
                        placeholders["user"] = fullName;
                        placeholders["cashier_name"] = fullName;
                        placeholders["cashier"] = fullName;
                    }
                }
            }

            var mappings = reportTemplate.SqlPlaceholderMappings;
            string rendered = reportTemplate.HtmlContent;

            // Execute each SQL template and replace its table/values placeholders
            foreach (var sqlTemplate in reportTemplate.SqlTemplates)
            {
                if (sqlTemplate == null) continue;

                var queryParams = new Dictionary<string, string>();
                var expectedPlaceholders = sqlTemplate.Placeholders;

                // Bind parameters for the SQL execution using mapping configurations
                foreach (var exp in expectedPlaceholders)
                {
                    var userValue = ResolvePlaceholderValue(sqlTemplate, exp.Name, parameterValues, mappings);
                    if (userValue != null)
                    {
                        queryParams[exp.Name] = userValue;
                    }
                }

                // Execute the actual query database retrieval
                var results = await _sqlTemplateService.ExecuteTemplateAsync(sqlTemplate.Uuid, queryParams, currentUser);

                var parametersObj = reportTemplate.Parameters;
                bool isTable = parametersObj.TableSqlTemplateUuids != null && parametersObj.TableSqlTemplateUuids.Contains(sqlTemplate.Uuid);

                if (isTable)
                {
                    // Render HTML table block output for the template placeholder
                    string tableHtml = BuildHtmlTableFromResults(sqlTemplate.TemplateName, results);
                    
                    string name2 = ToSnakeCase(sqlTemplate.TemplateName);
                    
                    rendered = Regex.Replace(rendered, @"\{\{" + sqlTemplate.Id + @"\.table\}\}", tableHtml, RegexOptions.IgnoreCase);
                    rendered = Regex.Replace(rendered, @"\{\{" + sqlTemplate.Id + @"_table\}\}", tableHtml, RegexOptions.IgnoreCase);
                    rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(name2) + @"_table\}\}", tableHtml, RegexOptions.IgnoreCase);

                    // Clear any individual select value placeholders
                    var selectValues = sqlTemplate.SelectValues;
                    foreach (var sv in selectValues)
                    {
                        var cleanSv = ToSnakeCase(sv.Name);
                        rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(sv.Name) + @"\}\}", "", RegexOptions.IgnoreCase);
                        rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(cleanSv) + @"\}\}", "", RegexOptions.IgnoreCase);
                        
                        string namespaceKeyId1 = $"{sqlTemplate.Id}.{sv.Name}";
                        string namespaceKeyId2 = $"{sqlTemplate.Id}.{cleanSv}";
                        rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(namespaceKeyId1) + @"\}\}", "", RegexOptions.IgnoreCase);
                        rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(namespaceKeyId2) + @"\}\}", "", RegexOptions.IgnoreCase);
                    }
                }
                else
                {
                    // Map query results as single column-to-value placeholders from the first row returned
                    if (results != null && results.Count > 0)
                    {
                        var firstRow = results[0];

                        foreach (var kvp in firstRow)
                        {
                            var colName = kvp.Key;
                            var cleanColName = ToSnakeCase(colName);
                            var colVal = kvp.Value == null ? "" : kvp.Value.ToString() ?? "";

                            // Replace direct columns tags
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(colName) + @"\}\}", colVal, RegexOptions.IgnoreCase);
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(cleanColName) + @"\}\}", colVal, RegexOptions.IgnoreCase);

                            // Replace ID namespaced column tags
                            string namespaceKeyId1 = $"{sqlTemplate.Id}.{colName}";
                            string namespaceKeyId2 = $"{sqlTemplate.Id}.{cleanColName}";
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(namespaceKeyId1) + @"\}\}", colVal, RegexOptions.IgnoreCase);
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(namespaceKeyId2) + @"\}\}", colVal, RegexOptions.IgnoreCase);
                        }
                    }
                    else
                    {
                        // Clear placeholders since query results are empty
                        var selectValues = sqlTemplate.SelectValues;

                        foreach (var sv in selectValues)
                        {
                            var cleanSv = ToSnakeCase(sv.Name);
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(sv.Name) + @"\}\}", "", RegexOptions.IgnoreCase);
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(cleanSv) + @"\}\}", "", RegexOptions.IgnoreCase);

                            string namespaceKeyId1 = $"{sqlTemplate.Id}.{sv.Name}";
                            string namespaceKeyId2 = $"{sqlTemplate.Id}.{cleanSv}";
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(namespaceKeyId1) + @"\}\}", "", RegexOptions.IgnoreCase);
                            rendered = Regex.Replace(rendered, @"\{\{" + Regex.Escape(namespaceKeyId2) + @"\}\}", "", RegexOptions.IgnoreCase);
                        }
                    }
                }
            }

            // Interpolate global shop properties and date stamps
            rendered = ProcessPlaceholders(rendered, placeholders);

            return (rendered, $"{reportTemplate.ReportName}.pdf");
        }

        /// <summary>
        /// Formats query result records as a styled HTML table.
        /// </summary>
        /// <param name="tableName">The name of the database template table.</param>
        /// <param name="rows">List of row records dictionary.</param>
        /// <returns>A styled HTML string representation of the table.</returns>
        private string BuildHtmlTableFromResults(string tableName, List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();
            sb.Append("<table class='report-table' style='width:100%; border-collapse: collapse; margin-top: 15px; font-size: 14px;'>");
            
            // Render placeholder cell if no rows exist in query results
            if (rows == null || rows.Count == 0)
            {
                sb.Append("<thead><tr style='background-color: #f3f4f6; text-align: left;'>");
                sb.Append("<th style='padding: 10px; border-bottom: 2px solid #e5e7eb;'>Result</th></tr></thead>");
                sb.Append("<tbody><tr><td style='padding: 15px; text-align: center; color: #6b7280;'>No records found</td></tr></tbody>");
                sb.Append("</table>");
                return sb.ToString();
            }

            var columns = rows[0].Keys.ToList();

            // Detect numeric columns and compute totals
            var numericColumns = new HashSet<string>();
            var columnTotals = new Dictionary<string, decimal>();
            bool hasAnyTotal = false;

            foreach (var col in columns)
            {
                string lowerCol = col.ToLowerInvariant();
                bool isExcludedName = lowerCol.Contains("id") || 
                                      lowerCol.Contains("uuid") || 
                                      lowerCol.Contains("number") || 
                                      lowerCol.Contains("code") || 
                                      lowerCol.Contains("phone") || 
                                      lowerCol.Contains("mobile") || 
                                      lowerCol.Contains("fax") ||
                                      lowerCol.Equals("no") || 
                                      lowerCol.EndsWith("_no") || 
                                      lowerCol.StartsWith("no_") || 
                                      lowerCol.EndsWith("no") || 
                                      lowerCol.Contains("barcode");

                bool isNumeric = false;
                decimal sum = 0;
                bool hasValue = false;

                foreach (var row in rows)
                {
                    if (row.TryGetValue(col, out var val) && val != null)
                    {
                        var valType = val.GetType();
                        var underlyingType = Nullable.GetUnderlyingType(valType) ?? valType;
                        if (IsNumericType(underlyingType))
                        {
                            isNumeric = true;
                            if (!isExcludedName)
                            {
                                try
                                {
                                    sum += Convert.ToDecimal(val);
                                    hasValue = true;
                                }
                                catch
                                {
                                    // ignore if conversion fails
                                }
                            }
                        }
                    }
                }

                if (isNumeric && !isExcludedName && hasValue)
                {
                    numericColumns.Add(col);
                    columnTotals[col] = sum;
                    hasAnyTotal = true;
                }
            }

            // Render table header columns
            sb.Append("<thead><tr style='background-color: #f3f4f6;'>");
            foreach (var col in columns)
            {
                string align = numericColumns.Contains(col) ? "right" : "left";
                sb.Append($"<th style='padding: 10px; border-bottom: 2px solid #e5e7eb; text-align: {align};'>{col}</th>");
            }
            sb.Append("</tr></thead><tbody>");

            // Render table content rows
            foreach (var row in rows)
            {
                sb.Append("<tr>");
                foreach (var col in columns)
                {
                    var val = row.TryGetValue(col, out var v) ? v : null;
                    var displayVal = "";
                    
                    // Format dates safely to YYYY/MM/DD
                    if (val is DateTime dt)
                    {
                        displayVal = dt.ToString("yyyy/MM/dd");
                    }
                    else if (val is DateOnly dOnly)
                    {
                        displayVal = dOnly.ToString("yyyy/MM/dd");
                    }
                    else if (val is DateTimeOffset dto)
                    {
                        displayVal = dto.ToString("yyyy/MM/dd");
                    }
                    else if (val != null)
                    {
                        displayVal = val.ToString() ?? "";
                        // Fallback check: if string resembles ISO date format, normalize it
                        if (Regex.IsMatch(displayVal, @"^\d{4}-\d{2}-\d{2}"))
                        {
                            if (DateTime.TryParse(displayVal, out DateTime parsedDt))
                            {
                                displayVal = parsedDt.ToString("yyyy/MM/dd");
                            }
                        }
                    }
                    
                    string align = numericColumns.Contains(col) ? "right" : "left";
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #e5e7eb; text-align: {align};'>{displayVal}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");

            // Render table footer totals row if applicable
            if (hasAnyTotal)
            {
                sb.Append("<tfoot><tr style='background-color: #f8fafc; font-weight: bold; border-top: 2px solid #cbd5e1; border-bottom: 2px solid #cbd5e1;'>");
                for (int i = 0; i < columns.Count; i++)
                {
                    var col = columns[i];
                    string align = numericColumns.Contains(col) ? "right" : "left";
                    if (i == 0)
                    {
                        sb.Append("<td style='padding: 10px; text-align: left;'><strong>TOTAL</strong></td>");
                    }
                    else if (columnTotals.TryGetValue(col, out var total))
                    {
                        string formattedTotal = total % 1 == 0 ? total.ToString("0") : total.ToString("0.00");
                        sb.Append($"<td style='padding: 10px; text-align: {align};'>{formattedTotal}</td>");
                    }
                    else
                    {
                        sb.Append("<td style='padding: 10px;'></td>");
                    }
                }
                sb.Append("</tr></tfoot>");
            }

            sb.Append("</table>");
            return sb.ToString();
        }

        private static bool IsNumericType(Type type)
        {
            if (type == null) return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Resolves the value of a SQL template placeholder from parameters and mappings using SQL template ID namespacing.
        /// </summary>
        private static string? ResolvePlaceholderValue(
            SqlTemplateResDto sqlTemplate,
            string placeholderName,
            Dictionary<string, string> parameterValues,
            List<SqlPlaceholderMappingDto> mappings)
        {
            string normPH = NormalizeKey(placeholderName);
            int templateId = sqlTemplate.Id;

            // 1. Direct namespaced check in parameterValues, e.g. "12.order_details"
            string idNamespacedKey = $"{templateId}.{placeholderName}";
            if (parameterValues.TryGetValue(idNamespacedKey, out var directValue))
            {
                return directValue;
            }

            // Case-insensitive / normalized direct namespace check
            foreach (var kvp in parameterValues)
            {
                if (string.Equals(kvp.Key, idNamespacedKey, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
                var parts = kvp.Key.Split('.', 2);
                if (parts.Length == 2 && parts[0] == templateId.ToString())
                {
                    if (NormalizeKey(parts[1]) == normPH)
                    {
                        return kvp.Value;
                    }
                }
            }

            // 2. Mappings check
            // We search for a mapping that matches this placeholder namespaced by ID (e.g. "12.order_details")
            // or directly mapped to the placeholder name (un-namespaced legacy fallback).
            var mapping = mappings.FirstOrDefault(m => 
                string.Equals(m.SqlPlaceholder, $"{templateId}.{placeholderName}", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.SqlPlaceholder, placeholderName, StringComparison.OrdinalIgnoreCase));

            if (mapping != null)
            {
                if (parameterValues.TryGetValue(mapping.ReportValue, out var mappedValue))
                {
                    return mappedValue;
                }
                if (parameterValues.TryGetValue(mapping.ReportValue.ToLowerInvariant(), out mappedValue))
                {
                    return mappedValue;
                }
            }

            // 3. Fallback direct match (without namespace)
            if (parameterValues.TryGetValue(placeholderName, out var directVal))
            {
                return directVal;
            }

            foreach (var kvp in parameterValues)
            {
                if (NormalizeKey(kvp.Key) == normPH)
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Replaces double curly-braces placeholders (e.g. {{shop_name}}) inside a template with actual matching values.
        /// </summary>
        /// <param name="template">The source HTML template markup.</param>
        /// <param name="values">Dictionary containing target key-value parameters.</param>
        /// <returns>The interpolated HTML markup string.</returns>
        private string ProcessPlaceholders(string template, Dictionary<string, string> values)
        {
            string result = template;
            foreach (var kvp in values)
            {
                result = Regex.Replace(result, @"\{\{" + Regex.Escape(kvp.Key) + @"\}\}", kvp.Value ?? "", RegexOptions.IgnoreCase);
            }
            return result;
        }
    }
}
