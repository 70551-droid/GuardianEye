using System.Diagnostics;
using System.Runtime.InteropServices;
using GuardianEye.Models;

namespace GuardianEye.Services
{
    public interface ILockScreenService
    {
        Task LockWorkstationAsync(int userId, string reason);
        Task UnlockWorkstationAsync(int userId);
        bool IsWorkstationLocked { get; }
        Task LaunchLockScreenAsync(int userId, string reason);
        void CloseLockScreen();
    }

    public class LockScreenService : ILockScreenService
    {
        private Process? _lockScreenProcess;
        private bool _isLocked = false;

        public bool IsWorkstationLocked => _isLocked;

        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        public async Task LockWorkstationAsync(int userId, string reason)
        {
            var user = await GetUserById(userId);
            
            await LogActivity(userId, "System Lock", reason);
            
            _isLocked = true;
            
            await Task.Run(() => {
                try
                {
                    LockWorkStation();
                }
                catch (Exception ex)
                {
                    Helpers.Logging.Error("Error locking workstation", ex);
                }
            });

            await LaunchLockScreenAsync(userId, reason);
        }

        public async Task UnlockWorkstationAsync(int userId)
        {
            _isLocked = false;
            
            CloseLockScreen();
            
            await LogActivity(userId, "System Unlock", "User unlocked the system");
        }

        public async Task LaunchLockScreenAsync(int userId, string reason)
        {
            try
            {
                var lockScreenPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "GuardianEye.LockScreen.exe");

                if (System.IO.File.Exists(lockScreenPath))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = lockScreenPath,
                        Arguments = $"--userid {userId} --reason \"{reason}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    _lockScreenProcess = Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error("Error launching lock screen", ex);
            }
        }

        public void CloseLockScreen()
        {
            try
            {
                if (_lockScreenProcess != null && !_lockScreenProcess.HasExited)
                {
                    _lockScreenProcess.Kill();
                    _lockScreenProcess.Dispose();
                    _lockScreenProcess = null;
                }
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error("Error closing lock screen", ex);
            }
        }

        private async Task<User?> GetUserById(int userId)
        {
            try
            {
                using var db = new Data.DatabaseService();
                return await db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Id = @Id", new { Id = userId });
            }
            catch { return null; }
        }

        private async Task LogActivity(int userId, string activityType, string description)
        {
            try
            {
                using var db = new Data.DatabaseService();
                await db.ExecuteAsync(
                    @"INSERT INTO ActivityLogs (UserId, ActivityType, Description, Timestamp)
                      VALUES (@UserId, @ActivityType, @Description, @Timestamp)",
                    new { UserId = userId, ActivityType = activityType, Description = description, Timestamp = DateTime.UtcNow });
            }
            catch { }
        }
    }
}