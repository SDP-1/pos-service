using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Models.DTO.Backup;
using pos_service.Services.Backup;

namespace pos_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpPost("now")]
        public async Task<IActionResult> CreateNow([FromBody] BackupLocationDto? location)
        {
            // if location provided, ensure it's saved and used; otherwise use last default
            string? locUuid = null;
            string? path = null;
            if (location != null)
            {
                // create or update location
                // we will rely on backup service to handle saving/updating location by uuid
                var resSave = await _backupService.SaveOrGetLocationAsync(location);
                locUuid = resSave?.Uuid;
                path = resSave?.Path;
            }

            var res = await _backupService.CreateBackupAsync(null, locUuid, path);
            if (!res.Success) return StatusCode(500, res);
            return Ok(res);
        }
    }
}
