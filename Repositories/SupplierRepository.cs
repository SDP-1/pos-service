using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.DTO.Contacts;
using pos_service.Models.DTO.Items;
using pos_service.Models.DTO.Suppliers;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupplierRepository"/> class with the database context.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public SupplierRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Adds a new supplier along with item associations inside a database transaction.
        /// </summary>
        /// <param name="supplier">The supplier entity to add.</param>
        /// <param name="itemSuppliers">Collection of item-to-supplier associations to insert.</param>
        /// <returns>The created Supplier entity.</returns>
        public async Task<Supplier> SaveNewSupplierAsync(Supplier supplier, IEnumerable<ItemSupplier> itemSuppliers)
        {
            // Atomically create supplier entity and its linked item supply catalog
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                // Assign generated supplier identity ID to all associated item supplier relations
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

        /// <summary>
        /// Updates an existing supplier within a database transaction.
        /// </summary>
        /// <param name="supplier">The supplier entity to update.</param>
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

        /// <summary>
        /// Retrieves a specific supplier by ID without related contacts and item lists.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>SupplierResDto if found; otherwise null.</returns>
        public async Task<SupplierResDto?> GetByIdAsync(int id)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id);

            return await makeSupplierResponceDto(query, includeRelated: false)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all suppliers including their contacts and supplied items.
        /// </summary>
        /// <returns>Collection of SupplierResDto.</returns>
        public async Task<IEnumerable<SupplierResDto>> GetAllAsync()
        {
            var query = _context.Suppliers
                .AsNoTracking();

            return await makeSupplierResponceDto(query, includeRelated: true)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all suppliers without loading contacts or supplied items (optimized for dropdown lists).
        /// </summary>
        /// <returns>Collection of basic SupplierResDto.</returns>
        public async Task<IEnumerable<SupplierResDto>> GetAllBasicAsync()
        {
            var query = _context.Suppliers
                .AsNoTracking();

            return await makeSupplierResponceDto(query, includeRelated: false)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a supplier by ID including contacts and supplied items.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>SupplierResDto with full details if found; otherwise null.</returns>
        public async Task<SupplierResDto?> GetByIdWithDetailsAsync(int id)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id);

            return await makeSupplierResponceDto(query, includeRelated: true)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves raw supplier entity by database ID.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>Supplier entity if found; otherwise null.</returns>
        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves a supplier and its supplied item catalog by supplier ID.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>SupplierResDto with item details if found; otherwise null.</returns>
        public async Task<SupplierResDto?> GetSupplierWithItemsAsync(int id)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id);

            return await makeSupplierResponceDto(query, includeRelated: true)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a supplier by name for uniqueness checking.
        /// </summary>
        /// <param name="name">The supplier name to search.</param>
        /// <returns>SupplierResDto if found; otherwise null.</returns>
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

        /// <summary>
        /// Removes all item-to-supplier associations for a given supplier ID.
        /// </summary>
        /// <param name="supplierId">The ID of the supplier.</param>
        public async Task DeleteItemAssociationsBySupplierId(int supplierId)
        {
            await _context.ItemSuppliers
                .Where(i => i.SuppliersId == supplierId)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// Adds multiple item-to-supplier link records to the database.
        /// </summary>
        /// <param name="itemSuppliers">Collection of ItemSupplier associations to insert.</param>
        public async Task AddItemAssociationsAsync(IEnumerable<ItemSupplier> itemSuppliers)
        {
            await _context.ItemSuppliers.AddRangeAsync(itemSuppliers);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a supplier entity from the database.
        /// </summary>
        /// <param name="supplier">The supplier entity to remove.</param>
        public async Task DeleteAsync(Supplier supplier)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }

        private IQueryable<SupplierResDto> makeSupplierResponceDto(IQueryable<Supplier> query, bool includeRelated)
        {
            return query.Select(s => new SupplierResDto
            {
                Id       = s.Id,
                Name     = s.Name,
                Address  = s.Address,
                contacts = includeRelated
                    ? s.Contacts.Select(c => new ContactResDto
                    {
                        Id          = c.Id,
                        Name        = c.Name,
                        Designation = c.Designation,
                        PhoneNumber = c.PhoneNumber,
                        Email       = c.Email,
                        UserId      = c.UserId,
                        SupplierId  = c.SupplierId,
                        Uuid        = c.Uuid,
                        CreatedAt   = c.CreatedAt,
                        UpdatedAt   = c.UpdatedAt,
                        CreatedBy   = c.CreatedBy,
                        UpdatedBy   = c.UpdatedBy,
                        IsActive    = c.IsActive,
                    }).ToList()
                    : null,
                Items = includeRelated
                    ? s.ItemSuppliers.Select(isu => new ItemMiniResDto
                    {
                        Id        = isu.Item.Id,
                        SubId     = isu.Item.SubId,
                        Uuid      = isu.Item.Uuid,
                        Name      = isu.Item.Name,
                        PrintName = isu.Item.PrintName,
                        AllowsDecimalQuantities = isu.Item.AllowsDecimalQuantities,
                        UnitType = isu.Item.Units.Where(u => u.IsBaseUnit).Select(u => u.UnitType).FirstOrDefault() != UnitType.None
                                    ? isu.Item.Units.Where(u => u.IsBaseUnit).Select(u => u.UnitType).FirstOrDefault()
                                    : (isu.Item.Units.OrderBy(u => u.QuantityInBaseUnits).Select(u => u.UnitType).FirstOrDefault() != UnitType.None
                                    ? isu.Item.Units.OrderBy(u => u.QuantityInBaseUnits).Select(u => u.UnitType).FirstOrDefault()
                                    : UnitType.Each),
                        Price    = new ItemPriceResDto(),
                        ExpDates = new List<ItemExpiryResDto>(),
                    }).ToList()
                    : null,
                Uuid      = s.Uuid,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedBy = s.UpdatedBy,
                IsActive  = s.IsActive,
            });
        }
    }
}
