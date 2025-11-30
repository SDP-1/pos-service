using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Contact;
using pos_service.Models.Enums;
using pos_service.Services;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for managing contact entities in the POS system.
    /// Provides CRUD operations for contact information with administrative access control.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.AllAdmins)]
    public class ContactsController : SystemBaseController
    {
        private readonly IContactService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactsController"/> class.
        /// </summary>
        /// <param name="service">The contact service for business logic operations.</param>
        /// <param name="currentUserService">The current user service for authentication context.</param>
        public ContactsController(IContactService service, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all contacts from the system.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a list of all contacts.
        /// Returns 200 OK with the contact list on success.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllContactsAsync(_currentUser));

        /// <summary>
        /// Retrieves a specific contact by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the contact details.
        /// Returns 200 OK with contact data if found, 404 Not Found if contact doesn't exist.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contact = await _service.GetContactByIdAsync(id, _currentUser);
            return contact == null ? NotFound() : Ok(contact);
        }

        /// <summary>
        /// Creates a new contact in the system.
        /// </summary>
        /// <param name="dto">The contact data transfer object containing contact information.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the created contact.
        /// Returns 201 Created with the new contact data and location header.
        /// </returns>
        /// <response code="201">Returns the newly created contact.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="401">If user is not authenticated.</response>
        /// <response code="403">If user does not have admin role.</response>
        [HttpPost]
        public async Task<IActionResult> Create(ContactReqDto dto)
        {
            var newContact = await _service.CreateContactAsync(dto, _currentUser);
            return CreatedAtAction(nameof(GetById), new { id = newContact.Id }, newContact);
        }

        /// <summary>
        /// Updates an existing contact with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to update.</param>
        /// <param name="dto">The contact data transfer object containing updated contact information.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the operation.
        /// Returns 204 No Content on success, 404 Not Found if contact doesn't exist.
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ContactReqDto dto)
        {
            var success = await _service.UpdateContactAsync(id, dto, _currentUser);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a contact with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to delete.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the operation.
        /// Returns 204 No Content on success, 404 Not Found if contact doesn't exist.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteContactAsync(id, _currentUser);
            return success ? NoContent() : NotFound();
        }
    }
}