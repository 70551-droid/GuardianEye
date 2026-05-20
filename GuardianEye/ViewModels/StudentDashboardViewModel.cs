using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Models;
using GuardianEye.Services;
using System;
using System.Windows.Threading;

namespace GuardianEye.ViewModels
{
    public partial class StudentDashboardViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly User _currentUser;
        private DispatcherTimer? _timer;

        [ObservableProperty]
        private string _welcomeMessage = "";

        [ObservableProperty]
        private string _remainingTimeText = "";

        [ObservableProperty]
        private string _sessionsUsedText = "";

        [ObservableProperty]
        private int _remainingMinutes = 0;

        [ObservableProperty]
        private int _remainingSeconds = 0;

        [ObservableProperty]
        private bool _isSessionActive = false;

        public StudentDashboardViewModel(IAuthService authService, ISessionService sessionService, User currentUser)
        {
            _authService = authService;
            _sessionService = sessionService;
            _currentUser = currentUser;

            WelcomeMessage = $"Welcome, {_currentUser.FullName}";
            IsSessionActive = _currentUser.IsLoggedIn;

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            if (_currentUser.SessionEndTime.HasValue)
            {
                UpdateRemainingTime();
            }
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
                    RemainingTimeText = "Session Expired";
                }
                else
                {
                    UpdateRemainingTime();
                }
            }
        }

        private void UpdateRemainingTime()
        {
            if (_currentUser.SessionEndTime.HasValue)
            {
                var remaining = _currentUser.SessionEndTime.Value - DateTime.UtcNow;
                if (remaining.TotalSeconds > 0)
                {
                    RemainingMinutes = (int)remaining.TotalMinutes;
                    RemainingSeconds = remaining.Seconds;
                    RemainingTimeText = $"Time Remaining: {RemainingMinutes:D2}:{RemainingSeconds:D2}";
                    SessionsUsedText = $"Sessions Used Today: {_currentUser.SessionsUsedToday} / {_currentUser.MaxDailySessions}";
                }
                else
                {
                    RemainingTimeText = "Session Expired";
                }
            }
        }
    }
}