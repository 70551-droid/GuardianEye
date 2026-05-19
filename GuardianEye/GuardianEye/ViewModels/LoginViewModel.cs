using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuardianEye.Models;
using GuardianEye.Services;
using System.Windows;

namespace GuardianEye.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _rememberMe = false;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password";
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var result = await _authService.LoginAsync(Username, Password);

                if (result.Success)
                {
                    if (result.User?.Role == "Admin")
                    {
                        _navigationService.NavigateTo<AdminDashboardViewModel>();
                    }
                    else
                    {
                        _navigationService.NavigateTo<StudentDashboardViewModel>();
                    }
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Login failed";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login";
                Helpers.Logging.Error("Login error", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}