using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Repositories;

namespace pos_service.Controllers
{
    [ApiController]
    [Route("api/backup/history")]
    [Authorize]
    public class BackupHistoryController : ControllerBase
    {
        private readonly IBackupHistoryRepository _repo;

        public BackupHistoryController(IBackupHistoryRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int max = 50)
        {
            if (max <= 0) max = 50;
            var list = await _repo.GetAllAsync(max);
            return Ok(list);
        }
    }
}
