using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using pos_service.Data;
using System.Data;
using System.Reflection;

namespace pos_service.Repositories.Base
{
    /// <summary>
    /// Implementation of stored procedure executor for MySQL/MariaDB.
    /// Handles execution of stored procedures and mapping results to entities.
    /// </summary>
    public class StoredProcedureExecutor : IStoredProcedureExecutor
    {
        private readonly AppDbContext _context;

        public StoredProcedureExecutor(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Executes a stored procedure and returns a list of entities.
        /// </summary>
        public async Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, Dictionary<string, object?> parameters) where T : class, new()
        {
            var results = new List<T>();

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;

            // Add parameters
            AddParameters(command, parameters);

            using var reader = await command.ExecuteReaderAsync();
            
            // Map results to entities
            while (await reader.ReadAsync())
            {
                var entity = MapToEntity<T>(reader);
                results.Add(entity);
            }

            return results;
        }

        /// <summary>
        /// Executes a stored procedure and returns a single entity.
        /// </summary>
        public async Task<T?> ExecuteStoredProcedureSingleAsync<T>(string procedureName, Dictionary<string, object?> parameters) where T : class, new()
        {
            var results = await ExecuteStoredProcedureAsync<T>(procedureName, parameters);
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Executes a stored procedure without returning results.
        /// </summary>
        public async Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, Dictionary<string, object?> parameters)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;

            // Add parameters
            AddParameters(command, parameters);

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Adds parameters to the command.
        /// </summary>
        private void AddParameters(IDbCommand command, Dictionary<string, object?> parameters)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key.StartsWith("@") || param.Key.StartsWith("p_") 
                    ? param.Key 
                    : $"p_{param.Key}";
                parameter.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Maps a data reader row to an entity.
        /// Uses reflection to map column names to property names (case-insensitive).
        /// </summary>
        private T MapToEntity<T>(IDataReader reader) where T : class, new()
        {
            var entity = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                // Find matching property (case-insensitive)
                var property = properties.FirstOrDefault(p => 
                    string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && property.CanWrite)
                {
                    try
                    {
                        // Handle enum conversion
                        if (property.PropertyType.IsEnum && value != null)
                        {
                            var enumValue = Enum.Parse(property.PropertyType, value.ToString()!);
                            property.SetValue(entity, enumValue);
                        }
                        // Handle nullable types
                        else if (Nullable.GetUnderlyingType(property.PropertyType) != null && value == null)
                        {
                            property.SetValue(entity, null);
                        }
                        // Handle type conversion
                        else if (value != null)
                        {
                            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            var convertedValue = Convert.ChangeType(value, targetType);
                            property.SetValue(entity, convertedValue);
                        }
                        else
                        {
                            property.SetValue(entity, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log warning but continue mapping other properties
                        Console.WriteLine($"Warning: Could not map column '{columnName}' to property '{property.Name}': {ex.Message}");
                    }
                }
            }

            return entity;
        }
    }
}
