using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    /// <summary>
    /// Low-level global keyboard hook that can operate in two modes:
    /// 1. Normal mode: passes all keys through, fires KeyDown event.
    /// 2. Lockdown mode: swallows dangerous system keys (Alt+Tab, Win, Ctrl+Esc, Alt+F4, Alt+Esc)
    ///    to prevent students from escaping the lock screen. Still fires KeyDown for the 
    ///    HiddenInputService to detect the admin's secret combo.
    /// </summary>
    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        /// <summary>
        /// When true, dangerous system key combinations are swallowed.
        /// </summary>
        public bool IsLockdownActive { get; set; }

        public event KeyEventHandler KeyDown;

        public GlobalKeyboardHook()
        {
            _proc = HookCallback;
        }

        public void Hook()
        {
            _hookID = SetHook(_proc);
        }

        public void Unhook()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Unhook();
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;

                // Always fire KeyDown so the HiddenInputService can detect the admin combo
                KeyDown?.Invoke(this, new KeyEventArgs(key));

                // If lockdown is active, block dangerous key combos
                if (IsLockdownActive)
                {
                    if (ShouldBlockKey(key, wParam))
                    {
                        // Swallow the key — do NOT pass it to the next hook
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Determines whether a key press should be blocked during lockdown.
        /// </summary>
        private bool ShouldBlockKey(Keys key, IntPtr wParam)
        {
            bool alt = (Control.ModifierKeys & Keys.Alt) != 0;
            bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;

            // Block Windows key (LWin / RWin)
            if (key == Keys.LWin || key == Keys.RWin)
                return true;

            // Block Alt+Tab (task switcher)
            if (alt && key == Keys.Tab)
                return true;

            // Block Alt+F4 (close window)
            if (alt && key == Keys.F4)
                return true;

            // Block Alt+Esc (cycle windows)
            if (alt && key == Keys.Escape)
                return true;

            // Block Ctrl+Esc (Start menu)
            if (ctrl && key == Keys.Escape)
                return true;

            // Block Alt+Space (system menu)
            if (alt && key == Keys.Space)
                return true;

            // Block F11 (fullscreen toggle in browsers)
            if (key == Keys.F11)
                return true;

            return false;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}