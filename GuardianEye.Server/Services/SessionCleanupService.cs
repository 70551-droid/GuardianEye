using GuardianEye.Server.Data;
using GuardianEye.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GuardianEye.Server.Services;

public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private const int CleanupIntervalSeconds = 60;
    private const int StaleSessionThresholdMinutes = 2;

    public SessionCleanupService(IServiceProvider serviceProvider, ILogger<SessionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Cleanup Service started. Running every {Interval}s, stale threshold: {Threshold}min",
            CleanupIntervalSeconds, StaleSessionThresholdMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GuardianEyeDbContext>();

                var staleThreshold = DateTime.UtcNow.AddMinutes(-StaleSessionThresholdMinutes);
                var staleSessions = await db.ActiveSessions
                    .Where(s => s.IsActive && s.LastHeartbeat < staleThreshold)
                    .ToListAsync(stoppingToken);

                if (staleSessions.Any())
                {
                    foreach (var session in staleSessions)
                    {
                        session.IsActive = false;
                        _logger.LogInformation("Deactivated stale session for user {UserId}. Last heartbeat: {LastHeartbeat}",
                            session.UserId, session.LastHeartbeat);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Cleaned up {Count} stale session(s)", staleSessions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(CleanupIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Session Cleanup Service stopped");
    }
}
