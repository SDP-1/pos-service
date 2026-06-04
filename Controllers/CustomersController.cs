using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Authorization;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Customers;
using pos_service.Models.Enums;
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
        /// <summary>
        /// Retrieves all customers visible to the current user.
        /// </summary>
        /// <returns>200 OK with the list of customers.</returns>
        [Permission(PermissionType.CUSTOMER_VIEW)]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllCustomersAsync(_currentUser));

        [HttpGet("search")]
        /// <summary>
        /// Searches customers by a free text term.
        /// </summary>
        /// <param name="searchTerm">Term to search customers by (name, phone, etc.).</param>
        /// <returns>200 OK with matching customers.</returns>
        [Permission(PermissionType.CUSTOMER_VIEW)]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
            => Ok(await _service.GetCustomersBySearchAsync(searchTerm, _currentUser));

        [HttpGet("{id:int}")]
        /// <summary>
        /// Gets a single customer by id.
        /// </summary>
        /// <param name="id">Customer identifier.</param>
        /// <returns>200 OK with customer or 404 NotFound.</returns>
        [Permission(PermissionType.CUSTOMER_VIEW)]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _service.GetCustomerByIdAsync(id, _currentUser);
            return customer == null ? NotFound() : Ok(customer);
        }

        [HttpPost]
        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="dto">Customer request DTO containing customer details.</param>
        /// <returns>201 Created with the new customer.</returns>
        [Permission(PermissionType.CUSTOMER_CREATE)]
        public async Task<IActionResult> Create(CustomerReqDto dto)
        {
            var newCust = await _service.CreateCustomerAsync(dto, _currentUser);
            return CreatedAtAction(nameof(GetById), new { id = newCust.Id }, newCust);
        }

        [HttpPut("{id}")]
        /// <summary>
        /// Updates an existing customer by id.
        /// </summary>
        /// <param name="id">Identifier of the customer to update.</param>
        /// <param name="dto">Customer request DTO with updated values.</param>
        /// <returns>204 NoContent on success or 404 NotFound.</returns>
        [Permission(PermissionType.CUSTOMER_UPDATE)]
        public async Task<IActionResult> Update(int id, CustomerReqDto dto)
        {
            var success = await _service.UpdateCustomerAsync(id, dto, _currentUser);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        /// <summary>
        /// Deletes a customer by id.
        /// </summary>
        /// <param name="id">Identifier of the customer to delete.</param>
        /// <returns>204 NoContent on success or 404 NotFound.</returns>
        [Permission(PermissionType.CUSTOMER_DELETE)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteCustomerAsync(id, _currentUser);
            return success ? NoContent() : NotFound();
        }
    }
}
