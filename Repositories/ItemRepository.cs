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

        public async Task<IEnumerable<Item>> GetBySearchAsync(string searchTerm)
        {
            var query = _context.Items
                .Include(i => i.Price)
                .Include(i => i.ExpDates)
                .Include(i => i.ItemSuppliers)
                    .ThenInclude(isu => isu.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(i =>
                    i.Name.Contains(searchTerm) ||
                    i.PrintName.Contains(searchTerm) ||
                    (i.BarCode != null && i.BarCode.Contains(searchTerm)) ||
                    i.Uuid.Contains(searchTerm)
                );
            }

            return await query.ToListAsync();
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
                    join cu in db.Users on i.CreatedBy equals cu.Uuid into createdJoin
                    from createdUser in createdJoin.DefaultIfEmpty()
                    join uu in db.Users on i.UpdatedBy equals uu.Uuid into updatedJoin
                    from updatedUser in updatedJoin.DefaultIfEmpty()
                    select new ItemResDto
                    {
                        Id                      = i.Id,
                        SubId                   = i.SubId,
                        Name                    = i.Name,
                        PrintName               = i.PrintName,
                        BarCode                 = i.BarCode,

                        StockQuantity           = i.Inventory != null ? i.Inventory.StockQuantity : 0m,
                        AllowsDecimalQuantities = i.Inventory != null ? i.Inventory.AllowsDecimalQuantities : false,
                        UnitType                = i.Inventory != null ? i.Inventory.UnitType : default,
                        Units                   = i.Inventory != null ? i.Inventory.Units.Select(u => new InventoryUnitDto
                        {
                            UnitType            = u.UnitType,
                            ParentUnitType      = u.ParentUnitType,
                            QuantityPerParent   = u.QuantityPerParent,
                            QuantityInBaseUnits = u.QuantityInBaseUnits
                        }).ToList() : new List<InventoryUnitDto>(),

                        Price = i.Price != null ? new ItemPriceDto
                        {
                            BuyingPrice            = i.Price.BuyingPrice,
                            MarkedPrice            = i.Price.MarkedPrice,
                            RetailPrice            = i.Price.RetailPrice,
                            WholesalePrice         = i.Price.WholesalePrice,
                            RetailDiscountRatio    = i.Price.RetailDiscountRatio,
                            WholesaleDiscountRatio = i.Price.WholesaleDiscountRatio
                        } : new ItemPriceDto(),

                        ExpDates = i.ExpDates.Select(ed => new ItemExpiryDto
                        {
                            ExpDate          = ed.ExpDate,
                            NotifyBeforeDays = ed.NotifyBeforeDays
                        }).ToList(),

                        Suppliers = i.ItemSuppliers.Select(isu => new SupplierResDto
                        {
                            Id   = isu.Supplier.Id,
                            Name = isu.Supplier.Name,
                            Uuid = isu.Supplier.Uuid,
                        }).ToList(),

                        Uuid      = i.Uuid,
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt,
                        CreatedBy = createdUser.FullName,
                        UpdatedBy = updatedUser.FullName,
                        IsActive  = i.IsActive,
                    };

            return await q.AsSplitQuery().ToListAsync();
        }
    }
}
