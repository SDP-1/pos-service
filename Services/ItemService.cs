using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Items;
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
            // Determine Id/SubId values. If not supplied, assign next available values.
            int idToUse;
            int subIdToUse;

            if (itemDto.Id.HasValue && itemDto.SubId.HasValue)
            {
                idToUse = itemDto.Id.Value;
                subIdToUse = itemDto.SubId.Value;

                if (await _itemRepository.ItemExistsAsync(idToUse, subIdToUse))
                {
                    return null; // already exists
                }
            }
            else if (itemDto.Id.HasValue && !itemDto.SubId.HasValue)
            {
                idToUse = itemDto.Id.Value;
                // compute next sub id for this main id
                subIdToUse = await _itemRepository.GetNextSubIdAsync(idToUse);

                if (await _itemRepository.ItemExistsAsync(idToUse, subIdToUse))
                {
                    return null; // unlikely but safe
                }
            }
            else
            {
                // No Id provided. Get next main id and start subId at 0
                idToUse = await _itemRepository.GetNextMainIdAsync();
                subIdToUse = 0;
            }

            var item = _mapper.Map<Item>(itemDto);
            item.Id = idToUse;
            item.SubId = subIdToUse;
            if (string.IsNullOrWhiteSpace(item.Uuid))
            {
                item.Uuid = Guid.NewGuid().ToString();
            }

            ApplyPrice(item, itemDto.Price);
            ApplyExpiries(item, itemDto);

            await ApplySuppliersAsync(item, itemDto.SupplierIds);

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
        public async Task<ItemResDto> UpdateItemAsync(int id, int subId, ItemReqDto itemDto, CurrentUser currentUser)
        {
            // Fetch the item with its related suppliers to update them
            var itemToUpdate = await _itemRepository.GetByIdWithSuppliersAsync(id, subId);
            if (itemToUpdate == null)
            {
                // Item not found.
                return null;
            }

            // Map flat properties from DTO to entity
            _mapper.Map(itemDto, itemToUpdate);

            ApplyPrice(itemToUpdate, itemDto.Price);
            ApplyExpiries(itemToUpdate, itemDto);

            await ApplySuppliersAsync(itemToUpdate, itemDto.SupplierIds);

            var result = await _itemRepository.UpdateAsync(itemToUpdate);

            return _mapper.Map<ItemResDto>(result); ;
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
        public async Task<IEnumerable<ItemMiniResDto>> GetItemMinDetailsByBarCodeAsync(string barCode, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetByBarCodeAsync(barCode);

            //Filter active items 
            var activeItems = items.Where(x => x.IsActive == true);

            return _mapper.Map<IEnumerable<ItemMiniResDto>>(activeItems);
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

        /// <summary>
        /// Search items by term matching name, print name, barcode or uuid.
        /// </summary>
        public async Task<IEnumerable<ItemResDto>> SearchItemsAsync(string searchTerm, CurrentUser currentUser)
        {
            var items = await _itemRepository.GetBySearchAsync(searchTerm);
            return _mapper.Map<IEnumerable<ItemResDto>>(items);
        }

        private static List<ItemExpiry> ResolveExpiries(ItemReqDto itemDto, Item item)
        {
            if (itemDto.ExpDates == null || !itemDto.ExpDates.Any())
            {
                return new List<ItemExpiry>();
            }

            return itemDto.ExpDates
                .GroupBy(exp => new { Date = exp.ExpDate.Date, exp.NotifyBeforeDays })
                .Select(group => new ItemExpiry
                {
                    ItemsId = item.Id,
                    ItemsSubId = item.SubId,
                    ItemUuid = item.Uuid,
                    ExpDate = group.Key.Date,
                    NotifyBeforeDays = group.Key.NotifyBeforeDays,
                    Uuid = Guid.NewGuid().ToString()
                })
                .ToList();
        }

        /// <summary>
        /// Ensures price details are applied to the item and keys are synchronized.
        /// </summary>
        private void ApplyPrice(Item item, ItemPriceDto? priceDto)
        {

            if (item.Price == null)
            {
                item.Price = new ItemPrice
                {
                    ItemsId    = item.Id,
                    ItemsSubId = item.SubId,
                    ItemUuid   = item.Uuid
                };
            }

            _mapper.Map(priceDto ?? new ItemPriceDto(), item.Price);
            item.Price.ItemsId    = item.Id;
            item.Price.ItemsSubId = item.SubId;
            item.Price.Uuid       = Guid.NewGuid().ToString();
            item.Price.ItemUuid   = item.Uuid;
        }

        /// <summary>
        /// Replaces expiry dates for the item based on the request DTO.
        /// </summary>
        private void ApplyExpiries(Item item, ItemReqDto itemDto)
        {
            item.ExpDates.Clear();
            var expiries = ResolveExpiries(itemDto, item);
            foreach (var expiry in expiries)
            {
                item.ExpDates.Add(expiry);
            }
        }

        /// <summary>
        /// Rebuilds supplier links for the item based on the provided supplier IDs.
        /// </summary>
        private async Task ApplySuppliersAsync(Item item, ICollection<int>? supplierIds)
        {
            item.ItemSuppliers.Clear();
            if (supplierIds == null || !supplierIds.Any())
            {
                return;
            }

            foreach (var supplierId in supplierIds)
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                if (supplier != null)
                {
                    item.ItemSuppliers.Add(new ItemSupplier
                    {
                        Uuid        = Guid.NewGuid().ToString(),
                        SuppliersId = supplier.Id,
                        ItemsId     = item.Id,
                        ItemsSubId  = item.SubId,
                        Supplier    = supplier,
                        Item        = item
                    });
                }
            }
        }
    }
}
