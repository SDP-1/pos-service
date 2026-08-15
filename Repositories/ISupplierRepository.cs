using pos_service.Models;
using pos_service.Models.DTO.Suppliers;

namespace pos_service.Repositories
{
    public interface ISupplierRepository
    {
        /// <summary>
        /// Retrieves a specific supplier by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>The supplier response DTO if found, otherwise null.</returns>
        Task<SupplierResDto?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a supplier entity by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>The supplier entity if found, otherwise null.</returns>
        Task<Supplier?> GetSupplierByIdAsync(int id);

        /// <summary>
        /// Retrieves a supplier by its unique identifier including related data such as contacts and items.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>The supplier response DTO with related data if found, otherwise null.</returns>
        Task<SupplierResDto?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Retrieves a supplier and its items by supplier id.
        /// </summary>
        Task<SupplierResDto?> GetSupplierWithItemsAsync(int id);

        /// <summary>
        /// Retrieves a supplier entity by its Name.
        /// Used to enforce unique supplier names.
        /// </summary>
        Task<SupplierResDto?> GetByNameAsync(string name);

        /// <summary>
        /// Retrieves all suppliers from the data store.
        /// </summary>
        /// <returns>A list of all supplier response DTOs.</returns>
        Task<IEnumerable<SupplierResDto>> GetAllAsync();

        /// <summary>
        /// Retrieves all suppliers without loading related navigation properties.
        /// Use this for lightweight queries (e.g. dropdowns) to avoid eager-loading contacts and items.
        /// </summary>
        Task<IEnumerable<SupplierResDto>> GetAllBasicAsync();

        /// <summary>
        /// Adds a new supplier with contacts and item associations inside a repository transaction.
        /// </summary>
        Task<Supplier> SaveNewSupplierAsync(Supplier supplier, IEnumerable<ItemSupplier> itemSuppliers);

        /// <summary>
        /// Updates an existing supplier inside a repository transaction.
        /// </summary>
        Task SaveUpdatedSupplierAsync(Supplier supplier);

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

        /// <summary>
        /// Deletes all item associations for a supplier using a set-based DB operation.
        /// </summary>
        Task DeleteItemAssociationsBySupplierId(int supplierId);

        /// <summary>
        /// Adds multiple item associations in a single operation.
        /// </summary>
        /// <param name="itemSuppliers">The collection of item supplier entities to add.</param>
        Task AddItemAssociationsAsync(IEnumerable<ItemSupplier> itemSuppliers);
    }
}