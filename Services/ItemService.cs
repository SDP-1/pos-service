using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Item;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository     _itemRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper             _mapper;

        /// <summary>
        /// Initializes a new instance of the ItemService.
        /// </summary>
        public ItemService(
            IItemRepository itemRepository,
            ISupplierRepository supplierRepository,
            IMapper mapper)
        {
            _itemRepository     = itemRepository;
            _supplierRepository = supplierRepository;
            _mapper             = mapper;
        }

        /// <summary>
        /// Retrieves all items from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of all item details.</returns>
        public async Task<IEnumerable<ItemResDto>> GetAllItemsAsync(CurrentUser currentUser)
        {
            var items = await _itemRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        /// <summary>
        /// Retrieves a specific item by its composite identifier (ID and SubID).
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>The item details if found, otherwise null.</returns>
        public async Task<ItemResDto?> GetItemByIdAsync(int id, int subId, CurrentUser currentUser)
        {
            var item = await _itemRepository.GetByIdAsync(id, subId);
            return _mapper.Map<ItemResDto?>(item);
        }

        /// <summary>
        /// Creates a new item in the system.
        /// </summary>
        /// <param name="itemDto">The item data transfer object containing item information.</param>
        /// <param name="currentUser">The current user creating the item.</param>
        /// <returns>The newly created item details if successful, otherwise null.</returns>
        public async Task<ItemResDto?> CreateItemAsync(ItemReqDto itemDto, CurrentUser currentUser)
        {
            if (await _itemRepository.ItemExistsAsync(itemDto.Id, itemDto.SubId))
            {
                // An item with this composite key already exists.
                return null;
            }

            var item = _mapper.Map<Item>(itemDto);

            // Handle Supplier Linking
            if (itemDto.SupplierIds != null && itemDto.SupplierIds.Any())
            {
                foreach (var supplierId in itemDto.SupplierIds)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                    if (supplier != null)
                    {
                        item.Suppliers.Add(supplier);
                    }
                }
            }

            var newItem = await _itemRepository.AddAsync(item);
            return _mapper.Map<ItemResDto>(newItem);
        }

        /// <summary>
        /// Updates an existing item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to update.</param>
        /// <param name="subId">The sub-identifier of the item to update.</param>
        /// <param name="itemDto">The item data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user updating the item.</param>
        /// <returns>True if update was successful, otherwise false.</returns>
        public async Task<bool> UpdateItemAsync(int id, int subId, ItemReqDto itemDto, CurrentUser currentUser)
        {
            // Fetch the item with its related suppliers to update them
            var itemToUpdate = await _itemRepository.GetByIdWithSuppliersAsync(id, subId);
            if (itemToUpdate == null)
            {
                // Item not found.
                return false;
            }

            // Map flat properties from DTO to entity
            _mapper.Map(itemDto, itemToUpdate);

            // Handle Supplier Linking
            itemToUpdate.Suppliers.Clear(); // Clear existing links
            if (itemDto.SupplierIds != null && itemDto.SupplierIds.Any())
            {
                foreach (var supplierId in itemDto.SupplierIds)
                {
                    var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                    if (supplier != null)
                    {
                        itemToUpdate.Suppliers.Add(supplier);
                    }
                }
            }

            await _itemRepository.UpdateAsync(itemToUpdate);
            return true;
        }

        /// <summary>
        /// Deletes an item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to delete.</param>
        /// <param name="subId">The sub-identifier of the item to delete.</param>
        /// <param name="currentUser">The current user deleting the item.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteItemAsync(int id, int subId, CurrentUser currentUser)
        {
            var itemToDelete = await _itemRepository.GetByIdAsync(id, subId);
            if (itemToDelete == null)
            {
                // Item not found.
                return false;
            }

            await _itemRepository.DeleteAsync(itemToDelete);
            return true;
        }

        /// <summary>
        /// Retrieves all items that share the same main identifier.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of items with the specified main ID.</returns>
        public async Task<IEnumerable<ItemResDto>> GetItemsByMainIdAsync(int id, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetByMainIdAsync(id);
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        /// <summary>
        /// Retrieves complete item details by barcode.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>Complete item details if found, otherwise empty collection.</returns>
        public async Task<IEnumerable<ItemResDto>> GetItemByBarCodeAsync(string barCode, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetByBarCodeAsync(barCode);
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        /// <summary>
        /// Retrieves minimal item details by barcode for quick lookups.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>Minimal item details if found, otherwise empty collection.</returns>
        public async Task<IEnumerable<BaseitemResDto>> GetItemMinDetailsByBarCodeAsync(string barCode, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetByBarCodeAsync(barCode);

            //Filter active items 
            var activeItems = items.Where(x => x.IsActive == true);

            return _mapper.Map<IEnumerable<BaseitemResDto>>(activeItems);
        }

        /// <summary>
        /// Retrieves an item by its unique UUID identifier.
        /// </summary>
        /// <param name="uuid">The UUID of the item to retrieve.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>The item details if found, otherwise null.</returns>
        public async Task<ItemResDto?> GetItemByUuidAsync(string uuid, CurrentUser currentUser)
        {
            var item = await _itemRepository.GetByUuidAsync(uuid);
            return _mapper.Map<ItemResDto?>(item);
        }

        /// <summary>
        /// Adds stock quantity to an existing item.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="quantity">The quantity to add to the item's stock.</param>
        /// <param name="currentUser">The current user adding stock.</param>
        /// <returns>The updated item details if successful, otherwise null.</returns>
        public async Task<ItemResDto?> AddStockAsync(int id, int subId, decimal quantity, CurrentUser currentUser)
        {
            var item = await _itemRepository.GetByIdAsync(id, subId);
            if (item == null)
            {
                return null; // Item not found
            }

            // Handle null StockQuantity by initializing to 0
            item.StockQuantity = item.StockQuantity + quantity;

            await _itemRepository.UpdateAsync(item);
            return _mapper.Map<ItemResDto>(item);
        }

        /// <summary>
        /// Retrieves quantity information for all items with the specified main ID.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <param name="currentUser">The current user requesting the quantities.</param>
        /// <returns>A dictionary containing quantity information for the items.</returns>
        public async Task<Dictionary<string, decimal>> GetQuantitiesByMainIdAsync(int id, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetByMainIdAsync(id);
            // Creates a dictionary like: { "1001/0": 50, "1001/1": 25 }
            return items.ToDictionary(
                item => $"{item.Id}/{item.SubId}",
                item => item.StockQuantity
            );
        }

        /// <summary>
        /// Retrieves the current quantity of an item by its UUID.
        /// </summary>
        /// <param name="uuid">The UUID of the item.</param>
        /// <param name="currentUser">The current user requesting the quantity.</param>
        /// <returns>The quantity value if found, otherwise null.</returns>
        public async Task<decimal?> GetQuantityByUuidAsync(string uuid, CurrentUser currentUser)
        {
            var item = await _itemRepository.GetByUuidAsync(uuid);
            // Returns the quantity, or 0 if the item is found but stock is null.
            // Returns null if the item is not found at all.
            return item?.StockQuantity ?? (item != null ? 0m : null);
        }

        /// <summary>
        /// Retrieves the current quantity of an item by its composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <param name="currentUser">The current user requesting the quantity.</param>
        /// <returns>The quantity value if found, otherwise null.</returns>
        public async Task<decimal?> GetQuantityByIdAsync(int id, int subId, CurrentUser currentUser)
        {
            var item = await _itemRepository.GetByIdAsync(id, subId);
            // Returns the quantity, or 0 if the item is found but stock is null.
            // Returns null if the item is not found at all.
            return item?.StockQuantity ?? (item != null ? 0m : null);
        }

        /// <summary>
        /// Gets all items associated with a given supplier ID.
        /// </summary>
        /// <param name="supplierId">The unique identifier of the supplier.</param>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of items associated with the specified supplier.</returns>
        public async Task<IEnumerable<ItemResDto>> GetItemsBySupplierIdAsync(int supplierId, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetBySupplierIdAsync(supplierId);
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }
    }
}
