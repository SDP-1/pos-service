using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Models;
using pos_service.Models.DTO.Backup;
using pos_service.Repositories;

namespace pos_service.Controllers
{
    [ApiController]
    [Route("api/backup/locations")]
    [Authorize]
    public class BackupLocationsController : ControllerBase
    {
        private readonly IBackupLocationRepository _repo;

        public BackupLocationsController(IBackupLocationRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BackupLocationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var model = new BackupLocation
            {
                Name = dto.Name,
                Path = dto.Path,
                IsRemote = dto.IsRemote,
                IsDefault = dto.IsDefault
            };

            var created = await _repo.AddAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BackupLocationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            existing.Name = dto.Name;
            existing.Path = dto.Path;
            existing.IsRemote = dto.IsRemote;
            existing.IsDefault = dto.IsDefault;
            var updated = await _repo.UpdateAsync(existing);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(existing);
            return NoContent();
        }
    }
}
