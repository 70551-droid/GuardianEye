using GuardianEye.Admin.Services.Api;
using GuardianEye.Shared.Dtos;
using System.Windows;

namespace GuardianEye.Admin.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAdminApiService _adminApiService;

        public LoginWindow(IAdminApiService adminApiService)
        {
            InitializeComponent();
            _adminApiService = adminApiService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            var username = UsernameTextBox.Text.Trim();
            var password = new System.Net.NetworkCredential(string.Empty, PasswordBox.Password).Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Please enter both username and password";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            var loginRequest = new AdminLoginRequestDto
            {
                Username = username,
                Password = password
            };

            try
            {
                var response = await _adminApiService.AdminLoginAsync(loginRequest);

                if (response.Success && response.User != null && response.User.Role == "Admin")
                {
                    // Navigate to dashboard
                    var dashboardShell = new DashboardShell();
                    dashboardShell.Show();
                    this.Close();
                }
                else
                {
                    ErrorTextBlock.Text = response.Message ?? "Invalid credentials or insufficient permissions";
                    ErrorTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (System.Exception ex)
            {
                ErrorTextBlock.Text = "Login failed: " + ex.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}