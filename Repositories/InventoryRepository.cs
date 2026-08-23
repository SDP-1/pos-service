using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pos_service.Data;
using pos_service.Data.Utilities;
using pos_service.Models;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.DTO.Items;
using pos_service.Models.Enums;
using System.Data.Common;

namespace pos_service.Repositories
{
    public class InventoryRepository : BaseRepository, IInventoryRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryRepository"/> class with the database context and optional logger.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">Optional logger instance.</param>
        public InventoryRepository(AppDbContext context, ILogger<InventoryRepository>? logger = null) 
            : base(context)
        {
        }

        /// <summary>
        /// Retrieves inventory stock, active unit configurations, and audit details for a specific item by its UUID.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <returns>InventoryResDto if found; otherwise null.</returns>
        public async Task<InventoryResDto?> GetByItemUuidAsync(string itemUuid)
        {
            // Load item with expiration dates and packaging unit definitions
            var item = await _context.Items
                .Include(i => i.ExpDates)
                .Include(i => i.Units.OrderBy(u => u.QuantityInBaseUnits))
                .FirstOrDefaultAsync(i => i.Uuid == itemUuid);

            if (item == null)
            {
                return null;
            }

            // Retrieve all active inventory batches for this item to aggregate total stock
            var batches = await _context.InventoryBatches
                .Where(b => b.ItemUuid == item.Uuid && b.IsActive)
                .ToListAsync();

            // Calculate total on-hand stock quantity across all active batches
            var stockQty = batches.Sum(b => b.RemainingQuantity);

            // Determine primary batch for display pricing (active batch with remaining stock preferred)
            var primaryBatch = batches
                .OrderByDescending(b => b.Status == BatchStatus.Active)
                .ThenByDescending(b => b.RemainingQuantity > 0)
                .ThenByDescending(b => b.CreatedAt)
                .FirstOrDefault();

            // Determine item base unit (defaults to marked base unit or smallest quantity unit)
            var baseUnit = item.Units.FirstOrDefault(u => u.IsBaseUnit)?.UnitType
                ?? item.Units.OrderBy(u => u.QuantityInBaseUnits).FirstOrDefault()?.UnitType
                ?? UnitType.Each;

            return new InventoryResDto
            {
                ItemUuid                = item.Uuid,
                StockQuantity           = stockQty,
                BatchCount              = batches.Count,
                AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                UnitType                = baseUnit,
                Units                   = item.Units.Select(u => new InventoryUnitResDto
                {
                    UnitType            = u.UnitType,
                    ParentUnitType      = u.ParentUnitType ?? u.UnitType,
                    QuantityPerParent   = u.QuantityPerParent ?? 1m,
                    QuantityInBaseUnits = u.QuantityInBaseUnits,
                    IsBaseUnit          = u.IsBaseUnit,
                    Uuid                = u.Uuid,
                    CreatedAt           = item.CreatedAt,
                    UpdatedAt           = item.UpdatedAt,
                    CreatedBy           = item.CreatedBy,
                    UpdatedBy           = item.UpdatedBy,
                    IsActive            = true
                }).ToList(),
                Price                   = primaryBatch != null ? new ItemPriceResDto
                {
                    BuyingPrice            = primaryBatch.CostPrice,
                    MarkedPrice            = primaryBatch.MarkedPrice,
                    RetailPrice            = primaryBatch.RetailPrice,
                    WholesalePrice         = primaryBatch.WholesalePrice,
                    RetailDiscountRatio    = primaryBatch.RetailDiscountRatio,
                    WholesaleDiscountRatio = primaryBatch.WholesaleDiscountRatio,
                    Uuid                   = primaryBatch.Uuid,
                    CreatedAt              = primaryBatch.CreatedAt,
                    UpdatedAt              = primaryBatch.UpdatedAt,
                    CreatedBy              = primaryBatch.CreatedBy,
                    UpdatedBy              = primaryBatch.UpdatedBy,
                    IsActive               = primaryBatch.IsActive
                } : new ItemPriceResDto(),
                Expiries                = item.ExpDates.Where(ed => ed.IsActive).Select(ed => new ItemExpiryResDto
                {
                    ExpDate          = ed.ExpDate,
                    NotifyBeforeDays = ed.NotifyBeforeDays,
                    Uuid             = ed.Uuid,
                    CreatedAt        = ed.CreatedAt,
                    UpdatedAt        = ed.UpdatedAt,
                    CreatedBy        = ed.CreatedBy,
                    UpdatedBy        = ed.UpdatedBy,
                    IsActive         = ed.IsActive
                }).ToList(),
                Uuid                    = item.Uuid,
                CreatedAt               = item.CreatedAt,
                UpdatedAt               = item.UpdatedAt,
                CreatedBy               = item.CreatedBy,
                UpdatedBy               = item.UpdatedBy,
                IsActive                = item.IsActive
            };
        }

        /// <summary>
        /// Retrieves all inventory stock records across the system.
        /// </summary>
        /// <returns>Collection of InventoryResDto.</returns>
        public async Task<IEnumerable<InventoryResDto>> GetAllAsync()
        {
            // Load all items with navigation children
            var items = await _context.Items
                .Include(i => i.ExpDates)
                .Include(i => i.Units.OrderBy(u => u.QuantityInBaseUnits))
                .OrderBy(i => i.Id)
                .ToListAsync();

            // Fetch all active batches in a single query and group in memory to eliminate N+1 roundtrips
            var allBatches = await _context.InventoryBatches
                .Where(b => b.IsActive)
                .ToListAsync();

            var batchesByItem = allBatches.GroupBy(b => b.ItemUuid).ToDictionary(g => g.Key, g => g.ToList());
            var result = new List<InventoryResDto>();

            foreach (var item in items)
            {
                batchesByItem.TryGetValue(item.Uuid, out var batches);
                batches ??= new List<InventoryBatch>();

                // Sum remaining batch quantities for total stock
                var stockQty = batches.Sum(b => b.RemainingQuantity);
                var primaryBatch = batches
                    .OrderByDescending(b => b.Status == BatchStatus.Active)
                    .ThenByDescending(b => b.RemainingQuantity > 0)
                    .ThenByDescending(b => b.CreatedAt)
                    .FirstOrDefault();

                var itemBaseUnit = item.Units.FirstOrDefault(u => u.IsBaseUnit)?.UnitType
                    ?? item.Units.OrderBy(u => u.QuantityInBaseUnits).FirstOrDefault()?.UnitType
                    ?? UnitType.Each;

                result.Add(new InventoryResDto
                {
                    ItemUuid                = item.Uuid,
                    StockQuantity           = stockQty,
                    BatchCount              = batches.Count,
                    AllowsDecimalQuantities = item.AllowsDecimalQuantities,
                    UnitType                = itemBaseUnit,
                    Units                   = item.Units.Select(u => new InventoryUnitResDto
                    {
                        UnitType            = u.UnitType,
                        ParentUnitType      = u.ParentUnitType ?? u.UnitType,
                        QuantityPerParent   = u.QuantityPerParent ?? 1m,
                        QuantityInBaseUnits = u.QuantityInBaseUnits,
                        IsBaseUnit          = u.IsBaseUnit,
                        Uuid                = u.Uuid,
                        CreatedAt           = item.CreatedAt,
                        UpdatedAt           = item.UpdatedAt,
                        CreatedBy           = item.CreatedBy,
                        UpdatedBy           = item.UpdatedBy,
                        IsActive            = true
                    }).ToList(),
                    Price                   = primaryBatch != null ? new ItemPriceResDto
                    {
                        BuyingPrice            = primaryBatch.CostPrice,
                        MarkedPrice            = primaryBatch.MarkedPrice,
                        RetailPrice            = primaryBatch.RetailPrice,
                        WholesalePrice         = primaryBatch.WholesalePrice,
                        RetailDiscountRatio    = primaryBatch.RetailDiscountRatio,
                        WholesaleDiscountRatio = primaryBatch.WholesaleDiscountRatio,
                        Uuid                   = primaryBatch.Uuid,
                        CreatedAt              = primaryBatch.CreatedAt,
                        UpdatedAt              = primaryBatch.UpdatedAt,
                        CreatedBy              = primaryBatch.CreatedBy,
                        UpdatedBy              = primaryBatch.UpdatedBy,
                        IsActive               = primaryBatch.IsActive
                    } : new ItemPriceResDto(),
                    Expiries                = item.ExpDates.Where(ed => ed.IsActive).Select(ed => new ItemExpiryResDto
                    {
                        ExpDate          = ed.ExpDate,
                        NotifyBeforeDays = ed.NotifyBeforeDays,
                        Uuid             = ed.Uuid,
                        CreatedAt        = ed.CreatedAt,
                        UpdatedAt        = ed.UpdatedAt,
                        CreatedBy        = ed.CreatedBy,
                        UpdatedBy        = ed.UpdatedBy,
                        IsActive         = ed.IsActive
                    }).ToList(),
                    Uuid                    = item.Uuid,
                    CreatedAt               = item.CreatedAt,
                    UpdatedAt               = item.UpdatedAt,
                    CreatedBy               = item.CreatedBy,
                    UpdatedBy               = item.UpdatedBy,
                    IsActive                = item.IsActive
                });
            }

            return result;
        }

        /// <summary>
        /// Adjusts inventory stock quantity, writes an audit record, and updates associated packaging units.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item to adjust.</param>
        /// <param name="dto">Request DTO containing adjustment quantity, reason, and unit definitions.</param>
        /// <param name="currentUser">Optional current authenticated user context.</param>
        /// <returns>The updated InventoryResDto.</returns>
        public async Task<InventoryResDto> UpdateItemInventoryAsync(string itemUuid, InventoryReqDto dto, CurrentUser? currentUser = null)
        {
            var item = await _context.Items
                .Include(i => i.Units)
                .FirstOrDefaultAsync(i => i.Uuid == itemUuid)
                ?? throw new ArgumentException($"Item with UUID {itemUuid} not found");

            item.AllowsDecimalQuantities = dto.AllowsDecimalQuantities;
            item.UpdatedBy               = currentUser?.Uuid;
            item.UpdatedAt               = DateTime.UtcNow;

            // Remove existing packaging unit associations to replace with updated definitions
            _context.ItemUnits.RemoveRange(item.Units);
            var unitsToSave = dto.Units != null && dto.Units.Any() ? dto.Units.ToList() : new List<InventoryUnitReqDto>();
            var baseUnitType = dto.UnitType != UnitType.None
                ? dto.UnitType
                : (unitsToSave.FirstOrDefault(u => u.IsBaseUnit || u.QuantityInBaseUnits == 1)?.UnitType ?? UnitType.Each);

            // Ensure base unit is explicitly represented in the packaging units collection
            if (!unitsToSave.Any(u => u.IsBaseUnit || (u.UnitType == baseUnitType && u.QuantityInBaseUnits == 1)))
            {
                unitsToSave.Insert(0, new InventoryUnitReqDto
                {
                    UnitType = baseUnitType,
                    ParentUnitType = baseUnitType,
                    QuantityPerParent = 1,
                    QuantityInBaseUnits = 1,
                    IsBaseUnit = true
                });
            }

            // Persist each configured unit
            foreach (var u in unitsToSave)
            {
                var isBase = u.IsBaseUnit || (u.UnitType == baseUnitType && u.QuantityInBaseUnits == 1);
                _context.ItemUnits.Add(new ItemUnit
                {
                    ItemUuid            = item.Uuid,
                    UnitType            = u.UnitType,
                    ParentUnitType      = u.ParentUnitType,
                    QuantityPerParent   = u.QuantityPerParent,
                    QuantityInBaseUnits = u.QuantityInBaseUnits,
                    IsBaseUnit          = isBase,
                    Uuid                = Guid.NewGuid().ToString()
                });
            }

            await _context.SaveChangesAsync();
            return (await GetByItemUuidAsync(itemUuid))!;
        }

        /// <summary>
        /// Retrieves historical manual adjustment audits for an item using stored procedure <c>sp_get_inventory_audit_history</c>.
        /// </summary>
        /// <param name="itemUuid">The unique identifier (UUID) of the item.</param>
        /// <param name="startDate">Optional start date boundary.</param>
        /// <param name="endDate">Optional end date boundary.</param>
        /// <param name="maxRecords">Optional limit on number of returned audit entries.</param>
        /// <returns>Collection of InventoryAdjustAuditResDto records.</returns>
        public async Task<IEnumerable<InventoryAdjustAuditResDto>> GetAuditHistoryAsync(
            string itemUuid,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? maxRecords = null)
        {
            // Prepare parameterized inputs to execute database stored procedure safely
            var parameters = new DbParameter[]
            {
                CreateParameter("p_item_uuid", itemUuid),
                CreateParameter("p_start_date", startDate),
                CreateParameter("p_end_date", endDate),
                CreateParameter("p_max_records", maxRecords ?? 100)
            };

            return await ExecuteStoredProcedureAsync<InventoryAdjustAuditResDto>(
                "sp_get_inventory_audit_history",
                parameters);
        }
    }
}
