using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Contacts;
using pos_service.Models.Enums;
using pos_service.Repositories;

namespace pos_service.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _repository;
        private readonly IMapper            _mapper;
        public ContactService(IContactRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper     = mapper;
        }

        /// <summary>
        /// Retrieves all contacts in the system.
        /// </summary>
        /// <param name="currentUser">Current user context (for auditing/authorization).</param>
        /// <returns>List of contact DTOs.</returns>
        public async Task<IEnumerable<ContactResDto>> GetAllContactsAsync(CurrentUser currentUser)
        {
            var contacts = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ContactResDto>>(contacts);
        }

        /// <summary>
        /// Retrieves a contact by id.
        /// </summary>
        /// <param name="id">Contact id.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>Contact DTO when found; otherwise null.</returns>
        public async Task<ContactResDto?> GetContactByIdAsync(int id, CurrentUser currentUser)
        {
            var contact = await _repository.GetByIdAsync(id);
            return _mapper.Map<ContactResDto?>(contact);
        }

        /// <summary>
        /// Creates a new contact record.
        /// </summary>
        /// <param name="dto">Contact creation DTO.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>Created contact DTO.</returns>
        public async Task<ContactResDto> CreateContactAsync(ContactReqDto dto, CurrentUser currentUser)
        {
            var contact = _mapper.Map<Contact>(dto);
            var newContact = await _repository.AddAsync(contact);
            return _mapper.Map<ContactResDto>(newContact);
        }

        /// <summary>
        /// Updates an existing contact by id.
        /// </summary>
        /// <param name="id">Contact id to update.</param>
        /// <param name="dto">Updated contact values.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>True on success; false when not found.</returns>
        public async Task<bool> UpdateContactAsync(int id, ContactReqDto dto, CurrentUser currentUser)
        {
            var contactToUpdate = await _repository.GetByIdAsync(id);
            if (contactToUpdate == null) return false;

            _mapper.Map(dto, contactToUpdate);
            await _repository.UpdateAsync(contactToUpdate);
            return true;
        }

        /// <summary>
        /// Deletes a contact by id.
        /// </summary>
        /// <param name="id">Contact id to delete.</param>
        /// <param name="currentUser">Current user context.</param>
        /// <returns>True when deleted; false when not found.</returns>
        public async Task<bool> DeleteContactAsync(int id, CurrentUser currentUser)
        {
            var contact = await _repository.GetByIdAsync(id);
            if (contact == null) return false;

            await _repository.DeleteAsync(contact);
            return true;
        }

        /// <summary>
        /// Merges incoming contact DTOs with existing contacts for a specified owner (User or Supplier):
        /// - Updates contacts that already exist (identified by uuid)
        /// - Adds new contacts (no uuid provided)
        /// - Deletes contacts not in the incoming list
        /// </summary>
        public async Task MergeContactsAsync(ContactOwnerType ownerType, int ownerId, IEnumerable<ContactReqDto>? incomingContacts)
        {
            // Fetch existing contacts for the owner
            var existingContacts = ownerType == ContactOwnerType.User
                ? await _repository.GetContactsByUserId(ownerId)
                : await _repository.GetContactsBySupplierId(ownerId);

            // If no incoming contacts provided, delete all existing ones
            if (incomingContacts == null || !incomingContacts.Any())
            {
                foreach (var contact in existingContacts)
                    await _repository.DeleteAsync(contact);
                return;
            }

            var incomingList = incomingContacts.ToList();

            // Identify contacts to update, add, and delete
            var existingDict  = existingContacts.ToDictionary(c => c.Uuid);
            var incomingUuids = new HashSet<string>(incomingList.Where(c => !string.IsNullOrEmpty(c.Uuid)).Select(c => c.Uuid));

            // Update existing contacts
            foreach (var incomingDto in incomingList.Where(c => !string.IsNullOrEmpty(c.Uuid)))
            {
                if (existingDict.TryGetValue(incomingDto.Uuid, out var existingContact))
                    await UpdateExistingContactAsync(existingContact, incomingDto);
            }

            // Add new contacts
            foreach (var incomingDto in incomingList.Where(c => string.IsNullOrEmpty(c.Uuid)))
            {
                await AddNewContactAsync(ownerType, ownerId, incomingDto);
            }

            // Delete contacts not in the incoming list
            foreach (var existingContact in existingContacts)
            {
                if (!incomingUuids.Contains(existingContact.Uuid))
                    await DeleteExistingContactAsync(existingContact);
            }
        }

        /// <summary>
        /// Updates an existing contact with new values from the DTO.
        /// </summary>
        private async Task UpdateExistingContactAsync(Contact contact, ContactReqDto dto)
        {
            contact.Name        = dto.Name;
            contact.Designation = dto.Designation;
            contact.PhoneNumber = dto.PhoneNumber;
            contact.Email       = dto.Email;
            contact.UserId      = dto.UserId;
            contact.IsActive    = dto.IsActive;

            await _repository.UpdateAsync(contact);
        }

        /// <summary>
        /// Adds a new contact with the specified owner type.
        /// </summary>
        private async Task AddNewContactAsync(ContactOwnerType ownerType, int ownerId, ContactReqDto dto)
        {
            var contact       = _mapper.Map<Contact>(dto);
            contact.Uuid      = Guid.NewGuid().ToString();
            contact.IsActive  = dto.IsActive;

            // Set the appropriate foreign key based on owner type
            if (ownerType == ContactOwnerType.User)
                contact.UserId = ownerId;
            else
                contact.SupplierId = ownerId;

            await _repository.AddAsync(contact);
        }

        /// <summary>
        /// Deletes a contact.
        /// </summary>
        private async Task DeleteExistingContactAsync(Contact contact)
        {
            await _repository.DeleteAsync(contact);
        }
    }
}
