using GuardianEye.Data;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public class UserService : IUserService
    {
        private readonly IDatabaseService _db;

        public UserService(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllStudentsAsync()
        {
            var users = await _db.QueryAsync<User>(
                "SELECT * FROM Users WHERE Role = 'Student' ORDER BY FullName");
            return users.ToList();
        }

        public async Task<List<User>> GetActiveStudentsAsync()
        {
            var users = await _db.QueryAsync<User>(
                "SELECT * FROM Users WHERE Role = 'Student' AND IsActive = 1 ORDER BY FullName");
            return users.ToList();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Username = @Username", new { Username = username });
        }

        public async Task<bool> CreateUserAsync(User user, string rawPassword)
        {
            try
            {
                var existing = await _db.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Username = @Username OR StudentId = @StudentId",
                    new { Username = user.Username, StudentId = user.StudentId });

                if (existing > 0) return false;

                user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawPassword, 13);
                user.CreatedAt = DateTime.UtcNow;

                var result = await _db.ExecuteAsync(
                    @"INSERT INTO Users (FullName, Username, PasswordHash, Role, StudentId, Class, DeviceId,
                      MaxDailySessions, SessionDurationMinutes, IsActive)
                      VALUES (@FullName, @Username, @PasswordHash, @Role, @StudentId, @Class, @DeviceId,
                      @MaxDailySessions, @SessionDurationMinutes, @IsActive)", user);

                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error creating user {user.Username}", ex);
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                var result = await _db.ExecuteAsync(
                    @"UPDATE Users SET FullName = @FullName, StudentId = @StudentId, Class = @Class,
                      DeviceId = @DeviceId, MaxDailySessions = @MaxDailySessions,
                      SessionDurationMinutes = @SessionDurationMinutes, IsActive = @IsActive,
                      UpdatedAt = @UpdatedAt, IsLocked = @IsLocked
                      WHERE Id = @Id", user);

                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error updating user ID {user.Id}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET IsActive = 0 WHERE Id = @Id", new { Id = id });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error deleting user ID {id}", ex);
                return false;
            }
        }

        public async Task<bool> ResetUserSessionsAsync(int userId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET SessionsUsedToday = 0 WHERE Id = @Id", new { Id = userId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error resetting sessions for user ID {userId}", ex);
                return false;
            }
        }

        public async Task<bool> LockUserAsync(int userId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET IsLocked = 1 WHERE Id = @Id", new { Id = userId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error locking user ID {userId}", ex);
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET IsLocked = 0 WHERE Id = @Id", new { Id = userId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error($"Error unlocking user ID {userId}", ex);
                return false;
            }
        }

        public async Task<int> GetUserSessionCountAsync(int userId)
        {
            return await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT SessionsUsedToday FROM Users WHERE Id = @Id", new { Id = userId });
        }
    }
}