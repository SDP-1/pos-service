using pos_service.Models;

namespace pos_service.Repositories
{
    public interface ISupplierRepository
    {
        /// <summary>
        /// Retrieves a specific supplier by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>The supplier entity if found, otherwise null.</returns>
        Task<Supplier?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a supplier by its unique identifier including related data.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>The supplier entity with related data if found, otherwise null.</returns>
        Task<Supplier?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Retrieves all suppliers from the data store.
        /// </summary>
        /// <returns>A list of all supplier entities.</returns>
        Task<IEnumerable<Supplier>> GetAllAsync();

        /// <summary>
        /// Adds a new supplier to the data store.
        /// </summary>
        /// <param name="supplier">The supplier entity to add.</param>
        /// <returns>The added supplier entity with updated identifiers.</returns>
        Task<Supplier> AddAsync(Supplier supplier);

        /// <summary>
        /// Updates an existing supplier in the data store.
        /// </summary>
        /// <param name="supplier">The supplier entity with updated information.</param>
        /// <returns>The updated supplier entity.</returns>
        Task<Supplier> UpdateAsync(Supplier supplier);

        /// <summary>
        /// Deletes a supplier from the data store.
        /// </summary>
        /// <param name="supplier">The supplier entity to delete.</param>
        Task DeleteAsync(Supplier supplier);
    }
}