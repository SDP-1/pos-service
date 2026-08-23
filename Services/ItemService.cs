using AutoMapper;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository      _itemRepository;
        private readonly IInventoryRepository      _inventoryRepository;
        private readonly IInventoryBatchRepository _batchRepository;
        private readonly ISettingService           _settingService;
        private readonly IMapper                   _mapper;
        private readonly ICacheService             _cache;

        /// <summary>
        /// Initializes a new instance of the ItemService.
        /// </summary>
        public ItemService(
            IItemRepository itemRepository,
            IInventoryRepository inventoryRepository,
            IInventoryBatchRepository batchRepository,
            ISettingService settingService,
            IMapper mapper,
            ICacheService cache)
        {
            _itemRepository      = itemRepository;
            _inventoryRepository = inventoryRepository;
            _batchRepository     = batchRepository;
            _settingService      = settingService;
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
            // Determine Id/SubId composite key values. If not supplied, assign next available sequence values.
            int idToUse;
            int subIdToUse;

            if (itemDto.Id.HasValue && itemDto.SubId.HasValue)
            {
                // Explicit composite key provided
                idToUse    = itemDto.Id.Value;
                subIdToUse = itemDto.SubId.Value;

                if (await _itemRepository.ItemExistsAsync(idToUse, subIdToUse))
                {
                    return null; // Item already exists with this composite key
                }
            }
            else if (itemDto.Id.HasValue && !itemDto.SubId.HasValue)
            {
                // Parent family ID specified; allocate next sub-variant index
                idToUse = itemDto.Id.Value;
                subIdToUse = await _itemRepository.GetNextSubIdAsync(idToUse);

                if (await _itemRepository.ItemExistsAsync(idToUse, subIdToUse))
                {
                    return null;
                }
            }
            else
            {
                // New parent item family; allocate next main ID and start subId at 0
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

            // Sync expiration dates and supplier linkages
            ApplyExpiries(item, itemDto);
            await ApplySuppliersAsync(item, itemDto.SupplierIds);

            item.AllowsDecimalQuantities = itemDto.AllowsDecimalQuantities;

            // Configure packaging units hierarchy and ensure base unit definition exists
            var unitsToCreate = (itemDto.Units ?? Enumerable.Empty<InventoryUnitReqDto>()).ToList();
            var baseUnitType = itemDto.UnitType != UnitType.None
                ? itemDto.UnitType
                : (unitsToCreate.FirstOrDefault(u => u.IsBaseUnit || u.QuantityInBaseUnits == 1)?.UnitType ?? UnitType.Each);

            // Insert base unit definition if missing from the collection
            if (!unitsToCreate.Any(u => u.IsBaseUnit || (u.UnitType == baseUnitType && u.QuantityInBaseUnits == 1)))
            {
                unitsToCreate.Insert(0, new InventoryUnitReqDto
                {
                    UnitType = baseUnitType,
                    ParentUnitType = baseUnitType,
                    QuantityPerParent = 1,
                    QuantityInBaseUnits = 1,
                    IsBaseUnit = true
                });
            }

            item.Units = unitsToCreate.Select(u => new ItemUnit
            {
                ItemUuid            = item.Uuid,
                UnitType            = u.UnitType,
                ParentUnitType      = u.ParentUnitType,
                QuantityPerParent   = u.QuantityPerParent,
                QuantityInBaseUnits = u.QuantityInBaseUnits,
                IsBaseUnit          = u.IsBaseUnit || (u.UnitType == baseUnitType && u.QuantityInBaseUnits == 1),
                Uuid                = Guid.NewGuid().ToString()
            }).ToList();

            await _itemRepository.SaveNewItemWithInventoryAsync(item);

            // Seed initial opening batch and stock movement ledger entry for this new item
            try
            {
                var initialBatch = new InventoryBatch
                {
                    Uuid                   = Guid.NewGuid().ToString(),
                    ItemUuid               = item.Uuid,
                    BatchNumber            = $"BATCH-INIT-{item.Id:D5}",
                    ReceivedQuantity       = itemDto.StockQuantity,
                    RemainingQuantity      = itemDto.StockQuantity,
                    CostPrice              = itemDto.Price?.BuyingPrice ?? 0,
                    MarkedPrice            = itemDto.Price?.MarkedPrice ?? 0,
                    RetailPrice            = itemDto.Price?.RetailPrice ?? 0,
                    WholesalePrice         = itemDto.Price?.WholesalePrice ?? 0,
                    RetailDiscountRatio    = itemDto.Price?.RetailDiscountRatio ?? 0,
                    WholesaleDiscountRatio = itemDto.Price?.WholesaleDiscountRatio ?? 0,
                    Reference              = "Initial Opening Lot",
                    SupplierUuid           = item.ItemSuppliers.FirstOrDefault()?.Supplier?.Uuid,
                    Status                 = BatchStatus.Active,
                    CreatedBy              = item.CreatedBy,
                    IsActive               = true
                };

                var initialMovement = new StockMovement
                {
                    Uuid          = Guid.NewGuid().ToString(),
                    ItemUuid      = item.Uuid,
                    MovementType  = StockMovementType.OpeningStock,
                    Quantity      = itemDto.StockQuantity,
                    Direction     = StockMovementDirection.IN,
                    CostPrice     = itemDto.Price?.BuyingPrice ?? 0,
                    Reason        = "Initial opening stock lot created with item",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = item.CreatedBy
                };

                await _batchRepository.AddBatchAsync(initialBatch, initialMovement);
            }
            catch (Exception)
            {
                // Fallback: batch initialization failure will not crash item creation
            }

            InvalidateCache();

            return _mapper.Map<ItemResDto>(item);
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

            ApplyExpiries(itemToUpdate, itemDto);

            await ApplySuppliersAsync(itemToUpdate, itemDto.SupplierIds);

            itemToUpdate.AllowsDecimalQuantities = itemDto.AllowsDecimalQuantities;

            var unitsToUpdate = (itemDto.Units ?? Enumerable.Empty<InventoryUnitReqDto>()).ToList();
            var updateBaseUnitType = itemDto.UnitType != UnitType.None
                ? itemDto.UnitType
                : (unitsToUpdate.FirstOrDefault(u => u.IsBaseUnit || u.QuantityInBaseUnits == 1)?.UnitType ?? UnitType.Each);

            if (!unitsToUpdate.Any(u => u.IsBaseUnit || (u.UnitType == updateBaseUnitType && u.QuantityInBaseUnits == 1)))
            {
                unitsToUpdate.Insert(0, new InventoryUnitReqDto
                {
                    UnitType = updateBaseUnitType,
                    ParentUnitType = updateBaseUnitType,
                    QuantityPerParent = 1,
                    QuantityInBaseUnits = 1,
                    IsBaseUnit = true
                });
            }

            itemToUpdate.Units = unitsToUpdate.Select(u => new ItemUnit
            {
                ItemUuid            = itemToUpdate.Uuid,
                UnitType            = u.UnitType,
                ParentUnitType      = u.ParentUnitType,
                QuantityPerParent   = u.QuantityPerParent,
                QuantityInBaseUnits = u.QuantityInBaseUnits,
                IsBaseUnit          = u.IsBaseUnit || (u.UnitType == updateBaseUnitType && u.QuantityInBaseUnits == 1),
                Uuid                = Guid.NewGuid().ToString()
            }).ToList();

            await _itemRepository.SaveUpdatedItemWithInventoryAsync(itemToUpdate);

            // Synchronize active batch prices or create default batch if none exists
            try
            {
                if (itemDto.Price != null)
                {
                    var activeBatches = await _batchRepository.GetActiveBatchesByItemUuidAsync(itemToUpdate.Uuid, includeExpired: true);
                    var primaryBatch = activeBatches.FirstOrDefault();
                    if (primaryBatch != null)
                    {
                        primaryBatch.CostPrice              = itemDto.Price.BuyingPrice;
                        primaryBatch.MarkedPrice            = itemDto.Price.MarkedPrice;
                        primaryBatch.RetailPrice            = itemDto.Price.RetailPrice;
                        primaryBatch.WholesalePrice         = itemDto.Price.WholesalePrice;
                        primaryBatch.RetailDiscountRatio    = itemDto.Price.RetailDiscountRatio;
                        primaryBatch.WholesaleDiscountRatio = itemDto.Price.WholesaleDiscountRatio;
                        primaryBatch.UpdatedBy              = currentUser?.Uuid;
                        primaryBatch.UpdatedAt              = DateTime.UtcNow;
                        await _batchRepository.UpdateBatchAsync(primaryBatch);
                    }
                    else
                    {
                        var newDefaultBatch = new InventoryBatch
                        {
                            Uuid                   = Guid.NewGuid().ToString(),
                            ItemUuid               = itemToUpdate.Uuid,
                            BatchNumber            = $"BATCH-INIT-{itemToUpdate.Id:D5}",
                            ReceivedQuantity       = 0,
                            RemainingQuantity      = 0,
                            CostPrice              = itemDto.Price.BuyingPrice,
                            MarkedPrice            = itemDto.Price.MarkedPrice,
                            RetailPrice            = itemDto.Price.RetailPrice,
                            WholesalePrice         = itemDto.Price.WholesalePrice,
                            RetailDiscountRatio    = itemDto.Price.RetailDiscountRatio,
                            WholesaleDiscountRatio = itemDto.Price.WholesaleDiscountRatio,
                            Reference              = "Default Batch",
                            SupplierUuid           = itemToUpdate.ItemSuppliers.FirstOrDefault()?.Supplier?.Uuid,
                            Status                 = BatchStatus.Active,
                            CreatedBy              = currentUser?.Uuid ?? itemToUpdate.CreatedBy,
                            IsActive               = true
                        };
                        await _batchRepository.AddBatchAsync(newDefaultBatch);
                    }
                }
            }
            catch (Exception)
            {
                // Fallback safe
            }

            InvalidateCache();

            return _mapper.Map<ItemResDto>(itemToUpdate);
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
            return await _itemRepository.GetResDtoByUuidAsync(uuid);
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

            var activeBatches = await _batchRepository.GetActiveBatchesByItemUuidAsync(itemDto.Uuid, includeExpired: true);
            var primaryBatch = activeBatches.FirstOrDefault();
            if (primaryBatch != null)
            {
                primaryBatch.ReceivedQuantity   += quantity;
                primaryBatch.RemainingQuantity  += quantity;
                primaryBatch.UpdatedBy           = currentUser?.Uuid;
                primaryBatch.UpdatedAt           = DateTime.UtcNow;
                await _batchRepository.UpdateBatchAsync(primaryBatch);

                var movement = new StockMovement
                {
                    Uuid          = Guid.NewGuid().ToString(),
                    BatchUuid     = primaryBatch.Uuid,
                    ItemUuid      = itemDto.Uuid,
                    MovementType  = StockMovementType.ManualAdjustIn,
                    Quantity      = quantity,
                    Direction     = StockMovementDirection.IN,
                    CostPrice     = primaryBatch.CostPrice,
                    Reason        = "Quick Add Stock",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = currentUser?.Uuid
                };
                await _batchRepository.AddStockMovementAsync(movement);
            }

            InvalidateCache();
            return await _itemRepository.GetByIdAsync(id, subId);
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
            if (item == null) return null;
            var batches = await _batchRepository.GetActiveBatchesByItemUuidAsync(item.Uuid, false);
            return batches.Sum(b => b.RemainingQuantity);
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
            if (item == null) return null;
            var batches = await _batchRepository.GetActiveBatchesByItemUuidAsync(item.Uuid, false);
            return batches.Sum(b => b.RemainingQuantity);
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
        /// Permanently deletes specified item expiry records by their unique identifiers.
        /// </summary>
        /// <param name="expiryUuids">The collection of expiry date UUIDs to delete.</param>
        /// <param name="currentUser">The current user requesting the operation.</param>
        /// <returns>The count of deleted expiry records.</returns>
        public async Task<int> DeleteExpiriesAsync(IEnumerable<string> expiryUuids, CurrentUser currentUser)
        {
            var count = await _itemRepository.DeleteExpiriesAsync(expiryUuids);
            InvalidateCache();
            return count;
        }

        private void InvalidateCache()
        {
            _cache.RemovePrimary(ServiceCacheKey.Items);
        }
    }
}
