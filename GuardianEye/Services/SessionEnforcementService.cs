using System.Timers;
using GuardianEye.Data;
using GuardianEye.Helpers;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public class SessionEnforcementService
    {
        private readonly IDatabaseService _db;
        private readonly ISessionService _sessionService;
        private readonly IActivityLogService _activityLogService;
        private readonly ILockScreenService _lockScreenService;
        private Timer? _enforcementTimer;
        private Timer? _midnightResetTimer;

        public SessionEnforcementService(IDatabaseService db, ISessionService sessionService,
            IActivityLogService activityLogService, ILockScreenService lockScreenService)
        {
            _db = db;
            _sessionService = sessionService;
            _activityLogService = activityLogService;
            _lockScreenService = lockScreenService;
        }

        public void StartEnforcement()
        {
            _enforcementTimer = new Timer(5000); // Check every 5 seconds
            _enforcementTimer.Elapsed += async (s, e) => await EnforceSessionsAsync();
            _enforcementTimer.Start();

            ScheduleMidnightReset();
            Logging.Info("Session enforcement started");
        }

        public void StopEnforcement()
        {
            _enforcementTimer?.Stop();
            _midnightResetTimer?.Stop();
            Logging.Info("Session enforcement stopped");
        }

        private async Task EnforceSessionsAsync()
        {
            try
            {
                var activeUsers = await _db.QueryAsync<User>(
                    "SELECT * FROM Users WHERE IsLoggedIn = 1 AND SessionEndTime IS NOT NULL");

                foreach (var user in activeUsers)
                {
                    var now = DateTime.UtcNow;
                    var endTime = user.SessionEndTime!.Value;

                    if (now >= endTime)
                    {
                        await ExpireSessionAsync(user);
                    }
                    else
                    {
                        var remaining = endTime - now;
                        await CheckAndNotifyWarningsAsync(user, remaining);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Error during session enforcement", ex);
            }
        }

        private async Task CheckAndNotifyWarningsAsync(User user, TimeSpan remaining)
        {
            try
            {
                var totalSeconds = (int)remaining.TotalSeconds;

                if (totalSeconds == 300)
                {
                    await LogWarningAsync(user, "5 minutes remaining", "Warning5Min");
                }
                else if (totalSeconds == 60)
                {
                    await LogWarningAsync(user, "1 minute remaining", "Warning1Min");
                }
                else if (totalSeconds <= 10)
                {
                    await LogWarningAsync(user, "10 seconds remaining - session ending soon", "Warning10Sec");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Error checking warnings for user {user.Id}", ex);
            }
        }

        private async Task LogWarningAsync(User user, string message, string activityType)
        {
            try
            {
                await _activityLogService.LogActivityAsync(new ActivityLog
                {
                    UserId = user.Id,
                    ActivityType = activityType,
                    Description = message,
                    Severity = "Warning",
                    DeviceName = Environment.MachineName
                });

                await _db.ExecuteAsync(
                    @"INSERT INTO Notifications (UserId, Title, Message, Type, ExpiresAt)
                      VALUES (@UserId, @Title, @Message, 'Warning', @ExpiresAt)",
                    new { UserId = user.Id, Title = "Session Warning",
                           Message = message, ExpiresAt = DateTime.UtcNow.AddMinutes(5) });
            }
            catch (Exception ex)
            {
                Logging.Error($"Error logging warning for user {user.Id}", ex);
            }
        }

        private async Task ExpireSessionAsync(User user)
        {
            try
            {
                await _lockScreenService.LockWorkstationAsync(user.Id, "Session expired");
                await _sessionService.EndSessionAsync(user.Id);
                await _db.ExecuteAsync(
                    "UPDATE Users SET IsLoggedIn = 0, SessionEndTime = NULL WHERE Id = @Id",
                    new { user.Id });

                await _activityLogService.LogActivityAsync(new ActivityLog
                {
                    UserId = user.Id,
                    ActivityType = "SessionExpired",
                    Description = "Session expired due to time limit",
                    Severity = "Critical",
                    DeviceName = Environment.MachineName
                });

                Logging.Info($"Session expired for user {user.Username}");
            }
            catch (Exception ex)
            {
                Logging.Error($"Error expiring session for user {user.Id}", ex);
            }
        }

        private void ScheduleMidnightReset()
        {
            try
            {
                _midnightResetTimer = new Timer(60000); // Check every minute
                _midnightResetTimer.Elapsed += (s, e) =>
                {
                    var now = DateTime.UtcNow;
                    if (now.Hour == 0 && now.Minute == 0)
                    {
                        _ = Task.Run(() => _sessionService.ResetAllDailySessionsAsync());
                    }
                };
                _midnightResetTimer.Start();
            }
            catch (Exception ex)
            {
                Logging.Error("Error scheduling midnight reset", ex);
            }
        }
    }
}