using GuardianEye.Data;
using GuardianEye.Helpers;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _db;
        private readonly IUserService _userService;

        public AuthService(IDatabaseService db, IUserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1",
                    new { Username = username });

                if (user == null)
                    return new AuthResult { Success = false, ErrorMessage = "Invalid username or password" };

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return new AuthResult { Success = false, ErrorMessage = "Invalid username or password" };

                if (user.IsLocked)
                    return new AuthResult { Success = false, ErrorMessage = "Account is locked" };

                if (user.SessionsUsedToday >= user.MaxDailySessions)
                    return new AuthResult { Success = false, ErrorMessage = "Daily session limit reached. Please try again tomorrow." };

                user.LastLoginDate = DateTime.UtcNow;
                user.IsLoggedIn = true;
                user.SessionEndTime = DateTime.UtcNow.AddMinutes(user.SessionDurationMinutes);
                user.UpdatedAt = DateTime.UtcNow;

                await _db.ExecuteAsync(
                    @"UPDATE Users SET LastLoginDate = @LastLoginDate, IsLoggedIn = 1, 
                      SessionEndTime = @SessionEndTime, UpdatedAt = @UpdatedAt 
                      WHERE Id = @Id", user);

                Logging.Info($"User {username} logged in successfully");
                return new AuthResult { Success = true, User = user };
            }
            catch (Exception ex)
            {
                Logging.Error($"Login error for user {username}", ex);
                return new AuthResult { Success = false, ErrorMessage = "Login failed. Please try again." };
            }
        }

        public async Task LogoutAsync(int userId)
        {
            try
            {
                await _db.ExecuteAsync(
                    @"UPDATE Users SET IsLoggedIn = 0, SessionEndTime = NULL WHERE Id = @Id",
                    new { Id = userId });

                await _db.ExecuteAsync(
                    @"INSERT INTO Sessions (UserId, EndTime, Status) 
                      VALUES (@UserId, @EndTime, 'Ended')",
                    new { UserId = userId, EndTime = DateTime.UtcNow });

                Logging.Info($"User ID {userId} logged out");
            }
            catch (Exception ex)
            {
                Logging.Error($"Logout error for user ID {userId}", ex);
            }
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

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _db.QueryAsync<User>("SELECT * FROM Users ORDER BY FullName");
            return users.ToList();
        }

        public async Task<bool> CreateUserAsync(User user, string rawPassword)
        {
            try
            {
                var existing = await _db.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Username = @Username OR StudentId = @StudentId",
                    new { Username = user.Username, StudentId = user.StudentId });

                if (existing > 0) return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
                user.CreatedAt = DateTime.UtcNow;

                var result = await _db.ExecuteAsync(
                    @"INSERT INTO Users (FullName, Username, PasswordHash, Role, StudentId, Class, DeviceId,
                      MaxDailySessions, SessionDurationMinutes, IsActive)
                      VALUES (@FullName, @Username, @PasswordHash, @Role, @StudentId, @Class, @DeviceId,
                      @MaxDailySessions, @SessionDurationMinutes, @IsActive)", user);

                Logging.Info($"User {user.Username} created successfully");
                return result > 0;
            }
            catch (Exception ex)
            {
                Logging.Error($"Error creating user {user.Username}", ex);
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
                Logging.Error($"Error updating user ID {user.Id}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET IsActive = 0 WHERE Id = @Id", new { Id = userId });
                return result > 0;
            }
            catch (Exception ex)
            {
                Logging.Error($"Error deleting user ID {userId}", ex);
                return false;
            }
        }

        public async Task<bool> ResetDailySessionsAsync(int userId)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET SessionsUsedToday = 0 WHERE Id = @Id", new { Id = userId });
                Logging.Info($"Daily sessions reset for user ID {userId}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Logging.Error($"Error resetting sessions for user ID {userId}", ex);
                return false;
            }
        }

        public async Task<bool> ValidateSessionAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return false;

                if (user.SessionsUsedToday >= user.MaxDailySessions)
                    return false;

                if (user.SessionEndTime.HasValue && user.SessionEndTime < DateTime.UtcNow)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Logging.Error($"Session validation error for user ID {userId}", ex);
                return false;
            }
        }
    }
}