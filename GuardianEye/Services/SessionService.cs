using GuardianEye.Data;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public interface ISessionService
    {
        Task<Session?> StartSessionAsync(int userId, string deviceName);
        Task EndSessionAsync(int sessionId);
        Task<List<Session>> GetUserSessionsAsync(int userId);
        Task<bool> IncrementSessionCountAsync(int userId);
        Task<int> GetSessionCountTodayAsync(int userId);
        Task ResetAllDailySessionsAsync();
    }

    public class SessionService : ISessionService
    {
        private readonly IDatabaseService _db;

        public SessionService(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<Session?> StartSessionAsync(int userId, string deviceName)
        {
            try
            {
                var user = await _db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Id = @Id", new { Id = userId });

                if (user == null) return null;
                if (user.SessionsUsedToday >= user.MaxDailySessions) return null;

                var sessionNumber = user.SessionsUsedToday + 1;
                var durationMinutes = user.SessionDurationMinutes;

                var session = new Session
                {
                    UserId = userId,
                    StartTime = DateTime.UtcNow,
                    Status = "Active",
                    DeviceName = deviceName,
                    SessionNumber = sessionNumber
                };

                var result = await _db.ExecuteAsync(
                    @"INSERT INTO Sessions (UserId, StartTime, Status, DeviceName, SessionNumber)
                      VALUES (@UserId, @StartTime, @Status, @DeviceName, @SessionNumber);
                      SELECT last_insert_rowid()",
                    new { session.UserId, session.StartTime, session.Status, session.DeviceName, session.SessionNumber });

                session.Id = (int)result;

                await _db.ExecuteAsync(
                    @"UPDATE Users SET SessionsUsedToday = SessionsUsedToday + 1, IsLoggedIn = 1,
                      SessionEndTime = @EndTime WHERE Id = @Id",
                    new { EndTime = DateTime.UtcNow.AddMinutes(durationMinutes), Id = userId });

                Helpers.Logging.Info($"Session started for user ID {userId}, session #{sessionNumber}");
                return session;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error starting session for user ID {userId}", ex);
                return null;
            }
        }

        public async Task EndSessionAsync(int sessionId)
        {
            try
            {
                var session = await _db.QueryFirstOrDefaultAsync<Session>(
                    "SELECT * FROM Sessions WHERE Id = @Id", new { Id = sessionId });

                if (session == null) return;

                session.EndTime = DateTime.UtcNow;
                if (session.StartTime != default)
                    session.Duration = session.EndTime - session.StartTime;
                session.Status = "Ended";

                await _db.ExecuteAsync(
                    @"UPDATE Sessions SET EndTime = @EndTime, Duration = @Duration, Status = @Status
                      WHERE Id = @Id", session);

                await _db.ExecuteAsync(
                    "UPDATE Users SET IsLoggedIn = 0, SessionEndTime = NULL WHERE Id = @Id",
                    new { Id = session.UserId });

                Helpers.Logging.Info($"Session {sessionId} ended");
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error ending session {sessionId}", ex);
            }
        }

        public async Task<List<Session>> GetUserSessionsAsync(int userId)
        {
            var sessions = await _db.QueryAsync<Session>(
                "SELECT * FROM Sessions WHERE UserId = @UserId ORDER BY StartTime DESC",
                new { UserId = userId });
            return sessions.ToList();
        }

        public async Task<bool> IncrementSessionCountAsync(int userId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET SessionsUsedToday = SessionsUsedToday + 1 WHERE Id = @Id",
                    new { Id = userId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error incrementing session count for user ID {userId}", ex);
                return false;
            }
        }

        public async Task<int> GetSessionCountTodayAsync(int userId)
        {
            return await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT SessionsUsedToday FROM Users WHERE Id = @Id", new { Id = userId });
        }

        public async Task ResetAllDailySessionsAsync()
        {
            try
            {
                await _db.ExecuteAsync("UPDATE Users SET SessionsUsedToday = 0 WHERE Role = 'Student'");
                Helpers.Logging.Info("All daily session counts reset");
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error("Error resetting all daily sessions", ex);
            }
        }
    }
}