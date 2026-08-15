using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using pos_service.Data;

namespace pos_service.Repositories
{
    /// <summary>
    /// Base class for repositories providing shared utility code, stored procedure execution,
    /// SQL query helpers, parameter factories, and ADO.NET connection management.
    /// </summary>
    public abstract class BaseRepository
    {
        protected readonly AppDbContext _context;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns text, or null if it's blank or whitespace.
        /// </summary>
        protected static string? TextOrNull(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return text;
        }

        /// <summary>
        /// Returns the given keyUuid, or a new UUID string if the given keyUuid is null or empty.
        /// </summary>
        protected static string MakeUuidIfNull(string? keyUuid)
        {
            if (string.IsNullOrEmpty(keyUuid))
            {
                return Guid.NewGuid().ToString();
            }

            return keyUuid;
        }

        /// <summary>
        /// Returns a fresh GUID string formatted as xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.
        /// </summary>
        protected static string NewGuidString()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Converts a string value to Guid.
        /// </summary>
        protected static Guid ToGuid(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("String is null or empty to convert to Guid.");

            return new Guid(value);
        }

        /// <summary>
        /// Safely loads records from a list of IDs in chunks, working around query length limits.
        /// </summary>
        protected static List<T> ChunkyLoad<TId, T>(IEnumerable<TId> idList, Func<List<TId>, List<T>> load, int? maxPerLoad = null)
        {
            const int DefaultChunkSize = 1000;

            List<TId> ids = idList?.ToList() ?? new List<TId>();
            if (ids.Count == 0)
                return new List<T>();

            List<T> loaded = new List<T>();
            int chunkSize = Math.Max(1, maxPerLoad ?? DefaultChunkSize);

            for (int i = 0; i <= (ids.Count / chunkSize); i++)
            {
                List<TId> chunk = ids.Skip(i * chunkSize).Take(chunkSize).ToList();
                if (chunk.Count == 0)
                    break;

                List<T> result = load(chunk);
                loaded.AddRange(result);
            }

            return loaded;
        }

        /// <summary>
        /// Returns true if parameter collection has any output or input/output parameter.
        /// </summary>
        protected static bool HasOutParameter(DbParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return false;
            return parameters.Any(x => x.Direction == ParameterDirection.Output || x.Direction == ParameterDirection.InputOutput);
        }

        /// <summary>
        /// Returns the value found in the parameter, or default value for T if null.
        /// </summary>
        protected static T? GetParameterValue<T>(DbParameter param)
        {
            if (param == null || param.Value == DBNull.Value) return default;
            return (T)param.Value;
        }



        /// <summary>
        /// DbParameter factory for safely creating MySqlParameter instances.
        /// </summary>
        protected static MySqlParameter CreateParam(
            string paramName,
            object? value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var param = new MySqlParameter
            {
                ParameterName = paramName,
                Value = value ?? DBNull.Value,
                Direction = direction
            };
            if (dbType.HasValue)
            {
                param.DbType = dbType.Value;
            }
            return param;
        }

        /// <summary>
        /// DbParameter factory overload for CreateParameter matching standard DbContext signature.
        /// </summary>
        protected static DbParameter CreateParameter(AppDbContext? db, string paramName, object? value)
        {
            return CreateParam(paramName, value);
        }

        /// <summary>
        /// DbParameter factory overload for CreateParameter.
        /// </summary>
        protected static DbParameter CreateParameter(string paramName, object? value)
        {
            return CreateParam(paramName, value);
        }

        /// <summary>
        /// Helper function to check if a specific column exists in the DbDataReader result.
        /// </summary>
        protected static bool HasColumn(DbDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // -------------------------- Async ADO.NET Connection Management --------------------------------

        /// <summary>
        /// Executes a stored procedure asynchronously on this database, returning a List of T.
        /// </summary>
        protected async Task<List<T>> ExecStoredProcListAsync<T>(
            string spName,
            Func<DbDataReader, T> map,
            params DbParameter[] parameters)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = spName;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        var list = new List<T>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                list.Add(map(reader));
                            }
                        }

                        return list;
                    }
                }
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Executes a stored procedure asynchronously on this database, returning a single mapped object T or default.
        /// </summary>
        protected async Task<T?> ExecStoredProcSingleAsync<T>(
            string spName,
            Func<DbDataReader, T> map,
            params DbParameter[] parameters)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = spName;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return map(reader);
                            }
                        }

                        return default;
                    }
                }
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Executes a stored procedure asynchronously and automatically maps results to a List of T using reflection.
        /// </summary>
        protected async Task<List<T>> ExecuteStoredProcedureAsync<T>(
            string spName,
            params DbParameter[] parameters) where T : class, new()
        {
            return await ExecStoredProcListAsync(spName, MapReaderToEntity<T>, parameters);
        }

        /// <summary>
        /// Overload accepting DbContext parameter for ExecuteStoredProcedureAsync.
        /// </summary>
        protected async Task<List<T>> ExecuteStoredProcedureAsync<T>(
            AppDbContext context,
            string spName,
            params DbParameter[] parameters) where T : class, new()
        {
            return await ExecStoredProcListAsync(spName, MapReaderToEntity<T>, parameters);
        }

        /// <summary>
        /// Executes a stored procedure asynchronously and automatically maps the first result row to T using reflection.
        /// </summary>
        protected async Task<T?> ExecuteStoredProcedureSingleAsync<T>(
            string spName,
            params DbParameter[] parameters) where T : class, new()
        {
            return await ExecStoredProcSingleAsync(spName, MapReaderToEntity<T>, parameters);
        }

        /// <summary>
        /// Overload accepting DbContext parameter for ExecuteStoredProcedureSingleAsync.
        /// </summary>
        protected async Task<T?> ExecuteStoredProcedureSingleAsync<T>(
            AppDbContext context,
            string spName,
            params DbParameter[] parameters) where T : class, new()
        {
            return await ExecStoredProcSingleAsync(spName, MapReaderToEntity<T>, parameters);
        }

        /// <summary>
        /// Maps the current row of a DbDataReader to an entity of type T using reflection.
        /// </summary>
        private static T MapReaderToEntity<T>(DbDataReader reader) where T : class, new()
        {
            var entity = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                if (reader.IsDBNull(i)) continue;

                var value = reader.GetValue(i);

                // 1. Exact match (case-insensitive)
                var prop = properties.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

                // 2. Normalize snake_case comparison if direct match not found
                if (prop == null)
                {
                    var normalizedCol = columnName.Replace("_", "");
                    prop = properties.FirstOrDefault(p => string.Equals(p.Name.Replace("_", ""), normalizedCol, StringComparison.OrdinalIgnoreCase));
                }

                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        if (targetType.IsEnum)
                        {
                            if (value is string s)
                                prop.SetValue(entity, Enum.Parse(targetType, s, true));
                            else
                                prop.SetValue(entity, Enum.ToObject(targetType, Convert.ChangeType(value, Enum.GetUnderlyingType(targetType))));
                        }
                        else if (targetType == typeof(Guid))
                        {
                            if (value is Guid g) prop.SetValue(entity, g);
                            else if (value is string strGuid && Guid.TryParse(strGuid, out var parsedGuid)) prop.SetValue(entity, parsedGuid);
                            else if (value is byte[] bytes && bytes.Length == 16) prop.SetValue(entity, new Guid(bytes));
                        }
                        else if (targetType == typeof(bool))
                        {
                            if (value is bool b) prop.SetValue(entity, b);
                            else prop.SetValue(entity, Convert.ToInt64(value) != 0);
                        }
                        else
                        {
                            var convertedValue = Convert.ChangeType(value, targetType);
                            prop.SetValue(entity, convertedValue);
                        }
                    }
                    catch
                    {
                        // Ignore conversion failure for mismatched non-critical column
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Dynamically queries a database view (view_*) asynchronously with optional WHERE parameter clause.
        /// </summary>
        protected async Task<List<T>> QueryViewListAsync<T>(
            string viewName,
            Func<DbDataReader, T> map,
            string? whereClause = null,
            params DbParameter[] parameters)
        {
            try
            {
                string sqlQuery = string.IsNullOrWhiteSpace(whereClause)
                    ? $"SELECT * FROM `{viewName}`"
                    : $"SELECT * FROM `{viewName}` WHERE {whereClause}";

                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        var list = new List<T>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                list.Add(map(reader));
                            }
                        }

                        return list;
                    }
                }
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Executes an SQL query asynchronously, returning the results in a DataTable.
        /// </summary>
        protected async Task<DataTable?> ExecSqlQueryAsync(string sqlQuery, params DbParameter[] parameters)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        var dt = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        return dt;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Executes SQL query asynchronously and returns the scalar result.
        /// </summary>
        protected async Task<object?> ExecSqlScalarAsync(string sqlQuery, params DbParameter[] parameters)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        return await cmd.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
