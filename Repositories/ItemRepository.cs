using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using System.Collections.Immutable;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.DTO.Contacts;
using pos_service.Models.Enums;
using System.Linq;

namespace pos_service.Repositories
{
    public class ItemRepository : BaseRepository, IItemRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRepository"/> class with the database context.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public ItemRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Adds a new item and persists its initial state within a database transaction.
        /// </summary>
        /// <param name="item">The item entity to insert.</param>
        public async Task SaveNewItemWithInventoryAsync(Item item)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Updates an existing item entity within a database transaction.
        /// </summary>
        /// <param name="itemToUpdate">The modified item entity to update.</param>
        public async Task SaveUpdatedItemWithInventoryAsync(Item itemToUpdate)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Items.Update(itemToUpdate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific item by its composite identifier (ID and SubID) projected as a response DTO.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>The ItemResDto if found; otherwise null.</returns>
        public async Task<ItemResDto?> GetByIdAsync(int id, int subId)
        {
            var query = _context.Items
                        .Where(i => i.Id == id && i.SubId == subId);

            var result = await makeItemResponceDto(_context, query);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all items from the database projected as response DTOs.
        /// </summary>
        /// <returns>Collection of ItemResDto.</returns>
        public async Task<IEnumerable<ItemResDto>> GetAllAsync()
        {
            var query = _context.Items.AsQueryable();

            return await makeItemResponceDto(_context, query);
        }

        /// <summary>
        /// Adds a new item entity to the database and saves changes.
        /// </summary>
        /// <param name="item">The item entity to add.</param>
        /// <returns>The created Item entity.</returns>
        public async Task<Item> AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        /// <summary>
        /// Calculates the next available main ID for a new item family.
        /// </summary>
        /// <returns>The next integer ID.</returns>
        public async Task<int> GetNextMainIdAsync()
        {
            var maxId = await _context.Items.MaxAsync(i => (int?)i.Id) ?? 0;
            return maxId + 1;
        }

        /// <summary>
        /// Calculates the next available sub-variant ID for a given main ID.
        /// </summary>
        /// <param name="mainId">The main ID of the item family.</param>
        /// <returns>The next integer sub-variant ID.</returns>
        public async Task<int> GetNextSubIdAsync(int mainId)
        {
            var maxSubId = await _context.Items.Where(i => i.Id == mainId).MaxAsync(i => (int?)i.SubId) ?? -1;
            return maxSubId + 1;
        }

        /// <summary>
        /// Marks an item entity as modified and saves changes to the database.
        /// </summary>
        /// <param name="item">The item entity to update.</param>
        /// <returns>The updated Item entity.</returns>
        public async Task<Item> UpdateAsync(Item item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return item;
        }

        /// <summary>
        /// Deletes an item from the database by its composite identifier (ID and SubID).
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>Error message string if item not found; otherwise null on success.</returns>
        public async Task<string?> DeleteAsync(int id, int subId)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id && i.SubId == subId);

            if (item == null)
                return $"Item not found.";

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return null;
        }

        /// <summary>
        /// Checks if an item exists by its composite identifier (ID and SubID).
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>True if the item exists; otherwise false.</returns>
        public async Task<bool> ItemExistsAsync(int id, int subId)
        {
            return await _context.Items.AnyAsync(e => e.Id == id && e.SubId == subId);
        }

        /// <summary>
        /// Retrieves an item entity by composite ID eagerly loading suppliers, units, and expiry dates.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <param name="subId">The sub-identifier of the item.</param>
        /// <returns>Item entity with relations if found; otherwise null.</returns>
        public async Task<Item?> GetByIdWithSuppliersAsync(int id, int subId)
        {
            // Eagerly loads the related Suppliers data
            return await _context.Items
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Units)
                .FirstOrDefaultAsync(i => i.Id == id && i.SubId == subId);
        }

        /// <summary>
        /// Retrieves all sub-variants of an item family by main ID projected as response DTOs.
        /// </summary>
        /// <param name="id">The main identifier of the item.</param>
        /// <returns>Collection of ItemResDto.</returns>
        public async Task<IEnumerable<ItemResDto>> GetByMainIdAsync(int id)
        {
            // Avoid eager loading (Include). Build a filtered IQueryable and project in makeItemResponceDto.
            var query = _context.Items
                .Where(i => i.Id == id)
                .AsQueryable();

            return await makeItemResponceDto(_context, query);
        }

        /// <summary>
        /// Retrieves items matching a specific barcode projected as response DTOs.
        /// </summary>
        /// <param name="barCode">The barcode string to search for.</param>
        /// <returns>Collection of matching ItemResDto.</returns>
        public async Task<IEnumerable<ItemResDto>> GetByBarCodeAsync(string barCode)
        {
            // Avoid eager loading (Include). Build a filtered IQueryable and project in makeItemResponceDto.
            var query = _context.Items
                .Where(i => i.BarCode == barCode)
                .AsQueryable();

            return await makeItemResponceDto(_context, query);
        }

        /// <summary>
        /// Retrieves an item entity by its unique identifier (UUID) with suppliers, units, and expiry dates.
        /// </summary>
        /// <param name="uuid">The unique identifier (UUID) of the item.</param>
        /// <returns>Item entity if found; otherwise null.</returns>
        public async Task<Item?> GetByUuidAsync(string uuid)
        {
            return await _context.Items
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Units)
                .FirstOrDefaultAsync(i => i.Uuid == uuid);
        }

        /// <summary>
        /// Retrieves an item response DTO by its unique identifier (UUID).
        /// </summary>
        /// <param name="uuid">The unique identifier (UUID) of the item.</param>
        /// <returns>ItemResDto if found; otherwise null.</returns>
        public async Task<ItemResDto?> GetResDtoByUuidAsync(string uuid)
        {
            var query = _context.Items.Where(i => i.Uuid == uuid);
            var result = await makeItemResponceDto(_context, query);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a collection of item entities matching a list of item UUIDs.
        /// </summary>
        /// <param name="uuids">Collection of item UUID strings.</param>
        /// <returns>Collection of Item entities.</returns>
        public async Task<IEnumerable<Item>> GetByUuidsAsync(IEnumerable<string> uuids)
        {
            var uuidList = uuids.ToList();
            return await _context.Items
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Units)
                .Where(i => uuidList.Contains(i.Uuid))
                .ToListAsync();
        }

        /// <summary>
        /// Gets all items that are supplied by the specified supplier ID.
        /// </summary>
        public async Task<IEnumerable<Item>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.Items
                .Include(i => i.ExpDates)
                .Where(i => i.ItemSuppliers.Any(isu => isu.SuppliersId == supplierId))
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Units)
                .ToListAsync();
        }

        /// <summary>
        /// Searches items by name, print name, barcode, ID, or UUID, returning the top 10 matches projected as response DTOs.
        /// </summary>
        /// <param name="searchTerm">The search term text.</param>
        /// <returns>Collection of matching ItemResDto.</returns>
        public async Task<IEnumerable<ItemResDto>> GetBySearchAsync(string searchTerm)
        {
            var query = _context.Items
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                var isItemId = int.TryParse(searchTerm, out var itemId);

                // Search across item id, name, print name, barcode, or UUID
                query = query.Where(i =>
                    (isItemId && i.Id == itemId) ||
                    i.Name.Contains(searchTerm) ||
                    i.PrintName.Contains(searchTerm) ||
                    (i.BarCode != null && i.BarCode.Contains(searchTerm)) ||
                    i.Uuid.Contains(searchTerm)
                );
            }

            // Order by ID and SubID and take top 10 matches for fast auto-complete
            query = query
                .OrderBy(i => i.Id)
                .ThenBy(i => i.SubId)
                .Take(10);

            return await makeItemResponceDto(_context, query);
        }

        /// <summary>
        /// Transforms an <see cref="IQueryable{Item}"/> into a list of <see cref="ItemResDto"/>, 
        /// performing explicit left joins to retrieve creator and updater names.
        /// </summary>
        /// <param name="db">The database context used to join against the Users table.</param>
        /// <param name="query">The filtered or unfiltered base query of Items to be projected.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of populated <see cref="ItemResDto"/> objects.
        /// </returns>
        private async Task<List<ItemResDto>> makeItemResponceDto(AppDbContext db, IQueryable<Item> query)
        {
            // Project items into DTO with dynamic stock sum, packaging unit resolution, and user full names
            var q = from i in query
                    select new ItemResDto
                    {
                        Id                      = i.Id,
                        SubId                   = i.SubId,
                        Name                    = i.Name,
                        PrintName               = i.PrintName,
                        BarCode                 = i.BarCode,
                        Description             = i.Description,
                        AllowsDecimalQuantities = i.AllowsDecimalQuantities,

                        Inventory = new InventoryResDto
                        {
                            ItemUuid                = i.Uuid,
                            // Compute total remaining stock across all active batches
                            StockQuantity           = db.InventoryBatches.Where(b => b.ItemUuid == i.Uuid && b.IsActive).Sum(b => (decimal?)b.RemainingQuantity) ?? 0m,
                            BatchCount              = db.InventoryBatches.Count(b => b.ItemUuid == i.Uuid && b.IsActive),
                            AllowsDecimalQuantities = i.AllowsDecimalQuantities,
                            // Resolve base unit type falling back to smallest packaging unit or Each
                            UnitType                = i.Units.Where(u => u.IsBaseUnit).Select(u => u.UnitType).FirstOrDefault() != UnitType.None
                                                        ? i.Units.Where(u => u.IsBaseUnit).Select(u => u.UnitType).FirstOrDefault()
                                                        : (i.Units.OrderBy(u => u.QuantityInBaseUnits).Select(u => u.UnitType).FirstOrDefault() != UnitType.None
                                                        ? i.Units.OrderBy(u => u.QuantityInBaseUnits).Select(u => u.UnitType).FirstOrDefault()
                                                        : UnitType.Each),
                            Units                   = i.Units.Select(u => new InventoryUnitResDto
                            {
                                UnitType            = u.UnitType,
                                ParentUnitType      = u.ParentUnitType ?? u.UnitType,
                                QuantityPerParent   = u.QuantityPerParent ?? 1m,
                                QuantityInBaseUnits = u.QuantityInBaseUnits,
                                IsBaseUnit          = u.IsBaseUnit,
                                Uuid                = u.Uuid,
                                IsActive            = true
                            }).ToList(),
                            Uuid                    = i.Uuid,
                            CreatedAt               = i.CreatedAt,
                            UpdatedAt               = i.UpdatedAt,
                            // Lookup creator and updater user names
                            CreatedBy               = db.Users.Where(user => user.Uuid == i.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.CreatedBy,
                            UpdatedBy               = db.Users.Where(user => user.Uuid == i.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.UpdatedBy,
                            IsActive                = i.IsActive
                        },

                        // Pick the active batch with stock or latest created batch for current pricing display
                        Price = db.InventoryBatches
                            .Where(b => b.ItemUuid == i.Uuid && b.IsActive)
                            .OrderByDescending(b => b.Status == BatchStatus.Active)
                            .ThenByDescending(b => b.RemainingQuantity > 0)
                            .ThenByDescending(b => b.CreatedAt)
                            .Select(b => new ItemPriceResDto
                            {
                                BuyingPrice            = b.CostPrice,
                                MarkedPrice            = b.MarkedPrice,
                                RetailPrice            = b.RetailPrice,
                                WholesalePrice         = b.WholesalePrice,
                                RetailDiscountRatio    = b.RetailDiscountRatio,
                                WholesaleDiscountRatio = b.WholesaleDiscountRatio,
                                Uuid                   = b.Uuid,
                                CreatedAt              = b.CreatedAt,
                                UpdatedAt              = b.UpdatedAt,
                                CreatedBy              = db.Users.Where(user => user.Uuid == b.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? b.CreatedBy,
                                UpdatedBy              = db.Users.Where(user => user.Uuid == b.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? b.UpdatedBy,
                                IsActive               = b.IsActive
                            })
                            .FirstOrDefault() ?? new ItemPriceResDto(),

                        ExpDates = i.ExpDates.Where(ed => ed.IsActive).Select(ed => new ItemExpiryResDto
                        {
                            ExpDate          = ed.ExpDate,
                            NotifyBeforeDays = ed.NotifyBeforeDays,
                            Uuid             = ed.Uuid,
                            CreatedAt        = ed.CreatedAt,
                            UpdatedAt        = ed.UpdatedAt,
                            CreatedBy        = db.Users.Where(user => user.Uuid == ed.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? ed.CreatedBy,
                            UpdatedBy        = db.Users.Where(user => user.Uuid == ed.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? ed.UpdatedBy,
                            IsActive         = ed.IsActive
                        }).ToList(),

                        Suppliers = i.ItemSuppliers.Select(isu => new SupplierResDto
                        {
                            Id        = isu.Supplier.Id,
                            Name      = isu.Supplier.Name,
                            Uuid      = isu.Supplier.Uuid,
                            Address   = isu.Supplier.Address,
                            CreatedAt = isu.Supplier.CreatedAt,
                            UpdatedAt = isu.Supplier.UpdatedAt,
                            CreatedBy = db.Users.Where(user => user.Uuid == isu.Supplier.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? isu.Supplier.CreatedBy,
                            UpdatedBy = db.Users.Where(user => user.Uuid == isu.Supplier.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? isu.Supplier.UpdatedBy,
                            IsActive  = isu.Supplier.IsActive,
                        }).ToList(),

                        BatchCount= db.InventoryBatches.Count(b => b.ItemUuid == i.Uuid && b.IsActive),
                        Uuid      = i.Uuid,
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt,
                        CreatedBy = db.Users.Where(user => user.Uuid == i.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.CreatedBy,
                        UpdatedBy = db.Users.Where(user => user.Uuid == i.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.UpdatedBy,
                        IsActive  = i.IsActive,
                    };

            return await q.AsSplitQuery().ToListAsync();
        }

        /// <summary>
        /// Permanently deletes a collection of item expiry records by their unique identifiers.
        /// </summary>
        /// <param name="expiryUuids">The collection of expiry UUIDs to permanently delete.</param>
        /// <returns>The number of expiry records deleted.</returns>
        public async Task<int> DeleteExpiriesAsync(IEnumerable<string> expiryUuids)
        {
            if (expiryUuids == null || !expiryUuids.Any())
                return 0;

            var uuidList = expiryUuids.Distinct().ToList();
            var expiries = await _context.Set<ItemExpiry>()
                .Where(e => uuidList.Contains(e.Uuid))
                .ToListAsync();

            if (!expiries.Any())
                return 0;

            _context.Set<ItemExpiry>().RemoveRange(expiries);
            return await _context.SaveChangesAsync();
        }
    }
}
