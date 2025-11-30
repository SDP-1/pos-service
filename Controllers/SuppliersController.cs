using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Supplier;
using pos_service.Models.Enums;
using pos_service.Services;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for managing supplier entities in the POS system.
    /// Provides CRUD operations for supplier information with administrative access control.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.AllAdmins)]
    public class SuppliersController : SystemBaseController
    {
        private readonly ISupplierService _service;

        /// <summary>
        /// Initializes a new instance of the SuppliersController class.
        /// </summary>
        /// <param name="service">The supplier service for business logic operations.</param>
        /// <param name="currentUserService">The current user service for authentication context.</param>
        public SuppliersController(ISupplierService service, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all suppliers from the system.
        /// </summary>
        /// <returns>A list of all suppliers.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllSuppliersAsync(_currentUser));

        /// <summary>
        /// Retrieves a specific supplier by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier.</param>
        /// <returns>The supplier details if found, otherwise returns NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _service.GetSupplierByIdAsync(id, _currentUser);
            return supplier == null ? NotFound() : Ok(supplier);
        }

        /// <summary>
        /// Creates a new supplier in the system.
        /// </summary>
        /// <param name="dto">The supplier data transfer object containing supplier information.</param>
        /// <returns>The newly created supplier details with location header.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(SupplierReqDto dto)
        {
            var newSupplier = await _service.CreateSupplierAsync(dto, _currentUser);
            return CreatedAtAction(nameof(GetById), new { id = newSupplier.Id }, newSupplier);
        }

        /// <summary>
        /// Updates an existing supplier with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier to update.</param>
        /// <param name="dto">The supplier data transfer object containing updated information.</param>
        /// <returns>NoContent if successful, otherwise returns NotFound.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SupplierReqDto dto)
        {
            var success = await _service.UpdateSupplierAsync(id, dto, _currentUser);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Deletes a supplier with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the supplier to delete.</param>
        /// <returns>NoContent if successful, otherwise returns NotFound.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteSupplierAsync(id, _currentUser);
            return success ? NoContent() : NotFound();
        }
    }
}