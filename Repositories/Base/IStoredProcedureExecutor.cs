using System.Data;

namespace pos_service.Repositories.Base
{
    /// <summary>
    /// Interface for executing stored procedures with parameterized queries.
    /// Provides methods for executing SPs and mapping results to entities.
    /// </summary>
    public interface IStoredProcedureExecutor
    {
        /// <summary>
        /// Executes a stored procedure and returns a list of entities.
        /// </summary>
        /// <typeparam name="T">The entity type to map results to.</typeparam>
        /// <param name="procedureName">The name of the stored procedure.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>List of mapped entities.</returns>
        Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, Dictionary<string, object?> parameters) where T : class, new();

        /// <summary>
        /// Executes a stored procedure and returns a single entity.
        /// </summary>
        /// <typeparam name="T">The entity type to map result to.</typeparam>
        /// <param name="procedureName">The name of the stored procedure.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>Single mapped entity or null.</returns>
        Task<T?> ExecuteStoredProcedureSingleAsync<T>(string procedureName, Dictionary<string, object?> parameters) where T : class, new();

        /// <summary>
        /// Executes a stored procedure without returning results (for INSERT, UPDATE, DELETE operations).
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, Dictionary<string, object?> parameters);
    }
}
