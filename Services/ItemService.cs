using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Inventory;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository     _itemRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper             _mapper;
        private readonly ICacheService       _cache;

        /// <summary>
        /// Initializes a new instance of the ItemService.
        /// </summary>
        public ItemService(
            IItemRepository itemRepository,
            ISupplierRepository supplierRepository,
            IInventoryRepository inventoryRepository,
            IMapper mapper,
            ICacheService cache)
        {
            _itemRepository      = itemRepository;
            _supplierRepository  = supplierRepository;
            _inventoryRepository = inventoryRepository;
            _mapper              = mapper;
            _cache               = cache;
        }

        /// <summary>
        /// Retrieves all items from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of all item details.</returns>
        public async Task<IEnumerable<ItemResDto>> GetAllItemsAsync(CurrentUser currentUser)
        {
            return await _cache.GetOrCreateAsync<IEnumerable<ItemResDto>>(ServiceCacheKey.Items, null,
                () => _itemRepository.GetAllAsync());
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
            return await _itemRepository.GetByIdAsync(id, subId);
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
                idToUse    = itemDto.Id.Value;
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
                idToUse    = await _itemRepository.GetNextMainIdAsync();
                subIdToUse = 0;
            }

            var item   = _mapper.Map<Item>(itemDto);
            item.Id    = idToUse;
            item.SubId = subIdToUse;
            if (string.IsNullOrWhiteSpace(item.Uuid))
            {
                item.Uuid = Guid.NewGuid().ToString();
            }

            ApplyPrice(item, itemDto.Price);
            ApplyExpiries(item, itemDto);

            await ApplySuppliersAsync(item, itemDto.SupplierIds);

            var newItem = await _itemRepository.AddAsync(item);

            var inventory = new Inventory
            {
                ItemUuid                = newItem.Uuid,
                StockQuantity           = itemDto.StockQuantity,
                AllowsDecimalQuantities = itemDto.AllowsDecimalQuantities,
                UnitType                = itemDto.UnitType,
                Units                   = (itemDto.Units ?? Enumerable.Empty<InventoryUnitReqDto>()).Select(u => new InventoryUnit
                {
                    UnitType            = u.UnitType,
                    ParentUnitType      = u.ParentUnitType,
                    QuantityPerParent   = u.QuantityPerParent,
                    QuantityInBaseUnits = u.QuantityInBaseUnits,
                    Uuid                = Guid.NewGuid().ToString()
                }).ToList(),
                Uuid                    = Guid.NewGuid().ToString()
            };

            inventory         = await _inventoryRepository.AddAsync(inventory);
            newItem.Inventory = inventory;

            InvalidateCache();

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

            var inventory = await _inventoryRepository.GetByItemUuidAsync(itemToUpdate.Uuid)
                ?? throw new InvalidOperationException($"Inventory not found for item {itemToUpdate.Uuid}");

            inventory.StockQuantity           = itemDto.StockQuantity;
            inventory.AllowsDecimalQuantities = itemDto.AllowsDecimalQuantities;
            inventory.UnitType                = itemDto.UnitType;

            // Only replace Units if the request explicitly supplied them. This prevents
            // clearing packaging configuration when callers only update scalar fields
            // such as StockQuantity. Use ApplyInventoryUnits to only change when
            // units actually differ from existing ones.
            if (itemDto.Units != null && itemDto.Units.Any())
            {
                ApplyInventoryUnits(inventory, itemDto.Units);
            }

            await _inventoryRepository.UpdateAsync(inventory);
            result.Inventory = inventory;

            InvalidateCache();

            return _mapper.Map<ItemResDto>(result); ;
        }

        /// <summary>
        /// Deletes an item with the specified composite identifier.
        /// </summary>
        /// <param name="id">The main identifier of the item to delete.</param>
        /// <param name="subId">The sub-identifier of the item to delete.</param>
        /// <param name="currentUser">The current user deleting the item.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        public async Task<string?> DeleteItemAsync(int id, int subId, CurrentUser currentUser)
        {
            var error = await _itemRepository.DeleteAsync(id, subId);
            if (error != null)
                return error;

            InvalidateCache();

            return null;
        }

        /// <summary>
        /// Retrieves all items that share the same main identifier.
        /// </summary>
        /// <param name="id">The main identifier to search for.</param>
        /// <param name="currentUser">The current user requesting the items.</param>
        /// <returns>A list of items with the specified main ID.</returns>
        public async Task<IEnumerable<ItemResDto>> GetItemsByMainIdAsync(int id, CurrentUser currentUser)
        {
            // Repository now returns ItemResDto directly
            return await _itemRepository.GetByMainIdAsync(id);
        }

        /// <summary>
        /// Retrieves complete item details by barcode.
        /// </summary>
        /// <param name="barCode">The barcode to search for.</param>
        /// <param name="currentUser">The current user requesting the item.</param>
        /// <returns>Complete item details if found, otherwise empty collection.</returns>
        public async Task<IEnumerable<ItemResDto>> GetItemByBarCodeAsync(string barCode, CurrentUser currentUser)
        {
            return await _itemRepository.GetByBarCodeAsync(barCode);
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


            return _mapper.Map<IEnumerable<ItemMiniResDto>>(items);
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
            var itemDto = await _itemRepository.GetByIdAsync(id, subId);
            if (itemDto == null)
            {
                return null; // Item not found
            }

            var inventory = await _inventoryRepository.GetByItemUuidAsync(itemDto.Uuid)
                ?? throw new InvalidOperationException($"Inventory not found for item {itemDto.Uuid}");

            inventory.StockQuantity += quantity;

            await _inventoryRepository.UpdateAsync(inventory);

            // Update DTO fields from inventory
            itemDto.Inventory ??= new InventoryResDto();
            itemDto.Inventory.ItemUuid                = inventory.ItemUuid;
            itemDto.Inventory.StockQuantity           = inventory.StockQuantity;
            itemDto.Inventory.AllowsDecimalQuantities = inventory.AllowsDecimalQuantities;
            itemDto.Inventory.UnitType                = inventory.UnitType;
            itemDto.Inventory.Units                   = inventory.Units.Select(u => new InventoryUnitResDto
            {
                UnitType            = u.UnitType,
                ParentUnitType      = u.ParentUnitType,
                QuantityPerParent   = u.QuantityPerParent,
                QuantityInBaseUnits = u.QuantityInBaseUnits
            }).ToList();

            InvalidateCache();

            return itemDto;
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
                item => item.Inventory?.StockQuantity ?? 0m
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
            return item == null
                ? null
                : item.Inventory?.StockQuantity
                    ?? throw new InvalidOperationException($"Inventory not found for item {item.Uuid}");
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

            return item == null? null : item.Inventory?.StockQuantity ?? 0m;
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
            return await _itemRepository.GetBySearchAsync(searchTerm);
        }

        // Consolidated expiry application. Builds the target expiries from the DTO,
        // compares them to the existing ones and only applies (replaces) when there
        // is a difference. This avoids touching the collection when nothing changed.

        /// <summary>
        /// Ensures price details are applied to the item and keys are synchronized.
        /// </summary>
        private void ApplyPrice(Item item, ItemPriceReqDto? priceDto)
        {
            // Normalize incoming DTO
            var dto = priceDto ?? new ItemPriceReqDto();

            // If there's no existing price, create and map values
            if (item.Price == null)
            {
                item.Price = new ItemPrice
                {
                    ItemsId    = item.Id,
                    ItemsSubId = item.SubId,
                    ItemUuid   = item.Uuid,
                    Uuid       = Guid.NewGuid().ToString()
                };

                _mapper.Map(dto, item.Price);
                item.Price.ItemsId    = item.Id;
                item.Price.ItemsSubId = item.SubId;
                item.Price.ItemUuid   = item.Uuid;
                return;
            }

            // Determine if any meaningful price field changed. If not, avoid touching the entity.
            bool changed =
                item.Price.BuyingPrice           != dto.BuyingPrice ||
                item.Price.MarkedPrice           != dto.MarkedPrice ||
                item.Price.RetailPrice           != dto.RetailPrice ||
                item.Price.WholesalePrice        != dto.WholesalePrice ||
                item.Price.RetailDiscountRatio   != dto.RetailDiscountRatio ||
                item.Price.WholesaleDiscountRatio!= dto.WholesaleDiscountRatio;

            if (!changed)
                return;

            // Apply new values but preserve identity fields (Uuid) and keys.
            _mapper.Map(dto, item.Price);
            item.Price.ItemsId    = item.Id;
            item.Price.ItemsSubId = item.SubId;
            item.Price.ItemUuid   = item.Uuid;
        }

        /// <summary>
        /// Replaces expiry dates for the item based on the request DTO.
        /// </summary>
        private void ApplyExpiries(Item item, ItemReqDto itemDto)
        {
            // Build new expiries from DTO (group by date and notify days)
            var newExpiryKeys = (itemDto.ExpDates ?? Enumerable.Empty<ItemExpiryReqDto>())
                .GroupBy(exp => new { Date = exp.ExpDate.Date, exp.NotifyBeforeDays })
                .Select(g => new { Date = g.Key.Date, g.Key.NotifyBeforeDays })
                .ToHashSet();

            // Build existing expiry key set for comparison
            var existingExpiryKeys = (item.ExpDates ?? Enumerable.Empty<ItemExpiry>())
                .Select(e => new { Date = e.ExpDate.Date, e.NotifyBeforeDays })
                .ToHashSet();

            // If nothing changed, avoid touching the collection
            if (existingExpiryKeys.SetEquals(newExpiryKeys))
                return;

            // Replace expiries since there is a change
            item.ExpDates.Clear();

            foreach (var key in newExpiryKeys.OrderBy(k => k.Date))
            {
                item.ExpDates.Add(new ItemExpiry
                {
                    ItemsId          = item.Id,
                    ItemsSubId       = item.SubId,
                    ItemUuid         = item.Uuid,
                    ExpDate          = key.Date,
                    NotifyBeforeDays = key.NotifyBeforeDays,
                    Uuid             = Guid.NewGuid().ToString()
                });
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
                    item.ItemSuppliers.Add(new ItemSupplier
                    {
                        Uuid        = Guid.NewGuid().ToString(),
                        SuppliersId = supplierId,
                        ItemsId     = item.Id,
                        ItemsSubId  = item.SubId,
                        Item        = item
                    });
            }
        }

        /// <summary>
        /// Replaces inventory units only when there is a difference between the
        /// existing units and the desired units from the request DTO. This avoids
        /// touching the collection when nothing changed, following the pattern of
        /// ApplyExpiries.
        /// </summary>
        private void ApplyInventoryUnits(Inventory inventory, ICollection<InventoryUnitReqDto>? unitsDto)
        {
            if (unitsDto == null || !unitsDto.Any())
                return;

            inventory.Units ??= new List<InventoryUnit>();

            // Build key sets for comparison
            var newUnitKeys = unitsDto
                .Select(u => new { u.UnitType, u.ParentUnitType, u.QuantityPerParent,u.QuantityInBaseUnits})
                .ToHashSet();

            var existingUnitKeys = inventory.Units
                .Select(u => new { u.UnitType, u.ParentUnitType, u.QuantityPerParent,u.QuantityInBaseUnits })
                .ToHashSet();

            if (existingUnitKeys.SetEquals(newUnitKeys))
                return;

            // Replace units since there is a change
            inventory.Units.Clear();

            foreach (var unitDto in unitsDto.OrderBy(u => u.UnitType))
            {
                inventory.Units.Add(new InventoryUnit
                {
                    UnitType            = unitDto.UnitType,
                    ParentUnitType      = unitDto.ParentUnitType,
                    QuantityPerParent   = unitDto.QuantityPerParent,
                    QuantityInBaseUnits = unitDto.QuantityInBaseUnits,
                    InventoryId         = inventory.Id,
                    Uuid                = Guid.NewGuid().ToString()
                });
            }
        }

        private void InvalidateCache()
        {
            _cache.RemovePrimary(ServiceCacheKey.Items);
        }
    }
}
