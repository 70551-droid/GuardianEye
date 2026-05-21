using GuardianEye.Data;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(ActivityLog log);
        Task<List<ActivityLog>> GetUserActivityLogsAsync(int userId, DateTime? from = null, DateTime? to = null);
        Task<List<ActivityLog>> GetAllActivityLogsAsync(DateTime? from = null, DateTime? to = null);
        Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count = 100);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly IDatabaseService _db;

        public ActivityLogService(IDatabaseService db)
        {
            _db = db;
        }

        public async Task LogActivityAsync(ActivityLog log)
        {
            try
            {
                log.Timestamp = DateTime.UtcNow;
                await _db.ExecuteAsync(
                    @"INSERT INTO ActivityLogs (UserId, ActivityType, Description, ApplicationName, 
                      WindowTitle, Url, Duration, DeviceName, Severity)
                      VALUES (@UserId, @ActivityType, @Description, @ApplicationName, @WindowTitle, 
                      @Url, @Duration, @DeviceName, @Severity)", log);
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error("Error logging activity", ex);
            }
        }

        public async Task<List<ActivityLog>> GetUserActivityLogsAsync(int userId, DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
            var toDate = to ?? DateTime.UtcNow;

            var logs = await _db.QueryAsync<ActivityLog>(
                @"SELECT * FROM ActivityLogs WHERE UserId = @UserId AND Timestamp BETWEEN @From AND @To
                  ORDER BY Timestamp DESC",
                new { UserId = userId, From = fromDate, To = toDate });

            return logs.ToList();
        }

        public async Task<List<ActivityLog>> GetAllActivityLogsAsync(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
            var toDate = to ?? DateTime.UtcNow;

            var logs = await _db.QueryAsync<ActivityLog>(
                @"SELECT * FROM ActivityLogs WHERE Timestamp BETWEEN @From AND @To
                  ORDER BY Timestamp DESC",
                new { From = fromDate, To = toDate });

            return logs.ToList();
        }

        public async Task<List<ActivityLog>> GetRecentActivityLogsAsync(int count = 100)
        {
            var logs = await _db.QueryAsync<ActivityLog>(
                $"SELECT * FROM ActivityLogs ORDER BY Timestamp DESC LIMIT {count}");
            return logs.ToList();
        }
    }
}