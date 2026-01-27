using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;
        public SupplierRepository(AppDbContext context) { _context = context; }

        public async Task<Supplier?> GetByIdAsync(int id) => await _context.Suppliers.FindAsync(id);
        public async Task<IEnumerable<Supplier>> GetAllAsync()
            { 
               //return await _context.Suppliers.ToListAsync();
                return await _context.Suppliers
                    .Include(s => s.Contacts)
                    .Include(s => s.ItemSuppliers)
                        .ThenInclude(isu => isu.Item)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetAllBasicAsync()
        {
            // Do not include related navigation properties to keep payload small
            return await _context.Suppliers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdWithDetailsAsync(int id)
        {
            // Return as NoTracking to avoid attaching related entities to the context
            // This prevents "already being tracked" conflicts when callers later
            // perform set-based deletes and re-inserts in the same DbContext.
            return await _context.Suppliers
                .AsNoTracking()
                .Include(s => s.ItemSuppliers)
                    .ThenInclude(isu => isu.Item)
                .Include(s => s.Contacts)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Supplier?> GetSupplierWithItemsAsync(int id)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Include(s => s.ItemSuppliers)
                    .ThenInclude(isu => isu.Item)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Supplier?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name == name);
        }
        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            supplier.Uuid = Guid.NewGuid().ToString();
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }
        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return supplier;
        }
        public async Task<Supplier> UpdateWithAssociationsAsync(Supplier supplier, IEnumerable<Contact> contacts, IEnumerable<string> itemUuids)
        {
            // Use a transaction to ensure atomicity
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update supplier scalar fields
                var existing = await _context.Suppliers.FindAsync(supplier.Id);
                if (existing == null) throw new InvalidOperationException("Supplier not found");

                // Update scalar values
                existing.Name = supplier.Name;
                existing.Address = supplier.Address;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = supplier.UpdatedBy;
                existing.IsActive = supplier.IsActive;

                _context.Entry(existing).State = EntityState.Modified;

                // Replace contacts: delete existing and add new set
                await _context.Contacts.Where(c => c.SupplierId == existing.Id).ExecuteDeleteAsync();
                if (contacts != null && contacts.Any())
                {
                    foreach (var c in contacts) { c.SupplierId = existing.Id; }
                    await _context.Contacts.AddRangeAsync(contacts);
                }

                // Replace item associations: delete existing and recreate based on itemUuids
                await _context.ItemSuppliers.Where(i => i.SuppliersId == existing.Id).ExecuteDeleteAsync();
                if (itemUuids != null && itemUuids.Any())
                {
                    var items = await _context.Items.Where(it => itemUuids.Contains(it.Uuid)).ToListAsync();
                    var toAdd = new List<ItemSupplier>();
                    foreach (var it in items)
                    {
                        toAdd.Add(new ItemSupplier
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            SuppliersId = existing.Id,
                            ItemsId = it.Id,
                            ItemsSubId = it.SubId
                        });
                    }
                    if (toAdd.Any()) await _context.ItemSuppliers.AddRangeAsync(toAdd);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return existing;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task DeleteAsync(Supplier supplier)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }
    }
}
