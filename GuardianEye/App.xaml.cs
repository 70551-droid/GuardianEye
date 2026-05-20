using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GuardianEye.Data;
using GuardianEye.Services;
using GuardianEye.ViewModels;
using GuardianEye.Views;
using System.Windows;

namespace GuardianEye
{
    public partial class App : Application
    {
        internal IHost? _host;
        private SessionEnforcementService? _enforcement;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Helpers.AppPaths.EnsureDirectories();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDatabaseService, DatabaseService>();
                    services.AddSingleton<IAuthService, AuthService>();
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<ISessionService, SessionService>();
                    services.AddSingleton<IDeviceService, DeviceService>();
                    services.AddSingleton<IActivityLogService, ActivityLogService>();
                    services.AddSingleton<ILockScreenService, LockScreenService>();
                    services.AddSingleton<SessionEnforcementService>();
                    
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<IThemeService, ThemeService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<AdminDashboardViewModel>();
                    services.AddTransient<StudentDashboardViewModel>();

                    services.AddTransient<LoginWindow>();
                    services.AddTransient<AdminDashboardView>();
                    services.AddTransient<StudentDashboardView>();
                })
                .Build();

            _enforcement = _host.Services.GetRequiredService<SessionEnforcementService>();
            _enforcement.StartEnforcement();

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _enforcement?.StopEnforcement();
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}