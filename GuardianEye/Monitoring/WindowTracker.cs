using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GuardianEye.Monitoring
{
    public interface IWindowTracker
    {
        Task<string> GetActiveWindowTitleAsync();
        Task<string> GetActiveProcessNameAsync();
        event EventHandler<WindowChangedEventArgs>? WindowChanged;
        void StartTracking();
        void StopTracking();
    }

    public class WindowChangedEventArgs : EventArgs
    {
        public string? WindowTitle { get; set; }
        public string? ProcessName { get; set; }
        public int ProcessId { get; set; }
    }

    public class WindowTracker : IWindowTracker
    {
        private Timer? _timer;
        private string? _lastWindowTitle;
        private string? _lastProcessName;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder str, int maxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public event EventHandler<WindowChangedEventArgs>? WindowChanged;

        public async Task<string> GetActiveWindowTitleAsync()
        {
            try
            {
                var hWnd = GetForegroundWindow();
                var length = GetWindowTextLength(hWnd);
                var sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<string> GetActiveProcessNameAsync()
        {
            try
            {
                var hWnd = GetForegroundWindow();
                GetWindowThreadProcessId(hWnd, out var processId);
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void StartTracking()
        {
            _timer = new Timer(CheckWindowChange, null, 0, 1000);
        }

        public void StopTracking()
        {
            _timer?.Dispose();
        }

        private async void CheckWindowChange(object? state)
        {
            var title = await GetActiveWindowTitleAsync();
            var processName = await GetActiveProcessNameAsync();

            if (title != _lastWindowTitle || processName != _lastProcessName)
            {
                _lastWindowTitle = title;
                _lastProcessName = processName;
                WindowChanged?.Invoke(this, new WindowChangedEventArgs
                {
                    WindowTitle = title,
                    ProcessName = processName
                });
            }
        }
    }
}