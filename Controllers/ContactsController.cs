using Microsoft.AspNetCore.Mvc;
using pos_service.Models.DTO.Contact;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _service;
        public ContactsController(IContactService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllContactsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contact = await _service.GetContactByIdAsync(id);
            return contact == null ? NotFound() : Ok(contact);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactReqDto dto)
        {
            var newContact = await _service.CreateContactAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newContact.Id }, newContact);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ContactReqDto dto)
        {
            var success = await _service.UpdateContactAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteContactAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
