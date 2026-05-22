using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Data;
using GuardianEye.Helpers;
using GuardianEye.Models;
using GuardianEye.Services;
using GuardianEye.Views;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IDatabaseService _db;
        private readonly ILockScreenService _lockScreenService;
        private readonly IServiceProvider _serviceProvider;
        private readonly User _currentUser;
        private readonly DispatcherTimer _timer;

        public int UserId => _currentUser.Id;

        [ObservableProperty]
        private string _remainingTimeText = "";

        [ObservableProperty]
        private string _sessionsUsedText = "";

        [ObservableProperty]
        private Brush _timeDisplayColor = Brushes.White;

        public StudentDashboardViewModel(
            IAuthService authService,
            ISessionService sessionService,
            IDatabaseService db,
            ILockScreenService lockScreenService,
            IServiceProvider serviceProvider,
            User currentUser)
        {
            _authService = authService;
            _sessionService = sessionService;
            _db = db;
            _lockScreenService = lockScreenService;
            _serviceProvider = serviceProvider;
            _currentUser = currentUser;

            SessionsUsedText = $"{_currentUser.SessionsUsedToday} / {_currentUser.MaxDailySessions}";

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

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

                await _db.ExecuteAsync(
                    "UPDATE Users SET IsLoggedIn = 0, SessionEndTime = NULL WHERE Id = @Id",
                    new { _currentUser.Id });

                _timer.Stop();

                var window = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this);
                window?.Close();

                await _lockScreenService.LaunchLockScreenAsync(_currentUser.Id, "User logged out");
            }
            catch (Exception ex)
            {
                Logging.Error("Error during logout", ex);
            }
        }

        [RelayCommand]
        private void RequestMoreTime()
        {
            var requestWindow = _serviceProvider.GetRequiredService<RequestMoreTimeWindow>();
            if (requestWindow.DataContext is RequestMoreTimeViewModel vm)
            {
                vm.StudentId = _currentUser.Id;
            }
            requestWindow.Owner = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            requestWindow.ShowDialog();
        }

        public void RefreshSessionsUsedText()
        {
            SessionsUsedText = $"{_currentUser.SessionsUsedToday} / {_currentUser.MaxDailySessions}";
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_currentUser.SessionEndTime.HasValue)
            {
                var remaining = _currentUser.SessionEndTime.Value - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 0)
                {
                    _timer.Stop();
                    RemainingTimeText = "00:00";
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
            RemainingTimeText = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
            SessionsUsedText = $"{_currentUser.SessionsUsedToday} / {_currentUser.MaxDailySessions}";

            var totalSeconds = (int)remaining.TotalSeconds;

            if (totalSeconds <= 60)
                TimeDisplayColor = Brushes.Red;
            else if (totalSeconds <= 300)
                TimeDisplayColor = new SolidColorBrush(Color.FromRgb(255, 200, 50));
            else
                TimeDisplayColor = Brushes.White;
        }
    }
}