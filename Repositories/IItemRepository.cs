using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IItemRepository
    {
          Task<Item?> GetByIdAsync(int id, int subId);
          Task<IEnumerable<Item>> GetAllAsync();
          Task<Item> AddAsync(Item item);
          Task<Item> UpdateAsync(Item item);
          Task DeleteAsync(Item item);
          Task<bool> ItemExistsAsync(int id, int subId);

          /// <summary>
          /// Gets an item by its composite key and includes its related Suppliers.
          /// </summary>
          Task<Item?> GetByIdWithSuppliersAsync(int id, int subId);

          /// <summary>
          /// Gets all item variants under a single main ID.
          /// </summary>
          Task<IEnumerable<Item>> GetByMainIdAsync(int id);

          /// <summary>
          /// Gets a single item by its barcode.
          /// </summary>
          Task<IEnumerable<Item>> GetByBarCodeAsync(string barCode);

          /// <summary>
          /// Gets a single item by its unique Guid (Uuid).
          /// </summary>
          Task<Item?> GetByUuidAsync(Guid uuid);

        /// <summary>
        /// Gets all items that are supplied by the specified supplier ID.
        /// </summary>
        Task<IEnumerable<Item>> GetBySupplierIdAsync(int supplierId);
    }
}
