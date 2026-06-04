using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.DTO.Items;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IItemRepository      _itemRepository;
        private readonly ISettingService      _settingService;
        private readonly IMapper              _mapper;
        private readonly ICacheService        _cache;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IItemRepository itemRepository,
            ISettingService settingService,
            IMapper mapper,
            ICacheService cache)
        {
            _inventoryRepository = inventoryRepository;
            _itemRepository      = itemRepository;
            _settingService      = settingService;
            _mapper              = mapper;
            _cache               = cache;
        }

        /// <summary>
        /// Retrieves all inventory records from the repository and maps to DTOs.
        /// </summary>
        /// <param name="currentUser">Current user context for potential authorization/audit.</param>
        /// <returns>Collection of InventoryResDto.</returns>
        public async Task<IEnumerable<InventoryResDto>> GetAllAsync(CurrentUser currentUser)
        {
            var inventories = await _inventoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<InventoryResDto>>(inventories);
        }

        /// <summary>
        /// Retrieves inventory for a specific item UUID.
        /// </summary>
        /// <param name="itemUuid">UUID of the item.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>Inventory DTO when found, otherwise null.</returns>
        public async Task<InventoryResDto?> GetByItemUuidAsync(string itemUuid, CurrentUser currentUser)
        {
            var inventory = await _inventoryRepository.GetByItemUuidAsync(itemUuid);
            return _mapper.Map<InventoryResDto?>(inventory);
        }

        /// <summary>
        /// Creates or updates the inventory record for the specified item UUID.
        /// When creating, initializes units based on provided packaging levels.
        /// </summary>
        /// <param name="itemUuid">UUID of the item.</param>
        /// <param name="dto">Inventory upsert DTO.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>The created or updated inventory DTO.</returns>
        public async Task<InventoryResDto> UpsertAsync(string itemUuid, InventoryReqDto dto, CurrentUser currentUser)
        {
            var item = await _itemRepository.GetByUuidAsync(itemUuid);
            if (item == null)
            {
                throw new ArgumentException($"Item with uuid {itemUuid} not found");
            }

            var inventory = await _inventoryRepository.GetByItemUuidAsync(itemUuid);
            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ItemUuid                = itemUuid,
                    StockQuantity           = dto.StockQuantity,
                    AllowsDecimalQuantities = dto.AllowsDecimalQuantities,
                    UnitType                = dto.UnitType,
                    Units                   = BuildUnits(dto, dto.UnitType),
                    Uuid                    = Guid.NewGuid().ToString()
                };

                await _inventoryRepository.AddAsync(inventory);
            }
            else
            {
                inventory.StockQuantity           = dto.StockQuantity;
                inventory.AllowsDecimalQuantities = dto.AllowsDecimalQuantities;
                inventory.UnitType                = dto.UnitType;

                inventory.Units.Clear();
                foreach (var unit in BuildUnits(dto, dto.UnitType))
                {
                    inventory.Units.Add(new InventoryUnit
                    {
                        UnitType            = unit.UnitType,
                        ParentUnitType      = unit.ParentUnitType,
                        QuantityPerParent   = unit.QuantityPerParent,
                        QuantityInBaseUnits = unit.QuantityInBaseUnits,
                        InventoryId         = inventory.Id,
                        Uuid                = Guid.NewGuid().ToString()
                    });
                }

                await _inventoryRepository.UpdateAsync(inventory);
            }

            InvalidateCache();

            // Expiries are managed via inventory adjustments. Upsert no longer modifies Item.ExpDates.

            return _mapper.Map<InventoryResDto>(inventory);
        }

        /// <summary>
        /// Adjusts the stock quantity for an item. Supports increasing or decreasing stock,
        /// validates packaging units and optional reason when decreasing, and can update expiries and price on the item.
        /// </summary>
        /// <param name="itemUuid">UUID of the item to adjust.</param>
        /// <param name="dto">Inventory adjustment request DTO.</param>
        /// <param name="currentUser">Current user performing the adjustment.</param>
        /// <returns>Updated inventory DTO when successful; otherwise null if inventory not found.</returns>
        public async Task<InventoryResDto?> AdjustStockAsync(string itemUuid, InventoryAdjustReqDto dto, CurrentUser currentUser)
        {
            var inventory = await _inventoryRepository.GetByItemUuidAsync(itemUuid);
            if (inventory == null)
            {
                return null;
            }

            var baseQuantity = ConvertToBaseQuantity(inventory, dto.UnitType, dto.Quantity);

            if (!inventory.AllowsDecimalQuantities && baseQuantity % 1 != 0)
            {
                throw new InvalidOperationException($"Item does not allow decimal quantities. Requested quantity resolves to {baseQuantity}.");
            }

            if (!dto.Increase && baseQuantity > inventory.StockQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available {inventory.StockQuantity}, requested {baseQuantity}");
            }

            // Validate reason if decreasing stock and setting requires it
            if (!dto.Increase)
            {
                var requireReasonSetting = await _settingService.GetByKeyAsync(SettingKey.RequireReasonOnDecreaseStock, currentUser);
                if (requireReasonSetting?.SettingValue == true && string.IsNullOrWhiteSpace(dto.Reason))
                {
                    throw new InvalidOperationException("Reason is required when decreasing inventory stock. Please provide a reason for this adjustment.");
                }
            }

            inventory.StockQuantity = dto.Increase
                ? inventory.StockQuantity + baseQuantity
                : inventory.StockQuantity - baseQuantity;

            // Update comment and reason on inventory record
            inventory.Comment = dto.Comment;
            inventory.Reason = dto.Reason;

            // Mark as user-adjusted for audit trail tracking
            inventory.IsUserAdjusted = true;

            await _inventoryRepository.UpdateAsync(inventory);

            // Manage expiries and price on the related Item if provided in the adjust request
            var item = await _itemRepository.GetByUuidAsync(inventory.ItemUuid);
            if (item != null)
            {
                var itemChanged = false;

                var expiriesChanged = ApplyExpiries(item, dto);
                var priceChanged = ApplyPrice(item, dto.Price);

                if (expiriesChanged || priceChanged)
                {
                    await _itemRepository.UpdateAsync(item);
                }
            }

            InvalidateCache();

            return _mapper.Map<InventoryResDto>(inventory);
        }

        private static decimal ConvertToBaseQuantity(Inventory inventory, UnitType requestedUnit, decimal quantity)
        {
            if (requestedUnit == inventory.UnitType)
            {
                return quantity;
            }

            var unitDef = inventory.Units.FirstOrDefault(u => u.UnitType == requestedUnit);
            if (unitDef == null)
            {
                throw new InvalidOperationException($"Unit type {requestedUnit} is not configured for this item.");
            }

            // Stored QuantityInBaseUnits represents the number of base units for the PARENT unit
            // (we persist baseFactors[parent]). To get how many base units one of the
            // requestedUnit represents, divide the stored parent base units by how many
            // requestedUnit fit into that parent (QuantityPerParent).
            if (unitDef.QuantityPerParent == 0)
            {
                throw new InvalidOperationException($"Invalid packaging configuration for unit {requestedUnit}: QuantityPerParent is zero.");
            }

            var baseUnitsPerRequestedUnit = unitDef.QuantityInBaseUnits / unitDef.QuantityPerParent;
            return quantity * baseUnitsPerRequestedUnit;
        }

        private static List<InventoryUnit> BuildUnits(InventoryReqDto dto, UnitType baseUnit)
        {
            if (dto.PackagingLevels != null && dto.PackagingLevels.Any())
            {
                // Semantics: UnitType is the child; ParentUnitType is the higher-level container.
                // 1 ParentUnitType contains QuantityPerParent of UnitType (child).
                // baseUnit is the smallest; compute base equivalents bottom-up.
                var baseFactors = new Dictionary<UnitType, decimal>
                {
                    [baseUnit] = 1m
                };

                // child -> parent relation (last wins if duplicate)
                var relationLookup = dto.PackagingLevels
                    .GroupBy(r => r.UnitType)
                    .ToDictionary(g => g.Key, g => g.Last());

                var unresolved = relationLookup.Values.ToList();
                var safety = 0;
                while (unresolved.Any() && safety < 100)
                {
                    safety++;
                    for (int i = unresolved.Count - 1; i >= 0; i--)
                    {
                        var rel = unresolved[i];
                        if (!baseFactors.TryGetValue(rel.UnitType, out var childFactor))
                        {
                            continue; // need child first
                        }

                        if (!baseFactors.ContainsKey(rel.ParentUnitType))
                        {
                            baseFactors[rel.ParentUnitType] = childFactor * rel.QuantityPerParent;
                        }

                        unresolved.RemoveAt(i);
                    }
                }

                if (unresolved.Any())
                {
                    var names = string.Join(", ", unresolved.Select(r => $"{r.ParentUnitType}->{r.UnitType}").Distinct());
                    throw new InvalidOperationException($"Unable to resolve packaging hierarchy for: {names}. Ensure the chain starts from the base unit {baseUnit}.");
                }

                // Only store child relations, not self-linked top-level parents
                var units = new List<InventoryUnit>();

                foreach (var rel in relationLookup.Values)
                {
                    // Persist the base-factor of the PARENT unit. This aligns stored
                    // QuantityInBaseUnits with the "2nd" option: QuantityInBaseUnits = baseFactors[parent]
                    units.Add(new InventoryUnit
                    {
                        UnitType            = rel.UnitType,
                        ParentUnitType      = rel.ParentUnitType,
                        QuantityPerParent   = rel.QuantityPerParent,
                        QuantityInBaseUnits = baseFactors[rel.ParentUnitType],
                        Uuid                = Guid.NewGuid().ToString()
                    });
                }

                // Add base unit if it's not already included as a child
                if (!units.Any(u => u.UnitType == baseUnit))
                {
                    units.Add(new InventoryUnit
                    {
                        UnitType            = baseUnit,
                        ParentUnitType      = baseUnit,
                        QuantityPerParent   = 1m,
                        QuantityInBaseUnits = 1m,
                        Uuid                = Guid.NewGuid().ToString()
                    });
                }

                return units
                    .OrderBy(u => u.QuantityInBaseUnits)
                    .ToList();
            }

            //return dto.Units.Select(u => new InventoryUnit
            //{
            //    UnitType = u.UnitType,
            //    ParentUnitType = u.ParentUnitType,
            //    QuantityPerParent = u.QuantityPerParent,
            //    QuantityInBaseUnits = u.QuantityInBaseUnits,
            //    Uuid = Guid.NewGuid().ToString()
            //})
            //.OrderBy(u => u.QuantityInBaseUnits)
            //.ToList();

            return new List<InventoryUnit>();
        }

        private static List<ItemExpiry> ResolveExpiries(InventoryAdjustReqDto dto, Item item)
        {
            if (dto.Expiries == null || !dto.Expiries.Any())
            {
                return new List<ItemExpiry>();
            }

            return dto.Expiries
                .GroupBy(exp => new { Date = exp.ExpDate.Date, exp.NotifyBeforeDays })
                .Select(group => new ItemExpiry
                {
                    ItemsId          = item.Id,
                    ItemsSubId       = item.SubId,
                    ItemUuid         = item.Uuid,
                    ExpDate          = group.Key.Date,
                    NotifyBeforeDays = group.Key.NotifyBeforeDays,
                    Uuid             = Guid.NewGuid().ToString()
                })
                .ToList();
        }

        private bool ApplyExpiries(Item item, InventoryAdjustReqDto dto)
        {
            // null means caller did not provide expiries -> no change
            if (dto.Expiries == null)
                return false;

            var newExpiries = ResolveExpiries(dto, item);

            // Build comparable sets of (date, notify) for comparison
            var existingSet = new HashSet<(DateTime date, int notify)>(
                item.ExpDates.Select(e => (e.ExpDate.Date, e.NotifyBeforeDays)));

            var newSet = new HashSet<(DateTime date, int notify)>(
                newExpiries.Select(e => (e.ExpDate.Date, e.NotifyBeforeDays)));

            if (existingSet.SetEquals(newSet))
            {
                // No change
                return false;
            }

            // Apply changes: clear and add new list
            item.ExpDates.Clear();
            foreach (var expiry in newExpiries)
            {
                item.ExpDates.Add(expiry);
            }

            return true;
        }

        private bool ApplyPrice(Item item, ItemPriceReqDto? priceDto)
        {
            // null means caller did not provide price -> no change
            if (priceDto == null)
                return false;

            if (item.Price == null)
            {
                item.Price = new ItemPrice
                {
                    ItemsId    = item.Id,
                    ItemsSubId = item.SubId,
                    ItemUuid   = item.Uuid,
                };

                _mapper.Map(priceDto, item.Price);
                item.Price.ItemsId    = item.Id;
                item.Price.ItemsSubId = item.SubId;
                item.Price.ItemUuid   = item.Uuid;
                item.Price.Uuid       = Guid.NewGuid().ToString();
                return true;
            }

            // Compare existing price values with incoming DTO
            bool differs = item.Price.BuyingPrice != priceDto.BuyingPrice
                || item.Price.MarkedPrice != priceDto.MarkedPrice
                || item.Price.RetailPrice != priceDto.RetailPrice
                || item.Price.WholesalePrice != priceDto.WholesalePrice
                || item.Price.RetailDiscountRatio != priceDto.RetailDiscountRatio
                || item.Price.WholesaleDiscountRatio != priceDto.WholesaleDiscountRatio;

            if (!differs)
                return false;

            // Apply mapping and update keys
            _mapper.Map(priceDto, item.Price);
            item.Price.ItemsId    = item.Id;
            item.Price.ItemsSubId = item.SubId;
            item.Price.ItemUuid   = item.Uuid;
            item.Price.Uuid       = Guid.NewGuid().ToString();
            return true;
        }

        private void InvalidateCache()
        {
            _cache.RemovePrimary(ServiceCacheKey.Items);
        }

        public async Task<IEnumerable<InventoryAdjustAuditResDto>> GetAuditHistoryAsync(
            string itemUuid,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxRecords = null,
            CurrentUser currentUser = null)
        {
            // Validate itemUuid
            if (string.IsNullOrWhiteSpace(itemUuid))
                throw new ArgumentException("ItemUuid is required");

            // Validate date range if both provided
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("StartDate cannot be greater than EndDate");

            // Validate max records
            if (maxRecords.HasValue && maxRecords < 1)
                throw new ArgumentException("MaxRecords must be at least 1");

            // Query audit history from repository
            var auditHistory = await _inventoryRepository.GetAuditHistoryAsync(
                itemUuid,
                startDate,
                endDate,
                maxRecords ?? 100);

            return auditHistory;
        }
    }
}
