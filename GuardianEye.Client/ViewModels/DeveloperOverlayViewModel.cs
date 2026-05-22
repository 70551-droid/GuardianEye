using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GuardianEye.ViewModels
{
    public partial class DeveloperOverlayViewModel : ObservableObject
    {
        private readonly IDeveloperOverrideService _overrideService;
        private Timer? _inactivityTimer;
        private DateTime _lastActivity = DateTime.UtcNow;
        private const int InactivityTimeoutSeconds = 10;
        private bool _isClosing = false;

        [ObservableProperty]
        private string _statusMessage = "Developer Override Active";

        [ObservableProperty]
        private bool _isVisible = true;

        public DeveloperOverlayViewModel(IDeveloperOverrideService overrideService)
        {
            _overrideService = overrideService;
            _inactivityTimer = new Timer(CheckInactivity, null, 1000, 1000);
        }

        [RelayCommand]
        private async Task ExtendSession()
        {
            if (_isClosing) return;
            ResetInactivity();
            try
            {
                await _overrideService.ExtendSessionAsync(15);
                StatusMessage = "Session extended by 15 minutes";
            }
            catch
            {
                StatusMessage = "Error extending session";
            }
        }

        [RelayCommand]
        private async Task AddSession()
        {
            if (_isClosing) return;
            ResetInactivity();
            try
            {
                await _overrideService.AddSessionAsync();
                StatusMessage = "Session count incremented";
            }
            catch
            {
                StatusMessage = "Error adding session";
            }
        }

        [RelayCommand]
        private async Task PauseEnforcement()
        {
            if (_isClosing) return;
            ResetInactivity();
            try
            {
                await _overrideService.PauseEnforcementAsync(TimeSpan.FromMinutes(5));
                StatusMessage = "Enforcement paused for 5 minutes";
            }
            catch
            {
                StatusMessage = "Error pausing enforcement";
            }
        }

        [RelayCommand]
        private async Task UnlockScreen()
        {
            if (_isClosing) return;
            ResetInactivity();
            try
            {
                await _overrideService.UnlockScreenAsync();
                StatusMessage = "Screen unlocked";
            }
            catch
            {
                StatusMessage = "Error unlocking screen";
            }
        }

        [RelayCommand]
        private void CloseOverlay()
        {
            if (_isClosing) return;
            _isClosing = true;
            IsVisible = false;
            _inactivityTimer?.Dispose();
            _inactivityTimer = null;
        }

        public void HandleActivity()
        {
            if (!_isClosing)
                ResetInactivity();
        }

        private void ResetInactivity()
        {
            _lastActivity = DateTime.UtcNow;
        }

        private void CheckInactivity(object? state)
        {
            try
            {
                if (DateTime.UtcNow.Subtract(_lastActivity).TotalSeconds >= InactivityTimeoutSeconds)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_isClosing)
                        {
                            IsVisible = false;
                            _inactivityTimer?.Dispose();
                            _inactivityTimer = null;
                        }
                    }));
                }
            }
            catch
            {
                // Ignore timer exceptions
            }
        }

        public void Dispose()
        {
            _inactivityTimer?.Dispose();
            _inactivityTimer = null;
        }
    }
}