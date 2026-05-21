using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Services;
using System;
using System.Threading;

namespace GuardianEye.ViewModels
{
    public partial class DeveloperOverlayViewModel : ObservableObject
    {
        private readonly IDeveloperOverrideService _overrideService;
        private Timer? _inactivityTimer;
        private DateTime _lastActivity = DateTime.UtcNow;
        private const int InactivityTimeoutSeconds = 10;

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
        private async void ExtendSession()
        {
            ResetInactivity();
            await _overrideService.ExtendSessionAsync(15);
            StatusMessage = "Session extended by 15 minutes";
        }

        [RelayCommand]
        private async void AddSession()
        {
            ResetInactivity();
            await _overrideService.AddSessionAsync();
            StatusMessage = "Session count incremented";
        }

        [RelayCommand]
        private async void PauseEnforcement()
        {
            ResetInactivity();
            await _overrideService.PauseEnforcementAsync(TimeSpan.FromMinutes(5));
            StatusMessage = "Enforcement paused for 5 minutes";
        }

        [RelayCommand]
        private async void UnlockScreen()
        {
            ResetInactivity();
            await _overrideService.UnlockScreenAsync();
            StatusMessage = "Screen unlocked";
        }

        [RelayCommand]
        private void CloseOverlay()
        {
            IsVisible = false;
            _inactivityTimer?.Stop();
        }

        public void HandleActivity()
        {
            ResetInactivity();
        }

        private void ResetInactivity()
        {
            _lastActivity = DateTime.UtcNow;
        }

        private void CheckInactivity(object? state)
        {
            if (DateTime.UtcNow.Subtract(_lastActivity).TotalSeconds >= InactivityTimeoutSeconds)
            {
                IsVisible = false;
                _inactivityTimer?.Dispose();
                _inactivityTimer = null;
            }
        }

        public void Dispose()
        {
            _inactivityTimer?.Dispose();
        }
    }
}