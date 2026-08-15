using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Contacts;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Suppliers;

namespace pos_service.Repositories
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        public SupplierRepository(AppDbContext context) : base(context) { }

        public async Task<Supplier> SaveNewSupplierAsync(Supplier supplier, IEnumerable<ItemSupplier> itemSuppliers)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                if (itemSuppliers != null && itemSuppliers.Any())
                {
                    foreach (var isu in itemSuppliers)
                    {
                        isu.SuppliersId = supplier.Id;
                    }
                    _context.ItemSuppliers.AddRange(itemSuppliers);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return supplier;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SaveUpdatedSupplierAsync(Supplier supplier)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SupplierResDto?> GetByIdAsync(int id)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id);

            return await makeSupplierResponceDto(query, includeRelated: false)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SupplierResDto>> GetAllAsync()
            {
                var query = _context.Suppliers
                    .AsNoTracking();

                return await makeSupplierResponceDto(query, includeRelated: true)
                    .ToListAsync();
        }

        public async Task<IEnumerable<SupplierResDto>> GetAllBasicAsync()
        {
            var query = _context.Suppliers
                .AsNoTracking();

            return await makeSupplierResponceDto(query, includeRelated: false)
                .ToListAsync();
        }

        public async Task<SupplierResDto?> GetByIdWithDetailsAsync(int id)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id);

            return await makeSupplierResponceDto(query, includeRelated: true)
                .FirstOrDefaultAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SupplierResDto?> GetSupplierWithItemsAsync(int id)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id);

            return await makeSupplierResponceDto(query, includeRelated: true)
                .FirstOrDefaultAsync();
        }

        public async Task<SupplierResDto?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Name == name);

            return await makeSupplierResponceDto(query, includeRelated: false)
                .FirstOrDefaultAsync();
        }
        /// <summary>
        /// Adds a new supplier to the data store and assigns a UUID.
        /// </summary>
        /// <param name="supplier">Supplier entity to add.</param>
        /// <returns>The added Supplier entity.</returns>
        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            supplier.Uuid = Guid.NewGuid().ToString();
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }
        /// <summary>
        /// Updates an existing supplier in the database.
        /// </summary>
        /// <param name="supplier">Supplier entity with updated values.</param>
        /// <returns>The updated Supplier entity.</returns>
        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task DeleteItemAssociationsBySupplierId(int supplierId)
        {
            await _context.ItemSuppliers
                .Where(i => i.SuppliersId == supplierId)
                .ExecuteDeleteAsync();
        }

        public async Task AddItemAssociationsAsync(IEnumerable<ItemSupplier> itemSuppliers)
        {
            await _context.ItemSuppliers.AddRangeAsync(itemSuppliers);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Supplier supplier)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }

        private IQueryable<SupplierResDto> makeSupplierResponceDto(IQueryable<Supplier> query, bool includeRelated)
        {
            return query.Select(s => new SupplierResDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                contacts = includeRelated
                    ? s.Contacts.Select(c => new ContactResDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Designation = c.Designation,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        UserId = c.UserId,
                        SupplierId = c.SupplierId,
                        Uuid = c.Uuid,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        CreatedBy = c.CreatedBy,
                        UpdatedBy = c.UpdatedBy,
                        IsActive = c.IsActive,
                    }).ToList()
                    : null,
                Items = includeRelated
                    ? s.ItemSuppliers.Select(isu => new ItemMiniResDto
                    {
                        Id = isu.Item.Id,
                        SubId = isu.Item.SubId,
                        Uuid = isu.Item.Uuid,
                        Name = isu.Item.Name,
                        PrintName = isu.Item.PrintName,
                        BarCode = isu.Item.BarCode,
                        AllowsDecimalQuantities = isu.Item.Inventory != null && isu.Item.Inventory.AllowsDecimalQuantities,
                        UnitType = isu.Item.Inventory != null ? isu.Item.Inventory.UnitType : Models.Enums.UnitType.Each,
                        Price = new ItemPriceResDto(),
                        ExpDates = new List<ItemExpiryResDto>(),
                    }).ToList()
                    : null,
                Uuid = s.Uuid,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedBy = s.UpdatedBy,
                IsActive = s.IsActive,
            });
        }
    }
}
