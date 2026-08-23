using pos_service.Models;
using pos_service.Models.DTO.Items;

namespace pos_service.Repositories
{
    public interface IItemRepository
    {
        /// <summary>
        /// Retrieves a specific item by its composite identifier (ID and SubID).
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>The item response DTO if found, otherwise null.</returns>
        Task<ItemResDto?> GetByIdAsync(int id, int subId);

        /// <summary>
        /// Retrieves all items from the data store projected as response DTOs.
        /// </summary>
        /// <returns>A list of item response DTOs.</returns>
        Task<IEnumerable<ItemResDto>> GetAllAsync();

        /// <summary>
        /// Adds a new item and its inventory inside a repository database transaction.
        /// </summary>
        Task SaveNewItemWithInventoryAsync(Item item);

        /// <summary>
        /// Updates an item and optional inventory record inside a repository database transaction.
        /// </summary>
        Task SaveUpdatedItemWithInventoryAsync(Item itemToUpdate);

        /// <summary>
        /// Adds a new item to the data store.
        /// </summary>
        /// <param name="item">The item entity to add.</param>
        /// <returns>The added item entity with updated identifiers.</returns>
        Task<Item> AddAsync(Item item);

        /// <summary>
        /// Updates an existing item in the data store.
        /// </summary>
        /// <param name="item">The item entity with updated information.</param>
        /// <returns>The updated item entity.</returns>
        Task<Item> UpdateAsync(Item item);

        /// <summary>
        /// Deletes an item from the data store by its composite identifier.
        /// If the item is not found, returns an error message string; otherwise returns null.
        /// </summary>
        /// <param name="id">The main identifier of the item to delete.</param>
        /// <param name="subId">The sub-identifier of the item to delete.</param>
        Task<string?> DeleteAsync(int id, int subId);

        /// <summary>
        /// Checks if an item with the specified composite identifier exists.
        /// </summary>
        /// <param name="id">The main identifier to check.</param>
        /// <param name="subId">The sub-identifier to check.</param>
        /// <returns>True if the item exists, otherwise false.</returns>
        Task<bool> ItemExistsAsync(int id, int subId);

        /// <summary>
        /// Gets an item by its composite key and includes its related Suppliers.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>The item entity with supplier information if found, otherwise null.</returns>
        Task<Item?> GetByIdWithSuppliersAsync(int id, int subId);

        /// <summary>
        /// Gets all item variants under a single main ID projected as response DTOs.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <returns>A list of item response DTOs sharing the same main ID.</returns>
        Task<IEnumerable<ItemResDto>> GetByMainIdAsync(int id);

        /// <summary>
        /// Gets items by their barcode projected as response DTOs.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <returns>A list of item response DTOs matching the barcode.</returns>
        Task<IEnumerable<ItemResDto>> GetByBarCodeAsync(string barCode);

        /// <summary>
        /// Gets a single item by its unique Guid (Uuid).
        /// </summary>
        /// <param name="uuid">The UUID of the item to retrieve.</param>
        /// <returns>The item entity if found, otherwise null.</returns>
        Task<Item?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Gets a single item response DTO by its unique Guid (Uuid).
        /// </summary>
        /// <param name="uuid">The UUID of the item to retrieve.</param>
        /// <returns>The item response DTO if found, otherwise null.</returns>
        Task<ItemResDto?> GetResDtoByUuidAsync(string uuid);

        /// <summary>
        /// Gets multiple items by their unique Guids (Uuids).
        /// </summary>
        /// <param name="uuids">The collection of UUIDs to retrieve.</param>
        /// <returns>A list of item entities matching the provided UUIDs.</returns>
        Task<IEnumerable<Item>> GetByUuidsAsync(IEnumerable<string> uuids);

        /// <summary>
        /// Gets all items that are supplied by the specified supplier ID.
        /// </summary>
        /// <param name="supplierId">The unique identifier of the supplier.</param>
        /// <returns>A list of item entities associated with the specified supplier.</returns>
        Task<IEnumerable<Item>> GetBySupplierIdAsync(int supplierId);

        /// <summary>
        /// Searches items by a term matching name, print name, barcode or uuid.
        /// If searchTerm is null or empty returns all items.
        /// </summary>
        Task<IEnumerable<ItemResDto>> GetBySearchAsync(string searchTerm);

        /// <summary>
        /// Returns the next available main Id to use when creating a new item.
        /// Typically returns max(Id) + 1 or 1 if no items exist.
        /// </summary>
        Task<int> GetNextMainIdAsync();

        /// <summary>
        /// Returns the next available SubId for the given main Id.
        /// Typically returns max(SubId) + 1 or 0 if no variants exist.
        /// </summary>
        /// <param name="mainId">The main identifier of the item.</param>
        /// <returns>The next available sub-identifier integer.</returns>
        Task<int> GetNextSubIdAsync(int mainId);

        /// <summary>
        /// Permanently deletes a collection of item expiry records by their unique identifiers.
        /// </summary>
        /// <param name="expiryUuids">The collection of expiry UUIDs to permanently delete.</param>
        /// <returns>The number of expiry records deleted.</returns>
        Task<int> DeleteExpiriesAsync(IEnumerable<string> expiryUuids);
    }
}