using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Data;
using GuardianEye.Helpers;
using GuardianEye.Models;
using GuardianEye.Services;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace GuardianEye.ViewModels
{
    public partial class StudentDashboardViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly IDatabaseService? _db;
        private readonly IActivityLogService? _activityLogService;
        private readonly User _currentUser;
        private readonly DispatcherTimer? _timer;

        public int UserId => _currentUser.Id;

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                var sessions = await _sessionService.GetUserSessionsAsync(_currentUser.Id);
                var activeSession = sessions.FirstOrDefault(s => s.Status == "Active");
                if (activeSession != null)
                {
                    await _sessionService.EndSessionAsync(activeSession.Id);
                }

                await _sessionService.EndSessionAsync(_currentUser.Id);
                await _authService.LogoutAsync(_currentUser.Id);
                await _db!.ExecuteAsync(
                    "UPDATE Users SET IsLoggedIn = 0, SessionEndTime = NULL WHERE Id = @Id",
                    new { _currentUser.Id });
                Helpers.Logging.Info($"Student {_currentUser.Username} logged out manually");
            }
            catch (Exception ex)
            {
                Helpers.Logging.Error("Error during student logout", ex);
            }
        }

        [RelayCommand]
        private async Task ForceLogoutAsync()
        {
            await LogoutAsync();
        }

        [ObservableProperty]
        private string _welcomeMessage = "";

        [ObservableProperty]
        private string _remainingTimeText = "";

        [ObservableProperty]
        private string _sessionStatus = "Session Active";

        [ObservableProperty]
        private string _sessionsUsedText = "";

        [ObservableProperty]
        private int _remainingMinutes = 0;

        [ObservableProperty]
        private int _remainingSeconds = 0;

        [ObservableProperty]
        private bool _isSessionActive = false;

        [ObservableProperty]
        private bool _showWarning = false;

        [ObservableProperty]
        private string _warningMessage = "";

        [ObservableProperty]
        private Brush _timeDisplayColor = Brushes.White;

        public StudentDashboardViewModel(IAuthService authService, ISessionService sessionService,
            IDatabaseService db, IActivityLogService activityLogService, User currentUser)
        {
            _authService = authService;
            _sessionService = sessionService;
            _db = db;
            _activityLogService = activityLogService;
            _currentUser = currentUser;

            WelcomeMessage = $"Welcome, {_currentUser.FullName}";
            IsSessionActive = _currentUser.IsLoggedIn;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_currentUser.SessionEndTime.HasValue)
            {
                var remaining = _currentUser.SessionEndTime.Value - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 0)
                {
                    _timer!.Stop();
                    IsSessionActive = false;
                    RemainingTimeText = "SESSION EXPIRED";
                    SessionStatus = "Session Expired";
                    TimeDisplayColor = Brushes.Red;
                }
                else
                {
                    UpdateRemainingTime(remaining);
                }
            }
        }

        private void UpdateRemainingTime(TimeSpan remaining)
        {
            RemainingMinutes = (int)remaining.TotalMinutes;
            RemainingSeconds = remaining.Seconds;
            RemainingTimeText = $"{RemainingMinutes:D2}:{RemainingSeconds:D2}";
            SessionsUsedText = $"Sessions Used: {_currentUser.SessionsUsedToday} / {_currentUser.MaxDailySessions}";

            var totalSeconds = (int)remaining.TotalSeconds;

            if (totalSeconds <= 10)
            {
                ShowWarning = true;
                WarningMessage = "⚠️  SESSION ENDING SOON!";
                TimeDisplayColor = Brushes.Red;

                if (totalSeconds % 2 == 0)
                    RemainingTimeText = $"⚠️ {RemainingTimeText}";
            }
            else if (totalSeconds <= 60)
            {
                ShowWarning = true;
                WarningMessage = "⚠️  Less than 1 minute remaining";
                TimeDisplayColor = new SolidColorBrush(Color.FromRgb(255, 165, 0));
            }
            else if (totalSeconds <= 300)
            {
                ShowWarning = false;
                TimeDisplayColor = new SolidColorBrush(Color.FromRgb(255, 215, 0));
            }
            else
            {
                ShowWarning = false;
                TimeDisplayColor = Brushes.White;
            }
        }
    }
}