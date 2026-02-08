using pos_service.Models;
using pos_service.Models.DTO.Contacts;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public interface IContactService
    {
        /// <summary>
        /// Retrieves a specific contact by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact.</param>
        /// <param name="currentUser">The current user requesting the contact.</param>
        /// <returns>The contact details if found, otherwise null.</returns>
        Task<ContactResDto?> GetContactByIdAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Retrieves all contacts from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the contacts.</param>
        /// <returns>A list of all contact details.</returns>
        Task<IEnumerable<ContactResDto>> GetAllContactsAsync(CurrentUser currentUser);

        /// <summary>
        /// Creates a new contact in the system.
        /// </summary>
        /// <param name="dto">The contact data transfer object containing contact information.</param>
        /// <param name="currentUser">The current user creating the contact.</param>
        /// <returns>The newly created contact details.</returns>
        Task<ContactResDto> CreateContactAsync(ContactReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing contact with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to update.</param>
        /// <param name="dto">The contact data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user updating the contact.</param>
        /// <returns>True if update was successful, otherwise false.</returns>
        Task<bool> UpdateContactAsync(int id, ContactReqDto dto, CurrentUser currentUser);

        /// <summary>
        /// Deletes a contact with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to delete.</param>
        /// <param name="currentUser">The current user deleting the contact.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteContactAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Merges incoming contact DTOs with existing contacts for a specified owner (User or Supplier).
        /// Updates existing contacts (identified by uuid), adds new ones, and deletes removed ones.
        /// </summary>
        /// <param name="ownerType">The type of entity that owns the contacts (User or Supplier).</param>
        /// <param name="ownerId">The ID of the owner entity (UserId or SupplierId).</param>
        /// <param name="incomingContacts">The list of contacts from the request (null means delete all).</param>
        Task MergeContactsAsync(ContactOwnerType ownerType, int ownerId, IEnumerable<ContactReqDto>? incomingContacts);
    }
}