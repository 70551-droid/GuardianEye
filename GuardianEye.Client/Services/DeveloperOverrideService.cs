using GuardianEye.Data;
using GuardianEye.Helpers;
using GuardianEye.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuardianEye.Services
{
    public interface IDeveloperOverrideService
    {
        event Action<int>? SessionExtended;
        event Action? EnforcementPaused;
        event Action? EnforcementResumed;
        event Action? ScreenUnlocked;
        bool IsEnforcementPaused { get; }
        void Initialize(IHiddenInputService hiddenInputService, int currentUserId);
        Task ExtendSessionAsync(int minutes);
        Task AddSessionAsync();
        Task PauseEnforcementAsync(TimeSpan duration);
        Task UnlockScreenAsync();
        string HashSecret(string secret);
    }

    public class DeveloperOverrideService : IDeveloperOverrideService, IDisposable
    {
        private readonly IDatabaseService _db;
        private readonly ISessionService _sessionService;
        private readonly ILockScreenService _lockScreenService;
        private Timer? _pauseTimer;
        private IHiddenInputService? _hiddenInputService;
        private int _currentUserId = 1;

        public bool IsEnforcementPaused { get; private set; } = false;
        public event Action<int>? SessionExtended;
        public event Action? EnforcementPaused;
        public event Action? EnforcementResumed;
        public event Action? ScreenUnlocked;

        public DeveloperOverrideService(IDatabaseService db, ISessionService sessionService, 
            ILockScreenService lockScreenService)
        {
            _db = db;
            _sessionService = sessionService;
            _lockScreenService = lockScreenService;
        }

        public void Initialize(IHiddenInputService hiddenInputService, int currentUserId)
        {
            _hiddenInputService = hiddenInputService;
            _currentUserId = currentUserId;
        }

        public async Task ExtendSessionAsync(int minutes)
        {
            try
            {
                var user = await _db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Id = @Id", new { Id = _currentUserId });

                if (user?.SessionEndTime.HasValue == true)
                {
                    var newEndTime = user.SessionEndTime.Value.AddMinutes(minutes);
                    await _db.ExecuteAsync(
                        "UPDATE Users SET SessionEndTime = @SessionEndTime WHERE Id = @Id",
                        new { SessionEndTime = newEndTime, Id = _currentUserId });

                    Logging.Info($"Developer override: Extended session by {minutes} minutes");
                    SessionExtended?.Invoke(minutes);
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Error extending session via override", ex);
            }
        }

        public async Task AddSessionAsync()
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    "UPDATE Users SET SessionsUsedToday = SessionsUsedToday - 1, MaxDailySessions = MaxDailySessions + 1 WHERE Id = @Id",
                    new { Id = _currentUserId });

                Logging.Info("Developer override: Added extra session");
            }
            catch (Exception ex)
            {
                Logging.Error("Error adding session via override", ex);
            }
        }

        public async Task PauseEnforcementAsync(TimeSpan duration)
        {
            IsEnforcementPaused = true;
            _pauseTimer = new Timer(_ => ResumeEnforcement(), null, 
                (int)duration.TotalMilliseconds, Timeout.Infinite);
            
            Logging.Info($"Developer override: Enforcement paused for {duration.TotalMinutes} minutes");
            EnforcementPaused?.Invoke();
        }

        private void ResumeEnforcement()
        {
            IsEnforcementPaused = false;
            _pauseTimer?.Dispose();
            _pauseTimer = null;
            Logging.Info("Developer override: Enforcement resumed");
            EnforcementResumed?.Invoke();
        }

        public async Task UnlockScreenAsync()
        {
            try
            {
                _lockScreenService.CloseLockScreen();
                Logging.Info("Developer override: Screen unlocked");
                ScreenUnlocked?.Invoke();
            }
            catch (Exception ex)
            {
                Logging.Error("Error unlocking screen via override", ex);
            }
        }

        public string HashSecret(string secret)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(secret.ToLowerInvariant());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public void Dispose()
        {
            _pauseTimer?.Dispose();
        }
    }
}