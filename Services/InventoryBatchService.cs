using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.InventoryBatches;
using pos_service.Models.DTO.StockMovements;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class InventoryBatchService : IInventoryBatchService
    {
        private readonly IInventoryBatchRepository _batchRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public InventoryBatchService(
            IInventoryBatchRepository batchRepository,
            IItemRepository itemRepository,
            IMapper mapper,
            ICacheService cache)
        {
            _batchRepository = batchRepository;
            _itemRepository  = itemRepository;
            _mapper          = mapper;
            _cache           = cache;
        }

        public async Task<IEnumerable<InventoryBatchResDto>> GetBatchesByItemUuidAsync(string itemUuid, bool includeExpired = false, CurrentUser? currentUser = null)
        {
            var batches = await _batchRepository.GetActiveBatchesByItemUuidAsync(itemUuid, includeExpired);
            return _mapper.Map<IEnumerable<InventoryBatchResDto>>(batches);
        }

        public async Task<IEnumerable<InventoryBatchSelectDto>> GetBatchesForPosAsync(string itemUuid, CurrentUser? currentUser = null)
        {
            // Retrieve active non-expired batches for POS checkout dropdown
            var batches = (await _batchRepository.GetActiveBatchesByItemUuidAsync(itemUuid, includeExpired: false)).ToList();
            var dtoList = _mapper.Map<List<InventoryBatchSelectDto>>(batches);

            if (dtoList.Any())
            {
                // The first batch in FEFO/FIFO order is marked as recommended for cashier auto-selection
                dtoList.First().IsRecommended = true;
            }

            return dtoList;
        }

        public async Task<InventoryBatchResDto> CreateBatchAsync(InventoryBatchReqDto dto, CurrentUser? currentUser = null)
        {
            // Verify parent item exists
            var item = await _itemRepository.GetByUuidAsync(dto.ItemUuid);
            if (item == null)
            {
                throw new ArgumentException($"Item with UUID {dto.ItemUuid} not found");
            }

            // Auto-generate sequential batch number if not supplied
            var batchNumber = string.IsNullOrWhiteSpace(dto.BatchNumber)
                ? await _batchRepository.GenerateBatchNumberAsync(dto.ItemUuid)
                : dto.BatchNumber;

            var batch = new InventoryBatch
            {
                Uuid                   = Guid.NewGuid().ToString(),
                ItemUuid               = dto.ItemUuid,
                BatchNumber            = batchNumber,
                ReceivedQuantity       = dto.Quantity,
                RemainingQuantity      = dto.Quantity,
                CostPrice              = dto.CostPrice,
                MarkedPrice            = dto.MarkedPrice,
                RetailPrice            = dto.RetailPrice,
                WholesalePrice         = dto.WholesalePrice,
                RetailDiscountRatio    = dto.RetailDiscountRatio,
                WholesaleDiscountRatio = dto.WholesaleDiscountRatio,
                Reference              = dto.Reference,
                PurchaseUuid           = dto.PurchaseUuid,
                SupplierUuid           = dto.SupplierUuid,
                Status                 = BatchStatus.Active,
                CreatedBy              = currentUser?.Uuid,
                IsActive               = true
            };

            // Create initial movement log for stock tracing
            var initialMovement = new StockMovement
            {
                Uuid          = Guid.NewGuid().ToString(),
                ItemUuid      = dto.ItemUuid,
                MovementType  = StockMovementType.Purchase,
                Quantity      = dto.Quantity,
                Direction     = StockMovementDirection.IN,
                CostPrice     = dto.CostPrice,
                ReferenceType = "ManualBatch",
                Reason        = dto.Reason ?? "Initial batch stock receipt",
                Comment       = dto.Comment,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = currentUser?.Uuid
            };

            var created = await _batchRepository.AddBatchAsync(batch, initialMovement);

            // Invalidate Redis/memory cache
            _cache.RemovePrimary(ServiceCacheKey.Items);

            return _mapper.Map<InventoryBatchResDto>(created);
        }

        public async Task<InventoryBatchResDto> UpdateBatchPricesAsync(
            string batchUuid,
            decimal costPrice,
            decimal markedPrice,
            decimal retailPrice,
            decimal wholesalePrice,
            decimal retailDiscountRatio,
            decimal wholesaleDiscountRatio,
            CurrentUser? currentUser = null)
        {
            var batch = await _batchRepository.GetByUuidAsync(batchUuid);
            if (batch == null)
            {
                throw new ArgumentException($"Batch with UUID {batchUuid} not found");
            }

            // Update individual price tier points and discount rates
            batch.CostPrice = costPrice;
            batch.MarkedPrice = markedPrice;
            batch.RetailPrice = retailPrice;
            batch.WholesalePrice = wholesalePrice;
            batch.RetailDiscountRatio = retailDiscountRatio;
            batch.WholesaleDiscountRatio = wholesaleDiscountRatio;
            batch.UpdatedBy = currentUser?.Uuid;
            batch.UpdatedAt = DateTime.UtcNow;

            var updated = await _batchRepository.UpdateBatchAsync(batch);

            // Invalidate item pricing cache
            _cache.RemovePrimary(ServiceCacheKey.Items);

            return _mapper.Map<InventoryBatchResDto>(updated);
        }

        public async Task<InventoryBatchResDto> AdjustBatchStockAsync(string batchUuid, decimal quantityDelta, StockMovementType type, string? reason = null, string? comment = null, CurrentUser? currentUser = null)
        {
            var batch = await _batchRepository.GetByUuidAsync(batchUuid);
            if (batch == null)
            {
                throw new ArgumentException($"Batch with UUID {batchUuid} not found");
            }

            var isIncrease = quantityDelta > 0;
            var absQuantity = Math.Abs(quantityDelta);

            // Check remaining batch capacity before deducting
            if (!isIncrease && absQuantity > batch.RemainingQuantity)
            {
                throw new InvalidOperationException($"Insufficient batch quantity. Remaining: {batch.RemainingQuantity}, Requested deduction: {absQuantity}");
            }

            // Update remaining stock quantity on the batch
            batch.RemainingQuantity = isIncrease
                ? batch.RemainingQuantity + absQuantity
                : batch.RemainingQuantity - absQuantity;

            // Expand received quantity if adjustment exceeds initial receipt
            if (isIncrease && batch.RemainingQuantity > batch.ReceivedQuantity)
            {
                batch.ReceivedQuantity = batch.RemainingQuantity;
            }

            // Record movement in audit ledger
            var movement = new StockMovement
            {
                Uuid          = Guid.NewGuid().ToString(),
                BatchUuid     = batch.Uuid,
                ItemUuid      = batch.ItemUuid,
                MovementType  = type,
                Quantity      = absQuantity,
                Direction     = isIncrease ? StockMovementDirection.IN : StockMovementDirection.OUT,
                CostPrice     = batch.CostPrice,
                Reason        = reason,
                Comment       = comment,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = currentUser?.Uuid
            };

            await _batchRepository.AddStockMovementAsync(movement);
            var updated = await _batchRepository.UpdateBatchAsync(batch);

            // Invalidate cache
            _cache.RemovePrimary(ServiceCacheKey.Items);

            return _mapper.Map<InventoryBatchResDto>(updated);
        }

        public async Task<InventoryBatchResDto> SetBatchStatusAsync(string batchUuid, bool isActive, BatchStatus? status = null, CurrentUser? currentUser = null)
        {
            var batch = await _batchRepository.GetByUuidAsync(batchUuid);
            if (batch == null)
            {
                throw new ArgumentException($"Batch with UUID {batchUuid} not found");
            }

            // Update active state and infer appropriate status enum
            batch.IsActive = isActive;
            if (status.HasValue)
            {
                batch.Status = status.Value;
            }
            else if (!isActive)
            {
                batch.Status = batch.RemainingQuantity <= 0 ? BatchStatus.Depleted : BatchStatus.WrittenOff;
            }
            else
            {
                batch.Status = BatchStatus.Active;
            }

            batch.UpdatedBy = currentUser?.Uuid;
            batch.UpdatedAt = DateTime.UtcNow;

            var updated = await _batchRepository.UpdateBatchAsync(batch);
            _cache.RemovePrimary(ServiceCacheKey.Items);

            return _mapper.Map<InventoryBatchResDto>(updated);
        }

        public async Task<IEnumerable<StockMovementResDto>> GetStockMovementsByItemUuidAsync(string itemUuid, CurrentUser? currentUser = null)
        {
            var movements = await _batchRepository.GetStockMovementsByItemUuidAsync(itemUuid);
            return _mapper.Map<IEnumerable<StockMovementResDto>>(movements);
        }

        public async Task<IEnumerable<InventoryBatchLogResDto>> GetBatchLogsByItemUuidAsync(string itemUuid, CurrentUser? currentUser = null)
        {
            var logs = await _batchRepository.GetBatchLogsByItemUuidAsync(itemUuid);
            return _mapper.Map<IEnumerable<InventoryBatchLogResDto>>(logs);
        }
    }
}
