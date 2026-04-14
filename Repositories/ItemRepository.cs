using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using System.Collections.Immutable;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.DTO.Contacts;
using System.Linq;

namespace pos_service.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItemResDto?> GetByIdAsync(int id, int subId)
        {
            var query = _context.Items
                        .Where(i => i.Id == id && i.SubId == subId);

            var result = await makeItemResponceDto(_context, query);

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<ItemResDto>> GetAllAsync()
        {
            var query = _context.Items.AsQueryable();

            return await makeItemResponceDto(_context, query);
        }

        public async Task<Item> AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<int> GetNextMainIdAsync()
        {
            var maxId = await _context.Items.MaxAsync(i => (int?)i.Id) ?? 0;
            return maxId + 1;
        }

        public async Task<int> GetNextSubIdAsync(int mainId)
        {
            var maxSubId = await _context.Items.Where(i => i.Id == mainId).MaxAsync(i => (int?)i.SubId) ?? -1;
            return maxSubId + 1;
        }

        public async Task<Item> UpdateAsync(Item item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return item;
        }

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

        public async Task<bool> ItemExistsAsync(int id, int subId)
        {
            return await _context.Items.AnyAsync(e => e.Id == id && e.SubId == subId);
        }

        public async Task<Item?> GetByIdWithSuppliersAsync(int id, int subId)
        {
            // Eagerly loads the related Suppliers data
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Inventory)
                    .ThenInclude(u => u.Units)
                .FirstOrDefaultAsync(i => i.Id == id && i.SubId == subId);
        }

        public async Task<IEnumerable<ItemResDto>> GetByMainIdAsync(int id)
        {
            // Avoid eager loading (Include). Build a filtered IQueryable and project in makeItemResponceDto.
            var query = _context.Items
                .Where(i => i.Id == id)
                .AsQueryable();

            return await makeItemResponceDto(_context, query);
        }

        public async Task<IEnumerable<ItemResDto>> GetByBarCodeAsync(string barCode)
        {
            // Avoid eager loading (Include). Build a filtered IQueryable and project in makeItemResponceDto.
            var query = _context.Items
                .Where(i => i.BarCode == barCode)
                .AsQueryable();

            return await makeItemResponceDto(_context, query);
        }

        public async Task<Item?> GetByUuidAsync(string uuid)
        {
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Include(i => i.Inventory)
                    .ThenInclude(u => u.Units)
                .FirstOrDefaultAsync(i => i.Uuid == uuid);
        }

        public async Task<IEnumerable<Item>> GetByUuidsAsync(IEnumerable<string> uuids)
        {
            var uuidList = uuids.ToList();
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .Where(i => uuidList.Contains(i.Uuid))
                .ToListAsync();
        }

        /// <summary>
        /// Gets all items that are supplied by the specified supplier ID.
        /// </summary>
        public async Task<IEnumerable<Item>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Where(i => i.ItemSuppliers.Any(isu => isu.SuppliersId == supplierId))
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemResDto>> GetBySearchAsync(string searchTerm)
        {
            var query = _context.Items
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                var isItemId = int.TryParse(searchTerm, out var itemId);

                query = query.Where(i =>
                    (isItemId && i.Id == itemId) ||
                    i.Name.Contains(searchTerm) ||
                    i.PrintName.Contains(searchTerm) ||
                    (i.BarCode != null && i.BarCode.Contains(searchTerm)) ||
                    i.Uuid.Contains(searchTerm)
                );
            }

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
            // Use explicit left joins to Users to obtain CreatedBy/UpdatedBy user names.
            var q = from i in query
                    select new ItemResDto
                    {
                        Id                      = i.Id,
                        SubId                   = i.SubId,
                        Name                    = i.Name,
                        PrintName               = i.PrintName,
                        BarCode                 = i.BarCode,

                        Inventory = i.Inventory != null ? new InventoryResDto
                        {
                            ItemUuid                = i.Inventory.ItemUuid,
                            StockQuantity           = i.Inventory.StockQuantity,
                            AllowsDecimalQuantities = i.Inventory.AllowsDecimalQuantities,
                            UnitType                = i.Inventory.UnitType,
                            Units                   = i.Inventory.Units.Select(u => new InventoryUnitResDto
                            {
                                UnitType            = u.UnitType,
                                ParentUnitType      = u.ParentUnitType,
                                QuantityPerParent   = u.QuantityPerParent,
                                QuantityInBaseUnits = u.QuantityInBaseUnits,
                                Uuid                = u.Uuid,
                                CreatedAt           = u.CreatedAt,
                                UpdatedAt           = u.UpdatedAt,
                                CreatedBy           = db.Users.Where(user => user.Uuid == u.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? u.CreatedBy,
                                UpdatedBy           = db.Users.Where(user => user.Uuid == u.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? u.UpdatedBy,
                                IsActive            = u.IsActive
                            }).ToList(),
                            Uuid                    = i.Inventory.Uuid,
                            CreatedAt               = i.Inventory.CreatedAt,
                            UpdatedAt               = i.Inventory.UpdatedAt,
                            CreatedBy               = db.Users.Where(user => user.Uuid == i.Inventory.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.Inventory.CreatedBy,
                            UpdatedBy               = db.Users.Where(user => user.Uuid == i.Inventory.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.Inventory.UpdatedBy,
                            IsActive                = i.Inventory.IsActive
                        } : null,

                        Price = i.Price != null ? new ItemPriceResDto
                        {
                            BuyingPrice            = i.Price.BuyingPrice,
                            MarkedPrice            = i.Price.MarkedPrice,
                            RetailPrice            = i.Price.RetailPrice,
                            WholesalePrice         = i.Price.WholesalePrice,
                            RetailDiscountRatio    = i.Price.RetailDiscountRatio,
                            WholesaleDiscountRatio = i.Price.WholesaleDiscountRatio,
                            Uuid                   = i.Price.Uuid,
                            CreatedAt              = i.Price.CreatedAt,
                            UpdatedAt              = i.Price.UpdatedAt,
                            CreatedBy              = db.Users.Where(user => user.Uuid == i.Price.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.Price.CreatedBy,
                            UpdatedBy              = db.Users.Where(user => user.Uuid == i.Price.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.Price.UpdatedBy,
                            IsActive               = i.Price.IsActive
                        } : new ItemPriceResDto(),

                        ExpDates = i.ExpDates.Select(ed => new ItemExpiryResDto
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

                        Uuid      = i.Uuid,
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt,
                        CreatedBy = db.Users.Where(user => user.Uuid == i.CreatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.CreatedBy,
                        UpdatedBy = db.Users.Where(user => user.Uuid == i.UpdatedBy).Select(user => user.FullName).FirstOrDefault() ?? i.UpdatedBy,
                        IsActive  = i.IsActive,
                    };

            return await q.AsSplitQuery().ToListAsync();
        }
    }
}
