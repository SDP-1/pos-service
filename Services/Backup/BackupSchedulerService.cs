using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pos_service.Models.DTO.Backup;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace pos_service.Services.Backup
{
    public class BackupSchedulerService : BackgroundService
    {
        private readonly ILogger<BackupSchedulerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

        public BackupSchedulerService(ILogger<BackupSchedulerService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackupSchedulerService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var outerScope = _scopeFactory.CreateScope();
                    var scheduleRepo = outerScope.ServiceProvider.GetRequiredService<Repositories.IBackupScheduleRepository>();
                    var schedules = await scheduleRepo.GetEnabledAsync();
                    var now = DateTime.Now; // using local time for human friendly schedules

                    foreach (var s in schedules.Where(x => x.Enabled))
                    {
                        // Ensure marker folder exists
                        var markerFolder = Path.Combine(AppContext.BaseDirectory, "backups");
                        try { if (!Directory.Exists(markerFolder)) Directory.CreateDirectory(markerFolder); } catch { }

                        // Use schedule-specific marker to avoid collisions
                        string markerPath(string name) => Path.Combine(markerFolder, name);

                        // Time-of-day format: HH:mm -> run once per day at that local time
                        var timeOfDayPattern = new Regex("^\\d{1,2}:\\d{2}$");
                        if (!string.IsNullOrWhiteSpace(s.Schedule) && timeOfDayPattern.IsMatch(s.Schedule))
                            {
                                if (TimeSpan.TryParse(s.Schedule, out var tm))
                                {
                                    var scheduledToday = DateTime.Today + tm; // local
                                    var marker = $"backup_time_{s.Uuid}.last";
                                    var lastRun = File.Exists(markerPath(marker)) ? File.GetLastWriteTime(markerPath(marker)) : DateTime.MinValue;

                                    // Run if now >= scheduled time AND we haven't run for today's scheduled time yet
                                    if (DateTime.Now >= scheduledToday && lastRun < scheduledToday)
                                    {
                                        _logger.LogInformation("Triggering scheduled daily backup for {schedule} (uuid={uuid})", s.Schedule, s.Uuid);
                                        // mark start time so intervals are calculated from start
                                        File.WriteAllText(markerPath(marker), DateTime.Now.ToString("o"));
                                        using var scope = _scopeFactory.CreateScope();
                                        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                                        var res = await backupService.CreateBackupAsync(s.Uuid, s.BackupLocation?.Uuid, s.BackupLocation?.Path, stoppingToken);
                                        // Format message as "Name (Schedule) <message>" or "Schedule <message>" when name missing
                                        var label = string.IsNullOrWhiteSpace(s.Name) ? s.Schedule : $"{s.Name} ({s.Schedule})";
                                        res.Message = string.IsNullOrWhiteSpace(label) ? res.Message : $"{label} {res.Message}";
                                        if (res.Success)
                                        {
                                            await backupService.UpdateScheduleLastRunAsync(s.Uuid, DateTime.Now);
                                        }
                                    }
                                }
                                continue;
                            }

                        // 2) ISO 8601 duration (PTnM etc.) -> treat as interval
                        if (!string.IsNullOrWhiteSpace(s.Schedule) && (s.Schedule.StartsWith("P", StringComparison.OrdinalIgnoreCase) || s.Schedule.StartsWith("PT", StringComparison.OrdinalIgnoreCase)))
                        {
                            try
                            {
                                var dur = System.Xml.XmlConvert.ToTimeSpan(s.Schedule);
                                var minutes = (int)dur.TotalMinutes;
                                if (minutes <= 0) continue;

                                var marker = $"backup_interval_{s.Uuid}_{minutes}.last";
                                var lastRun = File.Exists(markerPath(marker)) ? File.GetLastWriteTime(markerPath(marker)) : DateTime.MinValue;
                                if ((DateTime.Now - lastRun).TotalMinutes >= minutes)
                                {
                                    _logger.LogInformation("Triggering ISO interval backup every {minutes} minutes (uuid={uuid})", minutes, s.Uuid);
                                    // mark start time so next interval is measured from start
                                    File.WriteAllText(markerPath(marker), DateTime.Now.ToString("o"));
                                    using var scope = _scopeFactory.CreateScope();
                                    var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                                    var res = await backupService.CreateBackupAsync(s.Uuid, s.BackupLocation?.Uuid, s.BackupLocation?.Path, stoppingToken);
                                    // Format message as "Name (Schedule) <message>" or "Schedule <message>" when name missing
                                    var label = string.IsNullOrWhiteSpace(s.Name) ? s.Schedule : $"{s.Name} ({s.Schedule})";
                                    res.Message = string.IsNullOrWhiteSpace(label) ? res.Message : $"{label} {res.Message}";
                                    if (res.Success)
                                    {
                                        await backupService.UpdateScheduleLastRunAsync(s.Uuid, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            continue;
                        }

                        // No fallback TimeSpan parsing: HH:mm:ss format is not supported.
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running backup scheduler");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }
}
