using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace pos_service.Data.Utilities
{
    /// <summary>
    /// Abstract base class for repositories providing common stored procedure and SQL query execution patterns.
    /// Derived repositories inherit database operation capabilities for consistent data access patterns.
    /// Repositories are responsible for providing DbContext when calling these methods.
    /// </summary>
    public abstract class BaseOperations
    {
        protected readonly ILogger<BaseOperations>? _logger;

        /// <summary>
        /// Protected constructor - repositories must inherit from this class.
        /// Logger is injected via DI and used only for error logging.
        /// </summary>
        protected BaseOperations(ILogger<BaseOperations>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes a stored procedure and returns results mapped to a list of entities of type T.
        /// </summary>
        /// <typeparam name="T">The entity type to map results to</typeparam>
        /// <param name="db">The DbContext instance</param>
        /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
        /// <param name="parameters">DbParameter array for the stored procedure</param>
        /// <returns>List of mapped entities or empty list if no results</returns>
        protected async Task<List<T>> ExecuteStoredProcedureAsync<T>(
            AppDbContext db,
            string storedProcedureName,
            params DbParameter[] parameters) where T : class, new()
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty", nameof(storedProcedureName));

            try
            {
                string sql = BuildCallStatement(storedProcedureName, parameters);
                var dataTable = await ExecuteSqlQueryInternalAsync(db, sql, parameters);
                return MapDataTableToEntities<T>(dataTable);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error executing stored procedure '{storedProcedureName}'");
                throw;
            }
        }

        /// <summary>
        /// Executes a stored procedure and returns the first result mapped to an entity of type T.
        /// </summary>
        /// <typeparam name="T">The entity type to map result to</typeparam>
        /// <param name="db">The DbContext instance</param>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="parameters">DbParameter array</param>
        /// <returns>First mapped entity or null if no results</returns>
        protected async Task<T?> ExecuteStoredProcedureFirstAsync<T>(
            AppDbContext db,
            string storedProcedureName,
            params DbParameter[] parameters) where T : class, new()
        {
            var results = await ExecuteStoredProcedureAsync<T>(db, storedProcedureName, parameters);
            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Executes a stored procedure and returns the first column values as a list of type T.
        /// </summary>
        /// <typeparam name="T">The type to convert column values to</typeparam>
        /// <param name="db">The DbContext instance</param>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="parameters">DbParameter array</param>
        /// <returns>List of first column values</returns>
        protected async Task<List<T>> ExecuteStoredProcedureFirstColumnAsync<T>(
            AppDbContext db,
            string storedProcedureName,
            params DbParameter[] parameters)
        {
            var result = new List<T>();

            try
            {
                string sql = BuildCallStatement(storedProcedureName, parameters);
                var dataTable = await ExecuteSqlQueryInternalAsync(db, sql, parameters);

                if (dataTable == null || dataTable.Rows.Count == 0)
                    return result;

                var firstColumn = dataTable.Columns[0].ColumnName;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row[firstColumn] != DBNull.Value)
                    {
                        try
                        {
                            var value = (T)Convert.ChangeType(row[firstColumn], typeof(T));
                            result.Add(value);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, $"Error converting value to type {typeof(T).Name}");
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error executing stored procedure '{storedProcedureName}'");
                throw;
            }
        }

        /// <summary>
        /// Executes a stored procedure and returns a DataTable with raw results.
        /// </summary>
        /// <param name="db">The DbContext instance</param>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="parameters">DbParameter array</param>
        /// <returns>DataTable containing query results</returns>
        protected async Task<DataTable> ExecuteStoredProcedureDataTableAsync(
            AppDbContext db,
            string storedProcedureName,
            params DbParameter[] parameters)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty", nameof(storedProcedureName));

            try
            {
                string sql = BuildCallStatement(storedProcedureName, parameters);
                return await ExecuteSqlQueryInternalAsync(db, sql, parameters);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error executing stored procedure '{storedProcedureName}'");
                throw;
            }
        }

        /// <summary>
        /// Executes a raw SQL query and returns results mapped to a list of entities of type T.
        /// </summary>
        /// <typeparam name="T">The entity type to map results to</typeparam>
        /// <param name="db">The DbContext instance</param>
        /// <param name="sqlQuery">The SQL query to execute</param>
        /// <param name="parameters">DbParameter array</param>
        /// <returns>List of mapped entities</returns>
        protected async Task<List<T>> ExecuteSqlQueryAsync<T>(
            AppDbContext db,
            string sqlQuery,
            params DbParameter[] parameters) where T : class, new()
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentException("SQL query cannot be null or empty", nameof(sqlQuery));

            try
            {
                var dataTable = await ExecuteSqlQueryInternalAsync(db, sqlQuery, parameters);
                return MapDataTableToEntities<T>(dataTable);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error executing SQL query");
                throw;
            }
        }

        /// <summary>
        /// Executes a raw SQL query and returns a DataTable with results.
        /// </summary>
        /// <param name="db">The DbContext instance</param>
        /// <param name="sqlQuery">The SQL query to execute</param>
        /// <param name="parameters">DbParameter array</param>
        /// <returns>DataTable containing query results</returns>
        protected async Task<DataTable> ExecuteSqlQueryDataTableAsync(
            AppDbContext db,
            string sqlQuery,
            params DbParameter[] parameters)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            if (string.IsNullOrWhiteSpace(sqlQuery))
                throw new ArgumentException("SQL query cannot be null or empty", nameof(sqlQuery));

            try
            {
                return await ExecuteSqlQueryInternalAsync(db, sqlQuery, parameters);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error executing SQL query");
                throw;
            }
        }

        /// <summary>
        /// Internal implementation of SQL query execution.
        /// </summary>
        private async Task<DataTable> ExecuteSqlQueryInternalAsync(
            AppDbContext db,
            string sql,
            params DbParameter[] parameters)
        {
            var connection = db.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText    = sql;
                    command.CommandType    = CommandType.Text;
                    command.CommandTimeout = 300; // 5 minutes timeout

                    // Add parameters
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }

                    var dataTable = new DataTable();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }

                    return dataTable;
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        /// <summary>
        /// Maps a DataTable to a list of entities of type T.
        /// Uses reflection to match column names to property names (case-insensitive).
        /// </summary>
        private List<T> MapDataTableToEntities<T>(DataTable dataTable) where T : class, new()
        {
            var result = new List<T>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return result;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            foreach (DataRow row in dataTable.Rows)
            {
                var entity = new T();

                foreach (var property in properties)
                {
                    // Check if column exists in DataTable (case-insensitive)
                    var column = dataTable.Columns.Cast<DataColumn>()
                        .FirstOrDefault(c => string.Equals(c.ColumnName, property.Name, StringComparison.OrdinalIgnoreCase));

                    if (column == null)
                        continue;

                    var value = row[column];

                    // Skip null values
                    if (value == DBNull.Value)
                        continue;

                    try
                    {
                        // Handle nullable types
                        var targetType = property.PropertyType;
                        if (targetType.IsGenericType && 
                            targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            targetType = Nullable.GetUnderlyingType(targetType);
                        }

                        var convertedValue = Convert.ChangeType(value, targetType);
                        property.SetValue(entity, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Error mapping column '{column.ColumnName}' to property '{property.Name}'");
                    }
                }

                result.Add(entity);
            }

            return result;
        }

        /// <summary>
        /// Builds a CALL statement for stored procedure execution.
        /// </summary>
        private string BuildCallStatement(string storedProcedureName, DbParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return $"CALL {storedProcedureName}()";

            var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
            return $"CALL {storedProcedureName}({parameterNames})";
        }

        /// <summary>
        /// Helper method to create database-agnostic DbParameter.
        /// </summary>
        protected DbParameter CreateParameter(AppDbContext db, string name, object value)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            var connection          = db.Database.GetDbConnection();
            var parameter           = connection.CreateCommand().CreateParameter();
            parameter.ParameterName = name;
            parameter.Value         = value ?? DBNull.Value;
            return parameter;
        }

        /// <summary>
        /// Returns text, or null if it's blank.
        /// </summary>
        protected string TextOrNull(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return text;
        }

        /// <summary>
        /// Returns a new GUID string.
        /// </summary>
        protected static string NewGuidStr()
        {
            return Guid.NewGuid().ToString();
        }

        #region Logging

        /// <summary>
        /// Log error message only
        /// </summary>
        protected void LogError(Exception ex, string message)
        {
            _logger?.LogError(ex, message);
        }

        #endregion
    }
}

