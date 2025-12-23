using pos_service.Models;
using pos_service.Models.DTO.Items;

namespace pos_service.Services
{
    public interface IItemService
    {
        /// <summary>
        /// Retrieves a specific item by its composite identifier (ID and SubID).
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>The item details if found, otherwise null.</returns>
        Task<ItemResDto?> GetItemByIdAsync(int id, int subId, CurrentUser currentUser);

        /// <summary>
        /// Retrieves all items from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of all item details.</returns>
        Task<IEnumerable<ItemResDto>> GetAllItemsAsync(CurrentUser currentUser);

        /// <summary>
        /// Creates a new item in the system.
        /// </summary>
        /// <param name="itemDto">The item data transfer object containing item information.</param>
        /// <param name="currentUser">The current user creating the item.</param>
        /// <returns>The newly created item details if successful, otherwise null.</returns>
        Task<ItemResDto?> CreateItemAsync(ItemReqDto itemDto, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to update.</param>
        /// <param name="subId">The sub-identifier of the item to update.</param>
        /// <param name="itemDto">The item data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user updating the item.</param>
        /// <returns>True if update was successful, otherwise false.</returns>
        Task<bool> UpdateItemAsync(int id, int subId, ItemReqDto itemDto, CurrentUser currentUser);

        /// <summary>
        /// Deletes an item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to delete.</param>
        /// <param name="subId">The sub-identifier of the item to delete.</param>
        /// <param name="currentUser">The current user deleting the item.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteItemAsync(int id, int subId, CurrentUser currentUser);

        /// <summary>
        /// Adds stock quantity to an existing item.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="quantity">The quantity to add to the item's stock.</param>
        /// <param name="currentUser">The current user adding stock.</param>
        /// <returns>The updated item details if successful, otherwise null.</returns>
        Task<ItemResDto?> AddStockAsync(int id, int subId, decimal quantity, CurrentUser currentUser);

        /// <summary>
        /// Retrieves all items that share the same main identifier.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of items with the specified main ID.</returns>
        Task<IEnumerable<ItemResDto>> GetItemsByMainIdAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Retrieves minimal item details by barcode for quick lookups.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>Minimal item details if found, otherwise empty collection.</returns>
        Task<IEnumerable<ItemMiniResDto>> GetItemMinDetailsByBarCodeAsync(string barCode, CurrentUser currentUser);

        /// <summary>
        /// Retrieves complete item details by barcode.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>Complete item details if found, otherwise empty collection.</returns>
        Task<IEnumerable<ItemResDto>> GetItemByBarCodeAsync(string barCode, CurrentUser currentUser);

        /// <summary>
        /// Retrieves an item by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the item to retrieve.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>The item details if found, otherwise null.</returns>
        Task<ItemResDto?> GetItemByUuidAsync(string uuid, CurrentUser currentUser);

        /// <summary>
        /// Retrieves quantity information for all items with the specified main ID.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <param name="currentUser">The current user requesting the quantities.</param>
        /// <returns>A dictionary containing quantity information for the items.</returns>
        Task<Dictionary<string, decimal>> GetQuantitiesByMainIdAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Retrieves the current quantity of an item by its UUID.
        /// </summary>
        /// <param name="uuid">The UUID of the item.</param>
        /// <param name="currentUser">The current user requesting the quantity.</param>
        /// <returns>The quantity value if found, otherwise null.</returns>
        Task<decimal?> GetQuantityByUuidAsync(string uuid, CurrentUser currentUser);

        /// <summary>
        /// Retrieves the current quantity of an item by its composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="currentUser">The current user requesting the quantity.</param>
        /// <returns>The quantity value if found, otherwise null.</returns>
        Task<decimal?> GetQuantityByIdAsync(int id, int subId, CurrentUser currentUser);

        /// <summary>
        /// Gets all items associated with a given supplier ID.
        /// </summary>
        /// <param name="supplierId">The unique identifier of the supplier.</param>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of items associated with the specified supplier.</returns>
        Task<IEnumerable<ItemResDto>> GetItemsBySupplierIdAsync(int supplierId, CurrentUser currentUser);

        /// <summary>
        /// Search items by term matching name, print name, barcode or uuid.
        /// </summary>
        Task<IEnumerable<ItemResDto>> SearchItemsAsync(string searchTerm, CurrentUser currentUser);
    }
}