using GuardianEye.Models;

namespace GuardianEye.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllStudentsAsync();
        Task<List<User>> GetActiveStudentsAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> CreateUserAsync(User user, string rawPassword);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ResetUserSessionsAsync(int userId);
        Task<bool> LockUserAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);
        Task<int> GetUserSessionCountAsync(int userId);
    }

    public interface IThemeService
    {
        void ApplyDarkTheme();
        void ApplyLightTheme();
    }
}