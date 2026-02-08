using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IContactRepository
    {
        /// <summary>
        /// Retrieves a specific contact by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact.</param>
        /// <returns>The contact entity if found, otherwise null.</returns>
        Task<Contact?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all contacts from the data store.
        /// </summary>
        /// <returns>A list of all contact entities.</returns>
        Task<IEnumerable<Contact>> GetAllAsync();

        /// <summary>
        /// Retrieves all contacts for a specific supplier.
        /// </summary>
        /// <param name="supplierId">The supplier ID to retrieve contacts for.</param>
        /// <returns>A list of contact entities for the supplier.</returns>
        Task<IEnumerable<Contact>> GetContactsBySupplierId(int supplierId);

        /// <summary>
        /// Retrieves all contacts for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to retrieve contacts for.</param>
        /// <returns>A list of contact entities for the user.</returns>
        Task<IEnumerable<Contact>> GetContactsByUserId(int userId);

        /// <summary>
        /// Adds a new contact to the data store.
        /// </summary>
        /// <param name="contact">The contact entity to add.</param>
        /// <returns>The added contact entity with updated identifiers.</returns>
        Task<Contact> AddAsync(Contact contact);

        /// <summary>
        /// Updates an existing contact in the data store.
        /// </summary>
        /// <param name="contact">The contact entity with updated information.</param>
        /// <returns>The updated contact entity.</returns>
        Task<Contact> UpdateAsync(Contact contact);

        /// <summary>
        /// Deletes a contact from the data store.
        /// </summary>
        /// <param name="contact">The contact entity to delete.</param>
        Task DeleteAsync(Contact contact);

        /// <summary>
        /// Delete all contacts that reference the given supplier id using a set-based DB operation.
        /// </summary>
        Task DeleteBySupplierIdAsync(int supplierId);

        /// <summary>
        /// Delete all contacts that reference the given user id using a set-based DB operation.
        /// </summary>
        Task DeleteByUserIdAsync(int userId);

        /// <summary>
        /// Add multiple contacts in a single operation.
        /// </summary>
        /// <param name="contacts">The collection of contact entities to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<Contact> contacts);
    }
}