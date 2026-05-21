using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GuardianEye.Data;
using GuardianEye.Services;
using GuardianEye.ViewModels;
using GuardianEye.Views;
using GuardianEye.Client.Services.Api;
using System.Windows;
using System.Windows.Input;

namespace GuardianEye
{
    public partial class App : Application
    {
        internal IHost? _host;
        private SessionEnforcementService? _enforcement;
        private IHiddenInputService? _hiddenInputService;
        private DeveloperOverlayWindow? _overlayWindow;

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
                    
                    services.AddSingleton<IHiddenInputService, HiddenInputService>();
                    services.AddSingleton<IDeveloperOverrideService, DeveloperOverrideService>();
                    services.AddTransient<DeveloperOverlayViewModel>();
                    services.AddTransient<DeveloperOverlayWindow>();
                    
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<IThemeService, ThemeService>();

                    services.AddHttpClient<IAuthApiService, AuthApiService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<AdminDashboardViewModel>();
                    services.AddTransient<StudentDashboardViewModel>();

                    services.AddTransient<LoginWindow>();
                    services.AddTransient<AdminDashboardView>();
                    services.AddTransient<StudentDashboardView>();
                })
                .Build();

            // Initialize hidden input service
            _hiddenInputService = _host.Services.GetRequiredService<IHiddenInputService>();
            _hiddenInputService.SequenceEntered += OnHiddenSequenceEntered;

            // Initialize developer override service
            var overrideService = _host.Services.GetRequiredService<IDeveloperOverrideService>();
            overrideService.OverlayRequested += OnOverlayRequested;

            _enforcement = _host.Services.GetRequiredService<SessionEnforcementService>();
            _enforcement.StartEnforcement();

            // Setup activation hotkey: Ctrl + Win + Shift + Alt + *
            this.PreviewKeyDown += App_PreviewKeyDown;

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void App_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Win + Shift + Alt + * (OemAsterisk or D8 with Shift depending on keyboard)
            bool isCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool isWin = (Keyboard.GetKeyStates(Key.LeftWindows).IsKeyDown() || 
                          Keyboard.GetKeyStates(Key.RightWindows).IsKeyDown());
            bool isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            bool isAlt = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
            bool isAsterisk = e.Key == Key.OemAsterisk || e.Key == Key.D8;

            if (isCtrl && isWin && isShift && isAlt && isAsterisk)
            {
                _hiddenInputService?.StartListening();
                e.Handled = true;
            }
        }

        private void OnHiddenSequenceEntered(string sequence)
        {
            if (sequence?.ToLowerInvariant() == "guardianoverride")
            {
                OnOverlayRequested();
            }
        }

        private void OnOverlayRequested()
        {
            if (_overlayWindow == null)
            {
                _overlayWindow = _host?.Services.GetRequiredService<DeveloperOverlayWindow>();
                _overlayWindow.Closed += (s, e) => _overlayWindow = null;
            }

            if (!_overlayWindow.IsVisible)
            {
                _overlayWindow.Show();
                _overlayWindow.Activate();
                _overlayWindow.Focus();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _enforcement?.StopEnforcement();
            (_hiddenInputService as IDisposable)?.Dispose();
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}