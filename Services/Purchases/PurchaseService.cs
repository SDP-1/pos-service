using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Purchases;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Repositories.Purchases;
using pos_service.Services.Common.Cache;

namespace pos_service.Services.Purchases
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IInventoryBatchRepository _batchRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IInventoryBatchRepository batchRepository,
            IItemRepository itemRepository,
            IMapper mapper,
            ICacheService cache)
        {
            _purchaseRepository = purchaseRepository;
            _batchRepository    = batchRepository;
            _itemRepository     = itemRepository;
            _mapper             = mapper;
            _cache              = cache;
        }

        /// <summary>
        /// Retrieves all purchase receipts from the database mapped to response DTOs.
        /// </summary>
        /// <param name="currentUser">The authenticated user executing the query.</param>
        /// <returns>A collection of purchase receipt response DTOs.</returns>
        public async Task<IEnumerable<PurchaseResDto>> GetAllPurchasesAsync(CurrentUser? currentUser = null)
        {
            var purchases = await _purchaseRepository.GetAllPurchasesAsync();
            return _mapper.Map<IEnumerable<PurchaseResDto>>(purchases);
        }

        /// <summary>
        /// Retrieves a specific purchase receipt by its unique UUID.
        /// </summary>
        /// <param name="purchaseUuid">The unique UUID identifier of the purchase receipt.</param>
        /// <param name="currentUser">The authenticated user executing the query.</param>
        /// <returns>The purchase receipt response DTO if found, otherwise null.</returns>
        public async Task<PurchaseResDto?> GetByUuidAsync(string purchaseUuid, CurrentUser? currentUser = null)
        {
            var purchase = await _purchaseRepository.GetByUuidAsync(purchaseUuid);
            return purchase != null ? _mapper.Map<PurchaseResDto>(purchase) : null;
        }

        /// <summary>
        /// Creates a new purchase receipt, generates stock batches, logs initial stock movements, and updates item expiries.
        /// </summary>
        /// <param name="dto">The purchase receipt creation payload containing supplier, invoice, and line items.</param>
        /// <param name="currentUser">The authenticated user creating the purchase receipt.</param>
        /// <returns>The created purchase receipt response DTO.</returns>
        public async Task<PurchaseResDto> CreatePurchaseAsync(PurchaseReqDto dto, CurrentUser? currentUser = null)
        {
            // Validate that purchase receipt contains at least one line item
            if (dto.Items == null || !dto.Items.Any())
            {
                throw new ArgumentException("At least one line item is required for a purchase receipt");
            }

            // Generate sequential purchase number and calculate total cost
            var purchaseNumber = await _purchaseRepository.GeneratePurchaseNumberAsync();
            var totalCost = dto.Items.Sum(i => i.Quantity * i.CostPrice);

            // Construct and persist parent purchase receipt entity
            var purchase = new Purchase
            {
                Uuid           = Guid.NewGuid().ToString(),
                PurchaseNumber = purchaseNumber,
                SupplierUuid   = dto.SupplierUuid,
                InvoiceNumber  = dto.InvoiceNumber,
                PurchaseDate   = dto.PurchaseDate,
                TotalCost      = totalCost,
                TotalItems     = dto.Items.Count,
                Status         = PurchaseStatus.Received,
                Notes          = dto.Notes,
                CreatedBy      = currentUser?.Uuid,
                IsActive       = true
            };

            var savedPurchase = await _purchaseRepository.AddPurchaseAsync(purchase);

            // Process each purchase line item: create inventory batch and log stock movement
            foreach (var line in dto.Items)
            {
                var item = await _itemRepository.GetByUuidAsync(line.ItemUuid);
                if (item == null)
                {
                    continue;
                }

                // Generate batch number if not supplied by user
                var batchNumber = string.IsNullOrWhiteSpace(line.BatchNumber)
                    ? await _batchRepository.GenerateBatchNumberAsync(line.ItemUuid)
                    : line.BatchNumber;

                // Create new inventory batch representing received stock and cost price
                var batch = new InventoryBatch
                {
                    Uuid                   = Guid.NewGuid().ToString(),
                    ItemUuid               = line.ItemUuid,
                    BatchNumber            = batchNumber,
                    ReceivedQuantity       = line.Quantity,
                    RemainingQuantity      = line.Quantity,
                    CostPrice              = line.CostPrice,
                    MarkedPrice            = line.MarkedPrice,
                    RetailPrice            = line.RetailPrice,
                    WholesalePrice         = line.WholesalePrice,
                    RetailDiscountRatio    = line.RetailDiscountRatio,
                    WholesaleDiscountRatio = line.WholesaleDiscountRatio,
                    Reference              = line.Reference ?? $"Purchase {savedPurchase.PurchaseNumber}",
                    PurchaseUuid           = savedPurchase.Uuid,
                    SupplierUuid           = savedPurchase.SupplierUuid,
                    Status                 = BatchStatus.Active,
                    CreatedBy              = currentUser?.Uuid,
                    IsActive               = true
                };

                // Record inbound stock movement in audit ledger
                var initialMovement = new StockMovement
                {
                    Uuid          = Guid.NewGuid().ToString(),
                    ItemUuid      = line.ItemUuid,
                    MovementType  = StockMovementType.Purchase,
                    Quantity      = line.Quantity,
                    Direction     = StockMovementDirection.IN,
                    CostPrice     = line.CostPrice,
                    ReferenceType = "Purchase",
                    ReferenceUuid = savedPurchase.Uuid,
                    Reason        = $"Purchase receipt: {savedPurchase.PurchaseNumber}",
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = currentUser?.Uuid
                };

                await _batchRepository.AddBatchAsync(batch, initialMovement);

                // Add any newly provided expiry dates to item if not already existing
                if (line.ExpDates != null && line.ExpDates.Any())
                {
                    var validExpiries = line.ExpDates
                        .Where(e => e.ExpDate != default)
                        .ToList();

                    if (validExpiries.Any())
                    {
                        var existingDates = (item.ExpDates ?? Enumerable.Empty<ItemExpiry>())
                            .Select(e => e.ExpDate.Date)
                            .ToHashSet();

                        foreach (var expReq in validExpiries)
                        {
                            if (!existingDates.Contains(expReq.ExpDate.Date))
                            {
                                item.ExpDates.Add(new ItemExpiry
                                {
                                    ItemsId          = item.Id,
                                    ItemsSubId       = item.SubId,
                                    ItemUuid         = item.Uuid,
                                    ExpDate          = expReq.ExpDate.Date,
                                    NotifyBeforeDays = expReq.NotifyBeforeDays > 0 ? expReq.NotifyBeforeDays : 7,
                                    Uuid             = Guid.NewGuid().ToString(),
                                    CreatedBy        = currentUser?.Uuid,
                                    CreatedAt        = DateTime.UtcNow,
                                    IsActive         = true
                                });
                                existingDates.Add(expReq.ExpDate.Date);
                            }
                        }
                        await _itemRepository.UpdateAsync(item);
                    }
                }
            }

            // Invalidate cache so frontend gets fresh stock and price data
            _cache.RemovePrimary(ServiceCacheKey.Items);

            var complete = await _purchaseRepository.GetByUuidAsync(savedPurchase.Uuid);
            return _mapper.Map<PurchaseResDto>(complete ?? savedPurchase);
        }

        /// <summary>
        /// Deletes a purchase receipt by its unique UUID and invalidates the item cache.
        /// </summary>
        /// <param name="purchaseUuid">The unique UUID identifier of the purchase receipt to delete.</param>
        /// <param name="currentUser">The authenticated user performing the deletion.</param>
        /// <returns>True if the purchase receipt was successfully deleted, otherwise false.</returns>
        public async Task<bool> DeletePurchaseAsync(string purchaseUuid, CurrentUser? currentUser = null)
        {
            var result = await _purchaseRepository.DeletePurchaseAsync(purchaseUuid);
            if (result)
            {
                _cache.RemovePrimary(ServiceCacheKey.Items);
            }
            return result;
        }
    }
}
