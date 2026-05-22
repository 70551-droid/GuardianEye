using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Services;
using System;
using System.Windows;

namespace GuardianEye.ViewModels
{
    public partial class RequestMoreTimeViewModel : ObservableObject
    {
        private readonly ITimeRequestService _timeRequestService;

        [ObservableProperty]
        private string _minutes = "";

        [ObservableProperty]
        private string _reason = "";

        public int StudentId { get; set; }

        public RequestMoreTimeViewModel(ITimeRequestService timeRequestService)
        {
            _timeRequestService = timeRequestService;
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(Minutes) || !int.TryParse(Minutes, out var mins) || mins <= 0)
            {
                MessageBox.Show("Please enter a valid number of minutes.", "Invalid Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (mins > 120)
            {
                MessageBox.Show("Maximum request is 120 minutes.", "Too High",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Please provide a reason for your request.", "Reason Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await _timeRequestService.SubmitRequestAsync(StudentId, mins, Reason.Trim());

            if (success)
            {
                MessageBox.Show("Your time request has been submitted for review.", "Request Submitted",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            else
            {
                MessageBox.Show("Failed to submit request. Please try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            foreach (var w in Application.Current.Windows)
            {
                if (w is Window window && window.DataContext == this)
                {
                    window.Close();
                    return;
                }
            }
        }
    }
}