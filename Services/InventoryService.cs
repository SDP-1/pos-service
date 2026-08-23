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
        private readonly IInventoryRepository      _inventoryRepository;
        private readonly IItemRepository           _itemRepository;
        private readonly IInventoryBatchRepository _batchRepository;
        private readonly ISettingService           _settingService;
        private readonly IMapper                   _mapper;
        private readonly ICacheService             _cache;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IItemRepository itemRepository,
            IInventoryBatchRepository batchRepository,
            ISettingService settingService,
            IMapper mapper,
            ICacheService cache)
        {
            _inventoryRepository = inventoryRepository;
            _itemRepository      = itemRepository;
            _batchRepository     = batchRepository;
            _settingService      = settingService;
            _mapper              = mapper;
            _cache               = cache;
        }

        public async Task<IEnumerable<InventoryResDto>> GetAllAsync(CurrentUser currentUser)
        {
            return await _inventoryRepository.GetAllAsync();
        }

        public async Task<InventoryResDto?> GetByItemUuidAsync(string itemUuid, CurrentUser currentUser)
        {
            return await _inventoryRepository.GetByItemUuidAsync(itemUuid);
        }

        public async Task<InventoryResDto> UpdateAsync(string itemUuid, InventoryReqDto dto, CurrentUser currentUser)
        {
            var updated = await _inventoryRepository.UpdateItemInventoryAsync(itemUuid, dto, currentUser);
            InvalidateCache();
            return updated;
        }

        public async Task<InventoryResDto?> AdjustStockAsync(string itemUuid, InventoryAdjustReqDto dto, CurrentUser currentUser)
        {
            var inventory = await _inventoryRepository.GetByItemUuidAsync(itemUuid);
            if (inventory == null)
            {
                return null;
            }

            // Convert requested unit quantity into item base unit quantity (e.g. dozens/boxes -> single pieces)
            var baseQuantity = ConvertToBaseQuantity(inventory, dto.UnitType, dto.Quantity);

            // Validate fractional quantities if item does not permit decimals
            if (!inventory.AllowsDecimalQuantities && baseQuantity % 1 != 0)
            {
                throw new InvalidOperationException($"Item does not allow decimal quantities. Requested quantity resolves to {baseQuantity}.");
            }

            // Check sufficient stock on reduction
            if (!dto.Increase && baseQuantity > inventory.StockQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available {inventory.StockQuantity}, requested {baseQuantity}");
            }

            // Enforce mandatory reason setting for stock deductions if configured in system settings
            if (!dto.Increase)
            {
                var requireReasonSetting = await _settingService.GetByKeyAsync(SettingKey.RequireReasonOnDecreaseStock, currentUser);
                if (requireReasonSetting?.SettingValue == true && string.IsNullOrWhiteSpace(dto.Reason))
                {
                    throw new InvalidOperationException("Reason is required when decreasing inventory stock. Please provide a reason for this adjustment.");
                }
            }

            // Find an existing active batch or synthesize an initial lot
            var activeBatches = await _batchRepository.GetActiveBatchesByItemUuidAsync(itemUuid, includeExpired: true);
            var primaryBatch = activeBatches.FirstOrDefault();
            if (primaryBatch == null)
            {
                primaryBatch = new InventoryBatch
                {
                    Uuid                   = Guid.NewGuid().ToString(),
                    ItemUuid               = itemUuid,
                    BatchNumber            = $"BATCH-ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    ReceivedQuantity       = dto.Increase ? baseQuantity : 0,
                    RemainingQuantity      = dto.Increase ? baseQuantity : 0,
                    CostPrice              = dto.Price?.BuyingPrice ?? 0,
                    MarkedPrice            = dto.Price?.MarkedPrice ?? 0,
                    RetailPrice            = dto.Price?.RetailPrice ?? 0,
                    WholesalePrice         = dto.Price?.WholesalePrice ?? 0,
                    RetailDiscountRatio    = dto.Price?.RetailDiscountRatio ?? 0,
                    WholesaleDiscountRatio = dto.Price?.WholesaleDiscountRatio ?? 0,
                    Reference              = "Stock Adjustment Lot",
                    Status                 = BatchStatus.Active,
                    CreatedBy              = currentUser?.Uuid,
                    IsActive               = true
                };
                await _batchRepository.AddBatchAsync(primaryBatch);
            }
            else
            {
                // Increment or decrement batch stock quantity
                if (dto.Increase)
                {
                    primaryBatch.ReceivedQuantity += baseQuantity;
                    primaryBatch.RemainingQuantity += baseQuantity;
                }
                else
                {
                    primaryBatch.RemainingQuantity = Math.Max(0, primaryBatch.RemainingQuantity - baseQuantity);
                }

                // Update pricing tier overrides if supplied in adjustment payload
                if (dto.Price != null)
                {
                    primaryBatch.CostPrice              = dto.Price.BuyingPrice;
                    primaryBatch.MarkedPrice            = dto.Price.MarkedPrice;
                    primaryBatch.RetailPrice            = dto.Price.RetailPrice;
                    primaryBatch.WholesalePrice         = dto.Price.WholesalePrice;
                    primaryBatch.RetailDiscountRatio    = dto.Price.RetailDiscountRatio;
                    primaryBatch.WholesaleDiscountRatio = dto.Price.WholesaleDiscountRatio;
                }

                primaryBatch.UpdatedBy = currentUser?.Uuid;
                primaryBatch.UpdatedAt = DateTime.UtcNow;
                await _batchRepository.UpdateBatchAsync(primaryBatch);
            }

            // Create movement audit ledger row
            var movement = new StockMovement
            {
                Uuid          = Guid.NewGuid().ToString(),
                BatchUuid     = primaryBatch.Uuid,
                ItemUuid      = itemUuid,
                MovementType  = dto.Increase ? StockMovementType.ManualAdjustIn : StockMovementType.ManualAdjustOut,
                Quantity      = baseQuantity,
                Direction     = dto.Increase ? StockMovementDirection.IN : StockMovementDirection.OUT,
                CostPrice     = primaryBatch.CostPrice,
                Reason        = dto.Reason,
                Comment       = dto.Comment,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = currentUser?.Uuid
            };
            await _batchRepository.AddStockMovementAsync(movement);

            // Update item expiration dates if modified
            var item = await _itemRepository.GetByUuidAsync(itemUuid);
            if (item != null)
            {
                ApplyExpiries(item, dto);
                await _itemRepository.SaveUpdatedItemWithInventoryAsync(item);
            }

            InvalidateCache();
            return await _inventoryRepository.GetByItemUuidAsync(itemUuid);
        }

        private static decimal ConvertToBaseQuantity(InventoryResDto inventory, UnitType requestedUnit, decimal quantity)
        {
            // If requested unit matches base unit directly, return raw quantity
            var baseUnit = inventory.Units.FirstOrDefault(u => u.IsBaseUnit)?.UnitType ?? inventory.UnitType;
            if (requestedUnit == baseUnit)
            {
                return quantity;
            }

            // Locate packaging unit definition
            var unitDef = inventory.Units.FirstOrDefault(u => u.UnitType == requestedUnit);
            if (unitDef == null)
            {
                throw new InvalidOperationException($"Unit type {requestedUnit} is not configured for this item.");
            }

            // Convert to base units using precalculated multiplier or parent multiplier
            if (unitDef.QuantityInBaseUnits > 0)
            {
                return quantity * unitDef.QuantityInBaseUnits;
            }

            if (unitDef.QuantityPerParent == 0)
            {
                throw new InvalidOperationException($"Invalid packaging configuration for unit {requestedUnit}: QuantityPerParent is zero.");
            }

            return quantity * unitDef.QuantityPerParent;
        }

        private static List<ItemExpiry> ResolveExpiries(InventoryAdjustReqDto dto, Item item)
        {
            if (dto.Expiries == null || !dto.Expiries.Any())
            {
                return new List<ItemExpiry>();
            }

            // Deduplicate expiries by normalized date and notification days
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
            if (dto.Expiries == null)
                return false;

            var newExpiries = ResolveExpiries(dto, item);

            // Compare existing expiry set to avoid redundant database writes
            var existingSet = new HashSet<(DateTime date, int notify)>(
                item.ExpDates.Select(e => (e.ExpDate.Date, e.NotifyBeforeDays)));

            var newSet = new HashSet<(DateTime date, int notify)>(
                newExpiries.Select(e => (e.ExpDate.Date, e.NotifyBeforeDays)));

            if (existingSet.SetEquals(newSet))
            {
                return false;
            }

            item.ExpDates.Clear();
            foreach (var expiry in newExpiries)
            {
                item.ExpDates.Add(expiry);
            }

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
            if (string.IsNullOrWhiteSpace(itemUuid))
                throw new ArgumentException("ItemUuid is required");

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new ArgumentException("StartDate cannot be greater than EndDate");

            if (maxRecords.HasValue && maxRecords < 1)
                throw new ArgumentException("MaxRecords must be at least 1");

            return await _inventoryRepository.GetAuditHistoryAsync(
                itemUuid,
                startDate,
                endDate,
                maxRecords ?? 100);
        }
    }
}
