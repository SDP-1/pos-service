using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _context;
        /// <summary>
        /// Initializes a new instance of the ContactRepository.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        public ContactRepository(AppDbContext context) { _context = context; }

        /// <summary>
        /// Retrieves a specific contact by its unique identifier.
        /// </summary>
        /// <param name="id">Database id of the contact.</param>
        /// <returns>The contact entity if found, otherwise null.</returns>
        public async Task<Contact?> GetByIdAsync(int id) => await _context.Contacts.FindAsync(id);

        /// <summary>
        /// Retrieves all contacts from the data store.
        /// </summary>
        /// <returns>A list of all contact entities.</returns>
        public async Task<IEnumerable<Contact>> GetAllAsync() => await _context.Contacts.ToListAsync();

        /// <summary>
        /// Retrieves all contacts for a specific supplier.
        /// </summary>
        /// <param name="supplierId">The supplier ID to retrieve contacts for.</param>
        /// <returns>A list of contact entities for the supplier.</returns>
        public async Task<IEnumerable<Contact>> GetContactsBySupplierId(int supplierId)
        {
            return await _context.Contacts
                .Where(c => c.SupplierId == supplierId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all contacts for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to retrieve contacts for.</param>
        /// <returns>A list of contact entities for the user.</returns>
        public async Task<IEnumerable<Contact>> GetContactsByUserId(int userId)
        {
            return await _context.Contacts
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new contact to the data store and assigns a UUID.
        /// </summary>
        /// <param name="contact">The contact entity to add.</param>
        /// <returns>The added contact entity with updated identifiers.</returns>
        public async Task<Contact> AddAsync(Contact contact)
        {
            contact.Uuid = Guid.NewGuid().ToString();
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        /// <summary>
        /// Updates an existing contact in the data store.
        /// </summary>
        /// <param name="contact">The contact entity with updated information.</param>
        /// <returns>The updated contact entity.</returns>
        public async Task<Contact> UpdateAsync(Contact contact)
        {
            _context.Entry(contact).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contact;
        }

        /// <summary>
        /// Deletes a contact from the data store.
        /// </summary>
        /// <param name="contact">The contact entity to delete.</param>
        public async Task DeleteAsync(Contact contact)
        {
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all contacts that reference the given supplier id using a set-based DB operation.
        /// </summary>
        /// <param name="supplierId">Supplier id whose contacts should be deleted.</param>
        public async Task DeleteBySupplierIdAsync(int supplierId)
        {
            // Use EF Core set-based delete to avoid loading rows into memory
            await _context.Contacts.Where(c => c.SupplierId == supplierId).ExecuteDeleteAsync();
        }

        /// <summary>
        /// Deletes all contacts that reference the given user id using a set-based DB operation.
        /// </summary>
        /// <param name="userId">User id whose contacts should be deleted.</param>
        public async Task DeleteByUserIdAsync(int userId)
        {
            // Use EF Core set-based delete to avoid loading rows into memory
            await _context.Contacts.Where(c => c.UserId == userId).ExecuteDeleteAsync();
        }

        /// <summary>
        /// Adds multiple contacts in a single operation.
        /// </summary>
        /// <param name="contacts">The collection of contact entities to add.</param>
        public async Task AddRangeAsync(IEnumerable<Contact> contacts)
        {
            await _context.Contacts.AddRangeAsync(contacts);
            await _context.SaveChangesAsync();
        }
    }
}
