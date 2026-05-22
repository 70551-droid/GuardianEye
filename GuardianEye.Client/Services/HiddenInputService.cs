using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GuardianEye.Services
{
    public interface IHiddenInputService
    {
        event Action<string>? SequenceEntered;
        void StartListening();
        void StopListening();
        void Initialize();
    }

    public class HiddenInputService : IHiddenInputService, IDisposable
    {
        private readonly LowLevelKeyboardListener _listener;
        private bool _isListening = false;
        private DateTime _listenStartTime;
        private const int ListenTimeoutMs = 5000;
        private string _currentSequence = "";
        private const string SecretHash = "9a294fed8fa665300473aa3fb09a5b87014082c094e0ec4059b6234419ce63b5"; // SHA256 of secret phrase
        private const int SecretLength = 15; // Length of expected secret for early termination

        public event Action<string>? SequenceEntered;

        public HiddenInputService()
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += OnKeyPressed;
            _listener.ActivationComboPressed += () => StartListening();
        }

        public void Initialize()
        {
        }

        public void StartListening()
        {
            _isListening = true;
            _listenStartTime = DateTime.UtcNow;
            _currentSequence = "";
        }

        public void StopListening()
        {
            _isListening = false;
            _currentSequence = "";
        }

        private void OnKeyPressed(object? sender, char keyChar)
        {
            if (!_isListening) return;

            var elapsed = DateTime.UtcNow.Subtract(_listenStartTime).TotalMilliseconds;
            if (elapsed > ListenTimeoutMs)
            {
                StopListening();
                return;
            }

            if (keyChar == '\b')
            {
                if (_currentSequence.Length > 0)
                    _currentSequence = _currentSequence[..^1];
                return;
            }

            if (keyChar == '\u001B') // ESC - cancel listening mode
            {
                StopListening();
                return;
            }

            if (char.IsLetterOrDigit(keyChar) || keyChar == '_')
            {
                _currentSequence += keyChar.ToString().ToLowerInvariant();
                
                // Early termination: if sequence is longer than secret, reset
                if (_currentSequence.Length > SecretLength)
                {
                    _currentSequence = _currentSequence[^SecretLength..];
                }

                if (ComputeHash(_currentSequence) == SecretHash)
                {
                    StopListening();
                    SequenceEntered?.Invoke(_currentSequence);
                }
            }
        }

        private static string ComputeHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input.ToLowerInvariant());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }

        private class LowLevelKeyboardListener : IDisposable
        {
            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            private readonly IntPtr _hookId = IntPtr.Zero;
            private readonly LowLevelKeyboardProc _proc;
            private bool _disposed;

            public event EventHandler<char>? OnKeyPressed;
            public event Action? ActivationComboPressed;

            public LowLevelKeyboardListener()
            {
                _proc = HookCallback;
                try
                {
                    using var curProcess = Process.GetCurrentProcess();
                    using var curModule = curProcess.MainModule;
                    if (curModule != null)
                        _hookId = SetHook(_proc);
                }
                catch
                {
                    // Hook setup failed - service will be non-functional but won't crash
                }
            }

            private IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                return curModule != null 
                    ? SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0)
                    : IntPtr.Zero;
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                try
                {
                    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                    {
                        int vkCode = Marshal.ReadInt32(lParam);
                        
                        bool isCtrl = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
                        bool isShift = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                        bool isAlt = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
                        bool isWin = (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;
                        
                        if (isCtrl && isShift && isAlt && isWin && vkCode == VK_MULTIPLY)
                        {
                            ActivationComboPressed?.Invoke();
                        }
                        
                        if (vkCode >= 65 && vkCode <= 90)
                        {
                            bool isShiftPressed = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                            char keyChar = (char)(isShiftPressed ? vkCode : vkCode + 32);
                            OnKeyPressed?.Invoke(this, keyChar);
                        }
                        else if (vkCode >= 48 && vkCode <= 57)
                        {
                            OnKeyPressed?.Invoke(this, (char)vkCode);
                        }
                        else if (vkCode == VK_BACK)
                        {
                            OnKeyPressed?.Invoke(this, '\b');
                        }
                    }
                }
                catch
                {
                    // Suppress exceptions to keep hook alive
                }

                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            public void Dispose()
            {
                if (!_disposed && _hookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookId);
                    _disposed = true;
                }
            }

            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int VK_SHIFT = 0x10;
            private const int VK_CONTROL = 0x11;
            private const int VK_MENU = 0x12;
            private const int VK_LWIN = 0x5B;
            private const int VK_RWIN = 0x5C;
            private const int VK_BACK = 0x08;
            private const int VK_MULTIPLY = 0x6A; // Keypad Multiply

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc lpfn, 
                IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, 
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string? lpModuleName);

            [DllImport("user32.dll")]
            private static extern short GetAsyncKeyState(int vKey);
        }
    }
}