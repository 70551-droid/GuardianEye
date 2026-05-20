using GuardianEye.Admin.Services.Api;
using GuardianEye.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;

namespace GuardianEye.Admin.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAdminApiService _adminApiService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoggingIn;

        public LoginViewModel(IAdminApiService adminApiService)
        {
            _adminApiService = adminApiService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsLoggingIn) return;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password";
                return;
            }

            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {
                var loginRequest = new AdminLoginRequestDto
                {
                    Username = Username.Trim(),
                    Password = Password
                };

                var response = await _adminApiService.AdminLoginAsync(loginRequest);

                if (response.Success && response.User != null && response.User.Role == "Admin")
                {
                    // Navigate to dashboard (we'll need to implement this in the view)
                    var loginWindow = Application.Current.Windows.OfType<Views.LoginWindow>().FirstOrDefault();
                    if (loginWindow != null)
                    {
                        var dashboardShell = new Views.DashboardShell();
                        dashboardShell.Show();
                        loginWindow.Close();
                    }
                }
                else
                {
                    ErrorMessage = response.Message ?? "Invalid credentials or insufficient permissions";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Login failed: " + ex.Message;
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}