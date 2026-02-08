using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Customers;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : SystemBaseController
    {
        private readonly ICustomerService _service;

        public CustomersController(ICustomerService service, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllCustomersAsync(_currentUser));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _service.GetCustomerByIdAsync(id, _currentUser);
            return customer == null ? NotFound() : Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerReqDto dto)
        {
            var newCust = await _service.CreateCustomerAsync(dto, _currentUser);
            return CreatedAtAction(nameof(GetById), new { id = newCust.Id }, newCust);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CustomerReqDto dto)
        {
            var success = await _service.UpdateCustomerAsync(id, dto, _currentUser);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteCustomerAsync(id, _currentUser);
            return success ? NoContent() : NotFound();
        }
    }
}
