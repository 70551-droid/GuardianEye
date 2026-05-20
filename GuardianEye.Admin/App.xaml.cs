using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GuardianEye.Admin.Services.Api;
using GuardianEye.Admin.ViewModels;
using GuardianEye.Admin.Views;
using System.Windows;

namespace GuardianEye.Admin
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add HTTP client for API communication
                    services.AddHttpClient<IAdminApiService, AdminApiService>();

                    // Add ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<DashboardViewModel>();

                    // Add Views
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<DashboardShell>();
                })
                .Build();

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}