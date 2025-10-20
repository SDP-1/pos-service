using Microsoft.AspNetCore.Mvc;
using pos_service.Models.DTO;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;
        public SuppliersController(ISupplierService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllSuppliersAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _service.GetSupplierByIdAsync(id);
            return supplier == null ? NotFound() : Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SupplierReqDto dto)
        {
            var newSupplier = await _service.CreateSupplierAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newSupplier.Id }, newSupplier);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SupplierReqDto dto)
        {
            var success = await _service.UpdateSupplierAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteSupplierAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
