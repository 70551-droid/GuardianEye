using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using GuardianEye.Helpers;
using GuardianEye.Models;
using GuardianEye.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GuardianEye.Views
{
    public partial class LoginWindow : Window
    {
        private IAuthService? _authService;
        private SessionEnforcementService? _enforcementService;

        public int? UserId { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var host = (Application.Current as App)?._host;
            if (host == null) return;

            _authService = host.Services.GetService<IAuthService>();
            var sessionService = host.Services.GetService<ISessionService>();
            var activityService = host.Services.GetService<IActivityLogService>();
            var monitorService = host.Services.GetService<ILockScreenService>();
            var db = host.Services.GetService<IDatabaseService>();

            if (_authService != null && sessionService != null && activityService != null && monitorService != null && db != null)
            {
                _enforcementService = new SessionEnforcementService(db, sessionService, activityService, monitorService);
                _enforcementService.StartEnforcement();
            }

            RefreshSessionDisplay();
        }

        private async void RefreshSessionDisplay()
        {
            if (_authService == null) return;
            await Dispatcher.InvokeAsync(async () =>
            {
                var users = await _authService.GetAllUsersAsync();
                var loggedInCount = users.Count(u => u.IsLoggedIn);
                SessionStatusText.Text = $"Active sessions: {loggedInCount}";
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CppUnusedHandleMemberLocal")]
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source == null) return;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0112) // WM_SYSCOMMAND
            {
                int cmd = wParam.ToInt32() & 0xFFF0;
                if (cmd == 0xF100 || // SC_SIZE
                    cmd == 0xF000 || // SC_MOVE
                    cmd == 0xF010 || // SC_MINIMIZE
                    cmd == 0xF020 || // SC_MAXIMIZE
                    cmd == 0xF030 || // SC_RESTORE
                    cmd == 0xF060)   // SC_CLOSE
                {
                    if (IsLoginWindowActive())
                    {
                        handled = true;
                        return IntPtr.Zero;
                    }
                }
            }
            return IntPtr.Zero;
        }

        private bool IsLoginWindowActive()
        {
            return IsActive || WindowState == WindowState.Normal || WindowState == WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}