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

        /// <summary>
        /// Retrieves all configured backup locations.
        /// </summary>
        /// <returns>200 OK with a list of BackupLocation records.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Creates a new backup location configuration.
        /// </summary>
        /// <param name="dto">Backup location data transfer object.</param>
        /// <returns>201 Created with the created BackupLocation.</returns>
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

        /// <summary>
        /// Retrieves a backup location by id.
        /// </summary>
        /// <param name="id">Identifier of the backup location.</param>
        /// <returns>200 OK with the location or 404 NotFound if missing.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// Updates an existing backup location.
        /// </summary>
        /// <param name="id">Identifier of the backup location to update.</param>
        /// <param name="dto">Updated backup location data.</param>
        /// <returns>200 OK with the updated entity or 404 NotFound.</returns>
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

        /// <summary>
        /// Deletes a backup location by id.
        /// </summary>
        /// <param name="id">Identifier of the backup location to delete.</param>
        /// <returns>204 NoContent on success or 404 NotFound if missing.</returns>
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
