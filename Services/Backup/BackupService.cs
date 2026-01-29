using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pos_service.Models.DTO.Backup;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using pos_service.Models;
using pos_service.Repositories;

namespace pos_service.Services.Backup
{
    public class BackupService : IBackupService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<BackupService> _logger;
        private readonly IBackupScheduleRepository _scheduleRepository;
        private readonly IBackupLocationRepository _locationRepository;
        private readonly IBackupHistoryRepository _historyRepository;

        public BackupService(
            IWebHostEnvironment env,
            IConfiguration config,
            ILogger<BackupService> logger,
            IBackupScheduleRepository scheduleRepository,
            IBackupLocationRepository locationRepository,
            IBackupHistoryRepository historyRepository)
        {
            _env = env;
            _config = config;
            _logger = logger;
            _scheduleRepository = scheduleRepository;
            _locationRepository = locationRepository;
            _historyRepository = historyRepository;
            EnsureBackupFolder();
        }

        private void EnsureBackupFolder()
        {
            var folder = Path.Combine(_env.ContentRootPath, "backups");
            try { if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); } catch { }
        }

        // Manual trigger without parameters - uses saved default or last-used location
        public async Task<BackupResponseDto> CreateBackupAsync(CancellationToken cancellationToken = default)
        {
            var defaultLoc = (await _locationRepository.GetAllAsync()).FirstOrDefault(l => l.IsDefault && l.IsActive);
            if (defaultLoc != null)
            {
                return await CreateBackupAsync(null, defaultLoc.Uuid, defaultLoc.Path, cancellationToken);
            }

            return await CreateBackupAsync(null, null, null, cancellationToken);
        }

        // Main implementation - scheduleUuid and locationUuid optional
        public async Task<BackupResponseDto> CreateBackupAsync(string? scheduleUuid, string? locationUuid, string? targetPath = null, CancellationToken cancellationToken = default)
        {
            var executedAt = DateTime.Now;

            // Build label from schedule (Name (Schedule)) when scheduleUuid provided
            string label = string.Empty;
            if (!string.IsNullOrWhiteSpace(scheduleUuid))
            {
                try
                {
                    var sched = await _scheduleRepository.GetByUuidAsync(scheduleUuid);
                    if (sched != null)
                    {
                        label = string.IsNullOrWhiteSpace(sched.Name) ? sched.Schedule : $"{sched.Name} ({sched.Schedule})";
                    }
                }
                catch { }
            }

            // sanitize label for filename
            static string SanitizeLabel(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                // replace whitespace with underscore
                var tmp = Regex.Replace(s, "\\s+", "_");
                // remove any chars not allowed (keep letters, numbers, underscore, dash, dot, parentheses)
                tmp = Regex.Replace(tmp, "[^\\w\\-\\.()]+", "_");
                if (tmp.Length > 60) tmp = tmp.Substring(0, 60);
                return tmp;
            }

            var safeLabel = SanitizeLabel(label);
            var fileName = string.IsNullOrWhiteSpace(safeLabel)
                ? $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql"
                : $"{safeLabel}_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

            // Resolve effective target path
            if (string.IsNullOrWhiteSpace(locationUuid) && string.IsNullOrWhiteSpace(targetPath))
            {
                var defaultLoc = (await _locationRepository.GetAllAsync()).FirstOrDefault(l => l.IsDefault && l.IsActive);
                if (defaultLoc != null)
                {
                    locationUuid = defaultLoc.Uuid;
                    targetPath = defaultLoc.Path;
                }
                else
                {
                    var histories = (await _historyRepository.GetAllAsync()).OrderByDescending(h => h.ExecutedAt).ToList();
                    var last = histories.FirstOrDefault(h => !string.IsNullOrWhiteSpace(h.LocationUuid));
                    if (last != null)
                    {
                        var loc = await _locationRepository.GetByUuidAsync(last.LocationUuid);
                        if (loc != null)
                        {
                            locationUuid = loc.Uuid;
                            targetPath = loc.Path;
                        }
                    }
                }
            }

            var folder = string.IsNullOrWhiteSpace(targetPath) ? Path.Combine(_env.ContentRootPath, "backups") : targetPath;
            try { if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); } catch (Exception ex)
            {
                var err = new BackupResponseDto { Success = false, Message = $"Failed to create target folder: {ex.Message}", ExecutedAt = executedAt };
                await RecordHistoryAsync(scheduleUuid, locationUuid, err);
                return err;
            }

            var fullPath = Path.Combine(folder, fileName);

            try
            {
                var conn = _config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(conn))
                {
                    var resp = new BackupResponseDto { Success = false, Message = "No connection string configured", ExecutedAt = executedAt };
                    await RecordHistoryAsync(scheduleUuid, locationUuid, resp);
                    return resp;
                }

                var provider = _config.GetValue<string>("DatabaseProvider") ?? "mysql";
                if (!provider.Equals("mysql", StringComparison.OrdinalIgnoreCase))
                {
                    var unsupported = new BackupResponseDto { Success = false, Message = "Unsupported provider", ExecutedAt = executedAt };
                    await RecordHistoryAsync(scheduleUuid, locationUuid, unsupported);
                    return unsupported;
                }

                var csb = new MySqlConnector.MySqlConnectionStringBuilder(conn);
                var password = csb.Password ?? string.Empty;
                var user = csb.UserID ?? "root";
                var server = csb.Server ?? "localhost";
                var database = csb.Database ?? string.Empty;

                var configuredDump = _config["Backup:MySqlDumpPath"];
                var dumpExe = string.IsNullOrWhiteSpace(configuredDump) ? "mysqldump" : configuredDump;
                if (!string.IsNullOrWhiteSpace(configuredDump) && !File.Exists(dumpExe))
                {
                    var msg = $"Configured mysqldump not found at '{dumpExe}'.";
                    _logger.LogError(msg);
                    var resp = new BackupResponseDto { Success = false, Message = msg, ExecutedAt = executedAt };
                    await RecordHistoryAsync(scheduleUuid, locationUuid, resp);
                    return resp;
                }

                var args = $"-h {server} -u {user} --single-transaction --routines --triggers --databases {database} --result-file=\"{fullPath}\"";
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dumpExe,
                    Arguments = args,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (!string.IsNullOrEmpty(password))
                {
                    try { psi.Environment["MYSQL_PWD"] = password; } catch { }
                }

                var proc = System.Diagnostics.Process.Start(psi);
                if (proc == null)
                {
                    var fail = new BackupResponseDto { Success = false, Message = "Failed to start mysqldump process", ExecutedAt = executedAt };
                    await RecordHistoryAsync(scheduleUuid, locationUuid, fail);
                    return fail;
                }

                var error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync(cancellationToken);
                if (proc.ExitCode != 0)
                {
                    _logger.LogError("mysqldump failed: {error}", error);
                    var fail = new BackupResponseDto { Success = false, Message = $"mysqldump failed: {error}", ExecutedAt = executedAt };
                    await RecordHistoryAsync(scheduleUuid, locationUuid, fail);
                    return fail;
                }

                var success = new BackupResponseDto { Success = true, Message = "Backup successfully created", FilePath = fullPath, ExecutedAt = executedAt };
                await RecordHistoryAsync(scheduleUuid, locationUuid, success);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup failed");
                var err = new BackupResponseDto { Success = false, Message = "Backup failed: " + ex.Message, ExecutedAt = executedAt };
                await RecordHistoryAsync(scheduleUuid, locationUuid, err);
                return err;
            }
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesAsync()
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            return schedules.Select(s => new ScheduleDto
            {
                Uuid = s.Uuid,
                Schedule = s.Schedule,
                Name = s.Name,
                Enabled = s.Enabled,
                BackupLocationUuid = s.BackupLocationUuid
            });
        }

        public async Task AddScheduleAsync(ScheduleDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.BackupLocationUuid))
            {
                var loc = await _locationRepository.GetByUuidAsync(dto.BackupLocationUuid);
                if (loc == null) throw new ArgumentException("Backup location not found");
            }

            var entity = new BackupSchedule
            {
                Name = dto.Name,
                Schedule = dto.Schedule,
                Enabled = dto.Enabled,
                BackupLocationUuid = dto.BackupLocationUuid
            };

            await _scheduleRepository.AddAsync(entity);
        }

        public async Task<Models.BackupLocation?> SaveOrGetLocationAsync(Models.DTO.Backup.BackupLocationDto dto)
        {
            if (dto == null) return null;
            if (!string.IsNullOrWhiteSpace(dto.Uuid))
            {
                var existing = await _locationRepository.GetByUuidAsync(dto.Uuid);
                if (existing != null)
                {
                    existing.Name = dto.Name ?? existing.Name;
                    existing.Path = dto.Path;
                    existing.IsRemote = dto.IsRemote;
                    existing.IsDefault = dto.IsDefault;
                    var updated = await _locationRepository.UpdateAsync(existing);
                    if (dto.IsDefault)
                    {
                        var others = (await _locationRepository.GetAllAsync()).Where(x => x.Uuid != updated.Uuid && x.IsDefault).ToList();
                        foreach (var o in others) { o.IsDefault = false; await _locationRepository.UpdateAsync(o); }
                    }
                    return updated;
                }
            }

            var loc = new Models.BackupLocation { Name = dto.Name ?? "Location", Path = dto.Path, IsRemote = dto.IsRemote, IsDefault = dto.IsDefault };
            var created = await _locationRepository.AddAsync(loc);
            if (dto.IsDefault)
            {
                var others = (await _locationRepository.GetAllAsync()).Where(x => x.Uuid != created.Uuid && x.IsDefault).ToList();
                foreach (var o in others) { o.IsDefault = false; await _locationRepository.UpdateAsync(o); }
            }
            return created;
        }

        private async Task RecordHistoryAsync(string? scheduleUuid, string? locationUuid, BackupResponseDto result)
        {
            try
            {
                var history = new Models.BackupHistory
                {
                    ScheduleUuid = scheduleUuid ?? string.Empty,
                    LocationUuid = locationUuid ?? string.Empty,
                    ExecutedAt = result.ExecutedAt,
                    Success = result.Success,
                    Message = result.Message ?? string.Empty,
                    FilePath = result.FilePath ?? string.Empty
                };
                await _historyRepository.AddAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record backup history");
            }
        }

        public async Task RemoveScheduleAsync(string schedule)
        {
            var all = (await _scheduleRepository.GetAllAsync()).ToList();
            var toRemove = all.FirstOrDefault(s => s.Schedule == schedule);
            if (toRemove != null)
            {
                await _scheduleRepository.DeleteAsync(toRemove);
            }
        }
        public async Task UpdateScheduleLastRunAsync(string scheduleUuid, DateTime lastRunAt)
        {
            var all = (await _scheduleRepository.GetAllAsync()).ToList();
            var item = all.FirstOrDefault(s => s.Uuid == scheduleUuid);
            if (item != null)
            {
                // Store local system time for LastRunAt to match backup ExecutedAt which uses DateTime.Now
                item.LastRunAt = DateTime.Now;
                await _scheduleRepository.UpdateAsync(item);
            }
        }
    }
}
