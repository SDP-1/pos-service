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
    }
}